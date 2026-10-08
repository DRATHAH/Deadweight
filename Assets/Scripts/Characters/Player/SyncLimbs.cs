using UnityEngine;

public class SyncLimbs : MonoBehaviour
{
    Rigidbody rb;
    ConfigurableJoint joint;

    [SerializeField] Rigidbody animatedRb;
    [SerializeField] bool syncAnim = false;

    Quaternion startLocalRot;
    float startSlerpPositionSpring = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
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

        SetTargetRotationInternal(joint, animatedRb.transform.localRotation, startLocalRot, Space.Self);
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

    void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
    {
        var right = joint.axis;
        var forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        var up = Vector3.Cross(forward, right).normalized;
        Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

        // Transform into world space
        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);

        // Counter-rotate and apply the new local rotation
        // Joint space is the inverse of world space, so we need to invert our value
        if (space == Space.World)
        {
            resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
        }
        else
        {
            resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
        }

        // Transform back into joint space
        resultRotation *= worldToJointSpace;

        // Set target rotation to our newly calculate rotation
        joint.targetRotation = resultRotation;
    }
}
