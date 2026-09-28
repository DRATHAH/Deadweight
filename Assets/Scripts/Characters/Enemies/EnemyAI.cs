using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyAI : DamageableCharacter
{
    public float attackRange = 2f;
    public Transform attackTarget;
    public NavMeshAgent agent;

    List<Transform> players = new List<Transform>();
    bool canMove = true;

    public override void OnNetworkSpawn()
    {
        GameManager.instance.OnSpawnCPUs += Gamemager_OnSpawnCPUs;
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

    private void Update()
    {
        if (canMove)
        {
            float distanceToPlayer = -1;
            // Get all players connected in the game
            foreach (Transform target in players)
            {
                if ((target.position - transform.position).sqrMagnitude <= distanceToPlayer || distanceToPlayer == -1)
                {
                    distanceToPlayer = (target.position - transform.position).sqrMagnitude;
                    attackTarget = target;
                }
            }

            if ((attackTarget.position -  transform.position).magnitude >= attackRange)
            {
                //agent.SetDestination(attackTarget.position);
                agent.updateRotation = true;
            }
            else
            {
                //agent.SetDestination(transform.position);
                agent.updateRotation = false;
                if (rb.constraints != RigidbodyConstraints.FreezeRotationZ)
                {
                    Debug.Log(rb.constraints != RigidbodyConstraints.FreezeRotationZ);
                    transform.LookAt(attackTarget.position);
                }
            }
        }
    }
}
