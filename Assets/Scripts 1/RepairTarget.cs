
using UnityEngine;

public class RepairTarget : MonoBehaviour
{
    public enum ComponentType
    {
        PneumaticHose,
        RobotJoint,
        ConveyorSensor,
        Other
    }

    public ComponentType componentType = ComponentType.Other;
    public int jointNumber = 0;

    public void HitWithHammer()
    {
        Debug.Log("Hammer hit: " + componentType);
    }
}
