using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class GateOpen : MonoBehaviour
{
    public float openSpeed = 200f;
    public float closeSpeed = 200f;
    public string playerTag = "Player";

    HingeJoint hinge;
    JointMotor motor;

    void Awake()
    {
        hinge = GetComponent<HingeJoint>();
        motor = hinge.motor;
    }

    void SetMotor(float targetVelocity)
    {
        hinge.useMotor = true;
        motor.targetVelocity = targetVelocity;
        hinge.motor = motor;
    }

    // Called by trigger child
    public void Open() => SetMotor(openSpeed);
    public void Close() => SetMotor(-closeSpeed);
}
