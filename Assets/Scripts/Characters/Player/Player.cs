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
    public float moveSpeed = 1f;
    public float turnSpeed = 150f;
    public float attackRate = 1f;
    public float knockback = 10f;
    [Header("References")]
    public InputActionReference moveRef;
    public ConfigurableJoint mainJoint;
    public SphereCollider mainCol;

    float timeSinceAttack = 0f;
    Quaternion mainJointTargetRotation;
    float startSlerpPosSpring = 0;
    Vector3 startColPos = Vector3.zero;
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
        }

        OnAnyPlayerSpawn?.Invoke(this, EventArgs.Empty);
        startSlerpPosSpring = mainJoint.slerpDrive.positionSpring;
        limbs = GetComponentsInChildren<SyncLimbs>();
        startColPos = mainCol.center;
    }

    void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId)
        {
            // If client is holding something, put code to destroy it/drop it
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
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ResetAttackServerRpc(RpcParams rpcParams = default)
    {
        ulong attackerId = rpcParams.Receive.SenderClientId;
        Transform attacker = NetworkManager.Singleton.ConnectedClients[attackerId].PlayerObject.transform;
        attacker.GetComponent<Player>().canAttack.Value = true;
    }

    private void MoveServerAuth()
    {
        Vector2 input = moveRef.action.ReadValue<Vector2>();
        MoveServerRpc(input);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)] 
    void MoveServerRpc(Vector2 inputVector, RpcParams rpcParams = default)
    {
        float horInput = inputVector.x;
        float vertInput = inputVector.y;
        Vector3 movement = new Vector3(horInput, 0, vertInput);
        Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(movement.x * -1, 0, movement.z), transform.up);
        mainJointTargetRotation = Quaternion.RotateTowards(mainJointTargetRotation, desiredDirection, Time.fixedDeltaTime * turnSpeed);
        if (movement.magnitude > 0)
        {
            mainJoint.targetRotation = mainJointTargetRotation;
        }

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void OnCPUs(InputValue cpuButton)
    {
        GameManager.instance.PopulateCPUs();
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
        attacker.GetComponent<Player>().canAttack.Value = false;
        List<DamageableCharacter> hitTargets = new List<DamageableCharacter>();
        Collider[] hits = Physics.OverlapSphere(attacker.position + attacker.forward * 0.5f, 2);
        foreach (Collider hit in hits)
        {
            DamageableCharacter character = hit.transform.root.GetComponent<DamageableCharacter>();
            if (character && !hitTargets.Contains(character) && character != attacker.GetComponent<DamageableCharacter>())
            {
                hitTargets.Add(character);
                Vector3 hitDirection = (character.transform.position - attacker.position).normalized;
                character.OnHit(0, hitDirection * knockback);
            }
        }
    }

    public override IEnumerator Recover()
    {
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = 0;
        mainJoint.slerpDrive = jointDrive;
        mainCol.center = new Vector3(0, 1, 0);

        foreach(SyncLimbs limb in limbs)
        {
            limb.MakeRagdoll();
        }

        canMove.Value = false;
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
        canMove.Value = true;
    }
}
