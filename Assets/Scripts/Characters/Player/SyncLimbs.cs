using UnityEngine;

public class SyncLimbs : MonoBehaviour
{
    Rigidbody rb;
    ConfigurableJoint joint;

    Rigidbody animatedRb;
    bool syncAnim = false;

    Quaternion startLocalRot;
    float startSlerpPositionSpring = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        // Store values on start
        startLocalRot = transform.localRotation;
        startSlerpPositionSpring = joint.slerpDrive.positionSpring;
    }

    public void UpdateJointFromAnimation()
    {
        if (!syncAnim)
        {
            return;
        }
    }

    public void MakeRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = 1;
        joint.slerpDrive = jointDrive;
    }

    public void MakeActiveRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        joint.slerpDrive = jointDrive;
    }
}
