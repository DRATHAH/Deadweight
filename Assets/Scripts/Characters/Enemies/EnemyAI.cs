using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : DamageableCharacter
{
    [Header("Enemy Stats")]
    public float attackRange = 2f;
    public float attackRate = 1f;
    public float knockback = 10f;
    public float turnSpeed = 300f;
    [Header("References")]
    public Transform attackTarget;
    public NavMeshAgent agent;
    public ConfigurableJoint mainJoint;
    public SphereCollider mainCol;

    float timeSinceAttack = 0f;
    Quaternion mainJointTargetRotation;
    SyncLimbs[] limbs;
    float startSlerpPosSpring = 0;
    Vector3 startColPos = Vector3.zero;

    List<Transform> players = new List<Transform>();
    NetworkVariable<bool> canMove = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    NetworkVariable<bool> canAttack = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        GameManager.instance.OnSpawnCPUs += Gamemager_OnSpawnCPUs;
        limbs = GetComponentsInChildren<SyncLimbs>();
        startColPos = mainCol.center;
        startSlerpPosSpring = mainJoint.slerpDrive.positionSpring;
    }

    private void Gamemager_OnSpawnCPUs(object sender, System.EventArgs e)
    {
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Transform player = client.PlayerObject.transform;
            players.Add(player);
        }

        foreach (EnemyAI CPU in GameManager.instance.CPUs)
        {
            if (CPU != this)
            {
                players.Add(CPU.transform);
            }
        }
    }

    private void FixedUpdate()
    {
        if (canMove.Value)
        {
            float distanceToPlayer = -1;
            // Get all players connected in the game
            foreach (Transform target in players)
            {
                if (target && (target.position - transform.position).sqrMagnitude <= distanceToPlayer || distanceToPlayer == -1)
                {
                    distanceToPlayer = (target.position - transform.position).sqrMagnitude;
                    attackTarget = target;
                }
            }

            if ((attackTarget.position -  transform.position).magnitude >= attackRange)
            {
                Vector3 direction = (attackTarget.position - transform.position).normalized;
                Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(direction.x * -1, direction.y, direction.z), transform.up);
                mainJointTargetRotation = Quaternion.RotateTowards(mainJointTargetRotation, desiredDirection, Time.fixedDeltaTime * turnSpeed);
                mainJoint.targetRotation = mainJointTargetRotation;

                rb.MovePosition(rb.position + direction * agent.speed * Time.fixedDeltaTime);
            }
            else
            {
                //agent.SetDestination(transform.position);
                agent.updateRotation = false;
                if (canAttack.Value)
                {
                    OnAttack();
                }
            }

            if (timeSinceAttack < attackRate)
            {
                timeSinceAttack += Time.deltaTime;
            }
            else
            {
                canAttack.Value = true;
            }
        }
    }

    void OnAttack()
    {
        if (IsServer)
        {
            canAttack.Value = false;
            timeSinceAttack = 0;
            List<DamageableCharacter> hitTargets = new List<DamageableCharacter>();
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, 2);
            foreach (Collider hit in hits)
            {
                DamageableCharacter character = hit.transform.root.GetComponent<DamageableCharacter>();
                if (character && !hitTargets.Contains(character) && character != GetComponent<DamageableCharacter>())
                {
                    hitTargets.Add(character);
                    Vector3 hitDirection = (character.transform.position - transform.position).normalized;
                    character.OnHit(0, hitDirection * knockback);
                }
            }
        }
    }

    public override IEnumerator Recover()
    {
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = 0;
        mainJoint.slerpDrive = jointDrive;
        mainCol.center = new Vector3(0, 1, 0);

        foreach (SyncLimbs limb in limbs)
        {
            limb.MakeRagdoll();
        }

        yield return new WaitForSeconds(2);
        jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = startSlerpPosSpring;
        mainJoint.slerpDrive = jointDrive;
        foreach (SyncLimbs limb in limbs)
        {
            limb.MakeActiveRagdoll();
        }
        transform.position += new Vector3(0, Mathf.Abs(mainCol.center.y - startColPos.y), 0);
        mainCol.center = startColPos;
    }
}
