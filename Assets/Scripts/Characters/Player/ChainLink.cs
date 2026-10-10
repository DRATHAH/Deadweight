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
    public List<GameObject> ownedChains;
    public List<GameObject> otherChains;

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

    void CorrectPosition(float tension, Transform chain, bool lastChain)
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

            Transform chainStart = chain;
            Vector3 target = new Vector3(chainStart.position.x, transform.position.y, chainStart.position.z);
            Vector3 direction = (target - transform.position).normalized;
            float force = correctionForce * (tension - tensionLimit);
            GetComponent<Rigidbody>().AddForce(direction * force);
        }
    }

    void GetTotalTension()
    {
        float tension = 0;

        foreach(GameObject chain in ownedChains)
        {
            ConnectConfigJoints chainJoints = chain.GetComponent<ConnectConfigJoints>();
            tension += (chainJoints.chainStart.position - chainJoints.chainEnd.position).magnitude;
        }
        if (tension >= tensionLimit)
        {
            Debug.Log(tension);
            CorrectPosition(tension, ownedChains[0].transform, false);
            return;
        }

        tension = 0;

        foreach (GameObject chain in otherChains)
        {
            ConnectConfigJoints chainJoints = chain.GetComponent<ConnectConfigJoints>();
            tension += (chainJoints.chainStart.position - chainJoints.chainEnd.position).magnitude;
        }
        if (tension >= tensionLimit)
        {
            Debug.Log(tension);
            CorrectPosition(tension, otherChains[otherChains.Count-1].transform, true);
            return;
        }
    }
}
