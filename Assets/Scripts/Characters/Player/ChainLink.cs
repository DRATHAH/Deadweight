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

    void CorrectPosition(float tension)
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

            Rigidbody chainStart = connectedChains[connectedChains.Count - 1].GetComponent<ConnectConfigJoints>().chainStart;
            Vector3 target = new Vector3(chainStart.position.x, transform.position.y - 1, chainStart.position.z);
            Vector3 direction = (target - transform.position).normalized;
            float force = correctionForce * (tension - tensionLimit);
            //GetComponent<Rigidbody>().AddForce(direction * force);

            ChainLink attachedLink = attachedPlayer.GetComponent<ChainLink>();
            Rigidbody attachedStart = attachedLink.connectedChains[attachedLink.connectedChains.Count - 1].GetComponent<ConnectConfigJoints>().chainStart;
            target = new Vector3(attachedStart.position.x, attachedPlayer.position.y - 1, attachedStart.position.z);
            direction = (target - attachedPlayer.position).normalized;
            attachedPlayer.GetComponent<Rigidbody>().AddForce((direction * force));
        }
    }

    void GetTotalTension()
    {
        float tension = 0;
        foreach(GameObject chain in connectedChains)
        {
            HingeJoint[] chainJoints = chain.GetComponentsInChildren<HingeJoint>();
            foreach(HingeJoint joint in chainJoints)
            {
                tension += joint.currentForce.magnitude;
            }
        }

        if (tension >= tensionLimit)
        {
            CorrectPosition(tension);
        }
        Debug.Log(tension);
    }
}
