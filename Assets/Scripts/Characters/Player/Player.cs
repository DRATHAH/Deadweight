using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : DamageableCharacter
{
    public static event EventHandler OnAnyPlayerSpawn; // Runs whenever a player spawns

    public static Player LocalInstance { get; private set; }

    [Header("Player Stats")]
    public Weapon equippedWeapon;
    public float moveSpeed = 1f;
    public float turnSpeed = 150f;
    public float jumpForce = 10f;
    public float attackRate = 1f;
    [Header("References")]
    public Transform weaponPoint;
    public LayerMask groundLayer;
    public Animator animator;
    public InputActionReference moveRef;
    public InputActionReference jumpRef;
    public ConfigurableJoint mainJoint;
    public SphereCollider mainCol;
    public SkinnedMeshRenderer renderer;
    public Material controlledPlayerMat;

    float timeSinceAttack = 0f;
    Quaternion mainJointTargetRotation;
    Quaternion startingRotation;
    float startSlerpPosSpring = 0;
    Vector3 startColPos = Vector3.zero;

    bool isActiveRagdoll = true;
    SyncLimbs[] limbs;

    NetworkVariable<bool> canAttack = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );
    NetworkVariable<bool> canMove = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn() // Multiplayer's version of Start()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }

        if (IsOwner)
        {
            LocalInstance = this;
            renderer.material = controlledPlayerMat;
        }
        else
        {
            GetComponent<PlayerInput>().enabled = false;
        }

        OnAnyPlayerSpawn?.Invoke(this, EventArgs.Empty);
        startSlerpPosSpring = mainJoint.slerpDrive.positionSpring;
        limbs = GetComponentsInChildren<SyncLimbs>();
        startColPos = mainCol.center;
        startingRotation = transform.rotation;
    }

    void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId)
        {
            foreach(GameObject chain in GetComponent<ChainLink>().ownedChains)
            {
                if (chain != null)
                {
                    Destroy(chain);
                }
            }
            foreach (GameObject chain in GetComponent<ChainLink>().otherChains)
            {
                if (chain != null)
                {
                    Destroy(chain);
                }
            }
            // If client is holding something, put code to destroy it/drop it
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        if (canMove.Value)
        {
            MoveServerAuth();
        }

        if (timeSinceAttack < attackRate)
        {
            timeSinceAttack += Time.deltaTime;
        }
        else
        {
            ResetAttackServerRpc();
        }

        if (isActiveRagdoll)
        {
            foreach(SyncLimbs limb in limbs)
            {
                limb.UpdateJointFromAnimation();
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ResetAttackServerRpc(RpcParams rpcParams = default)
    {
        ulong attackerId = rpcParams.Receive.SenderClientId;
        Transform attacker = NetworkManager.Singleton.ConnectedClients[attackerId].PlayerObject.transform;
        attacker.GetComponent<Player>().canAttack.Value = true;
    }

    void OnJump(InputValue jumpButton)
    {
        if (IsGrounded() && IsOwner)
        {
            JumpServerRpc(jumpForce);
        }
    }

    private void MoveServerAuth()
    {
        Vector2 input = moveRef.action.ReadValue<Vector2>();
        MoveServerRpc(input);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)] 
    void MoveServerRpc(Vector2 inputVector, RpcParams rpcParams = default)
    {
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, moveSpeed * 2);

        ulong clientId = rpcParams.Receive.SenderClientId;
        Transform playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform;

        float horInput = inputVector.x;
        float vertInput = inputVector.y;
        Vector3 movement = new Vector3(horInput, 0, vertInput);
        if (movement.magnitude > 0)
        {
            Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(movement.x * -1, 0, movement.z), transform.up) * startingRotation;
            mainJointTargetRotation = Quaternion.RotateTowards(mainJointTargetRotation, desiredDirection, Time.fixedDeltaTime * turnSpeed);
            mainJoint.targetRotation = mainJointTargetRotation;

            float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
            playerObj.GetComponent<Player>().animator.SetFloat("Speed", 1);
        }
        else
        {
            playerObj.GetComponent<Player>().animator.SetFloat("Speed", 0);
        }

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void JumpServerRpc(float jump, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Rigidbody rigidbody = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Rigidbody>();
        rigidbody.AddForce(Vector3.up * jump, ForceMode.Impulse);
    }

    bool IsGrounded()
    {
        Vector3 start = mainCol.transform.TransformPoint(mainCol.center);
        float rayLength = mainCol.radius + 0.025f;
        bool hasHit = Physics.SphereCast(start, mainCol.radius / 2, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer, QueryTriggerInteraction.Ignore);
        Debug.Log(hasHit);
        return hasHit;
    }

    void OnAttack(InputValue attackButton)
    {
        if (canAttack.Value)
        {
            timeSinceAttack = 0;
            AttackServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void AttackServerRpc(RpcParams rpcParams = default)
    {
        ulong attackerId = rpcParams.Receive.SenderClientId;
        Transform attacker = NetworkManager.Singleton.ConnectedClients[attackerId].PlayerObject.transform;
        attacker.GetComponent<Player>().animator.SetTrigger("Attack");
        attacker.GetComponent<Player>().canAttack.Value = false;
        /*List<DamageableCharacter> hitTargets = new List<DamageableCharacter>();
        Collider[] hits = Physics.OverlapSphere(attacker.position + attacker.forward * 0.5f, 2);
        foreach (Collider hit in hits)
        {
            DamageableCharacter character = hit.transform.root.GetComponent<DamageableCharacter>();
            if (character && !hitTargets.Contains(character) && character != attacker.GetComponent<DamageableCharacter>())
            {
                hitTargets.Add(character);
                Vector3 hitDirection = (character.transform.position - attacker.position).normalized;
                character.OnHit(equippedWeapon.dmg, hitDirection * equippedWeapon.knockbackStrength);
            }
        }*/
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
            canMove.Value = false;
        }

        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = 0;
        mainJoint.slerpDrive = jointDrive;
        mainCol.center = new Vector3(0, 1, 0);
        foreach(SyncLimbs limb in limbs)
        {
            limb.MakeRagdoll();
        }

        isActiveRagdoll = false;

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
            if (IsServer)
            {
                canMove.Value = true;
            }
            isActiveRagdoll = true;
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

        isActiveRagdoll = false;
    }

    public override void OnDestroy()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }
}
