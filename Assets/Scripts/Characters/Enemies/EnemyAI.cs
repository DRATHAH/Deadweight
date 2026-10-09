using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : DamageableCharacter
{
    [Header("Enemy Stats")]
    public int attackDmg = 1;
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
    Quaternion startingRotation;
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
        GameManager.instance.OnSpawnCPUs += GameManager_OnSpawnCPUs;
        limbs = GetComponentsInChildren<SyncLimbs>();
        startColPos = mainCol.center;
        startSlerpPosSpring = mainJoint.slerpDrive.positionSpring;
        agent.updateRotation = false;
        agent.updatePosition = false;
        startingRotation = transform.rotation;
    }

    private void GameManager_OnSpawnCPUs(object sender, System.EventArgs e)
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
        if (targetable)
        {
            agent.nextPosition = transform.position;
        }

        if (canMove.Value)
        {
            attackTarget = GetAttackTarget();

            if (attackTarget && (attackTarget.position -  transform.position).magnitude >= attackRange)
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(attackTarget.position, path) && NavMesh.SamplePosition(path.corners[0], out NavMeshHit cornerHit, 1, NavMesh.AllAreas))
                {
                    Vector3[] corners = path.corners;
                    Vector3 direction = (new Vector3(corners[1].x, corners[1].y + 1, corners[1].z) - transform.position).normalized;
                    Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(direction.x * -1, 0, direction.z), transform.up) * startingRotation;
                    mainJointTargetRotation = Quaternion.RotateTowards(mainJointTargetRotation, desiredDirection, Time.fixedDeltaTime * turnSpeed);
                    mainJoint.targetRotation = mainJointTargetRotation;
                    rb.MovePosition(rb.position + direction * agent.speed * Time.fixedDeltaTime);

                }
                else if (NavMesh.SamplePosition(attackTarget.position, out NavMeshHit hit, attackRange, NavMesh.AllAreas))
                {
                    Vector3 direction = (new Vector3(hit.position.x, hit.position.y + 1, hit.position.z) - transform.position).normalized;
                    Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(direction.x * -1, 0, direction.z), transform.up) * startingRotation;
                    mainJointTargetRotation = Quaternion.RotateTowards(mainJointTargetRotation, desiredDirection, Time.fixedDeltaTime * turnSpeed);
                    mainJoint.targetRotation = mainJointTargetRotation;
                    rb.MovePosition(rb.position + direction * agent.speed * Time.fixedDeltaTime);
                }
                else
                {
                    Debug.Log("Enemy can't move there");
                }
            }
            else if (attackTarget)
            {
                Vector3 direction = (attackTarget.position - transform.position).normalized;
                Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(direction.x * -1, 0, direction.z), transform.up) * startingRotation;
                mainJointTargetRotation = Quaternion.RotateTowards(mainJointTargetRotation, desiredDirection, Time.fixedDeltaTime * turnSpeed);
                mainJoint.targetRotation = mainJointTargetRotation;
                if (canAttack.Value)
                {
                    OnAttack();
                }
            }
            else
            {
                Debug.Log("No reachable attack targets");
            }

            if (timeSinceAttack < attackRate)
            {
                timeSinceAttack += Time.deltaTime;
            }
            else if (IsServer)
            {
                canAttack.Value = true;
            }
        }
    }

    private Transform GetAttackTarget()
    {
        Transform target = null;
        float distanceToPlayer = -1;
        // Get all players connected in the game

        foreach (Transform player in players)
        {
            if (player && ((player.position - transform.position).sqrMagnitude <= distanceToPlayer || distanceToPlayer == -1) && player.GetComponent<DamageableCharacter>().targetable)
            {
                distanceToPlayer = (player.position - transform.position).sqrMagnitude;
                NavMeshPath path = new NavMeshPath();
                if ((agent.CalculatePath(player.position, path) && path.status == NavMeshPathStatus.PathComplete) || NavMesh.SamplePosition(player.position, out NavMeshHit hit, attackRange, NavMesh.AllAreas))
                {
                    target = player;
                }
            }
        }

        return target;
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
                    character.OnHit(attackDmg, hitDirection * knockback);
                }
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public override void OnHitClientRpc(int newHealth)
    {
        Health = newHealth;
        if (targetable)
        {
            GetComponent<HealthPopup>().TriggerPopup(health, maxHealth);
        }
    }

    public override IEnumerator Recover()
    {
        if (IsServer)
        {
            canAttack.Value = false;
            JointDrive jointDrive = mainJoint.slerpDrive;
            jointDrive.positionSpring = 0;
            mainJoint.slerpDrive = jointDrive;
            mainCol.center = new Vector3(0, 1, 0);
            foreach (SyncLimbs limb in limbs)
            {
                limb.MakeRagdoll();
            }

            yield return new WaitForSeconds(2);

            if (targetable)
            {
                jointDrive = mainJoint.slerpDrive;
                jointDrive.positionSpring = startSlerpPosSpring;
                mainJoint.slerpDrive = jointDrive;
                foreach (SyncLimbs limb in limbs)
                {
                    limb.MakeActiveRagdoll();
                }
                transform.position += new Vector3(0, Mathf.Abs(mainCol.center.y - startColPos.y), 0);
                mainCol.center = startColPos;
                canMove.Value = true;
            }
        }
    }

    public override void RemoveCharacter()
    {
        if (IsServer)
        {
            canMove.Value = false;
            canAttack.Value = false;
        }

        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = 0;
        mainJoint.slerpDrive = jointDrive;
        mainCol.center = new Vector3(0, 1, 0);
        rb.mass = 0.01f;

        foreach (SyncLimbs limb in limbs)
        {
            limb.MakeRagdoll();
        }
    }
}
