using UnityEngine;

public class ConnectConfigJoints : MonoBehaviour
{
    public Rigidbody chainStart;
    public Rigidbody chainEnd;

    public void InitializeChain(Rigidbody attachedBody)
    {
        ConfigurableJoint joint = attachedBody.GetComponent<ConfigurableJoint>();
        transform.position = attachedBody.transform.position;

        chainStart.GetComponent<ConfigurableJoint>().connectedBody = attachedBody;
    }
}
