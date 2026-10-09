using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChainLink : NetworkBehaviour
{
    public float chainDistance = 5f;
    public float tensionLimit = 400;
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
        GetTotalTension();
        //CorrectPosition();
    }

    void CorrectPosition(float tension, int chainIndex)
    {
        if (attachedPlayer)
        {
            /*float distance = (transform.position - attachedPlayer.position).magnitude;
            Vector3 direction = (attachedPlayer.position - transform.position).normalized;
            if (distance > chainDistance)
            {
                float force = correctionForce * (distance - chainDistance);

                GetComponent<Rigidbody>().AddForce(direction * force);
                attachedPlayer.GetComponent<Rigidbody>().AddForce(-direction * force);
            }*/

            Transform chainStart = connectedChains[chainIndex].transform;
            Vector3 target = new Vector3(chainStart.position.x, transform.position.y, chainStart.position.z);
            Vector3 direction = (target - transform.position).normalized;
            float force = correctionForce * (tension - tensionLimit);
            GetComponent<Rigidbody>().AddForce(direction * force);

            ChainLink attachedLink = attachedPlayer.GetComponent<ChainLink>();
            Transform attachedStart = attachedLink.connectedChains[Mathf.Abs(chainIndex - 5)].transform;
            target = new Vector3(attachedStart.position.x, attachedPlayer.position.y, attachedStart.position.z);
            direction = (target - attachedPlayer.position).normalized;
            attachedPlayer.GetComponent<Rigidbody>().AddForce((direction * force));
        }
    }

    void GetTotalTension()
    {
        float tension = 0;

        for (int i = 0; i < 3; i++)
        {
            ConnectConfigJoints chainJoints = connectedChains[i].GetComponent<ConnectConfigJoints>();
            tension += (chainJoints.chainStart.position - chainJoints.chainEnd.position).magnitude;
        }
        if (tension >= tensionLimit)
        {
            CorrectPosition(tension, 0);
        }

        for (int i = 3; i < 6; i++)
        {
            ConnectConfigJoints chainJoints = connectedChains[i].GetComponent<ConnectConfigJoints>();
            tension += (chainJoints.chainStart.position - chainJoints.chainEnd.position).magnitude;
        }
        if (tension >= tensionLimit)
        {
            CorrectPosition(tension, connectedChains.Count-1);
        }
    }
}
