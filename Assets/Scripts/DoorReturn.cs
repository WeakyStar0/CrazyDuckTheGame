using UnityEngine;

public class DoorReturn : MonoBehaviour
{
    private HingeJoint hinge;

    void Start()
    {
        hinge = GetComponent<HingeJoint>();
        JointSpring spring = hinge.spring;
        spring.spring = 20f;
        spring.damper = 5f;
        spring.targetPosition = 0f;
        hinge.spring = spring;
        hinge.useSpring = true;
    }
}
