using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChainLink : NetworkBehaviour
{
    public float chainDistance = 5f;
    public float correctionForce = 100f;
    public ConfigurableJoint[] anchors;

    public Transform attachedPlayer;
    public List<GameObject> connectedChains;

    public void SetDistance(float distance)
    {
        chainDistance = distance;
    }

    public void InitializeChain(Transform otherPlayer)
    {
        attachedPlayer = otherPlayer;
    }

    void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        CorrectPosition();
    }

    void CorrectPosition()
    {
        if (attachedPlayer)
        {
            float distance = (transform.position - attachedPlayer.position).magnitude;
            Vector3 direction = (attachedPlayer.position - transform.position).normalized;
            if (distance > chainDistance)
            {
                float force = correctionForce * (distance - chainDistance);

                GetComponent<Rigidbody>().AddForce(direction * force);
                attachedPlayer.GetComponent<Rigidbody>().AddForce(-direction * force);
            }
        }
    }
}
