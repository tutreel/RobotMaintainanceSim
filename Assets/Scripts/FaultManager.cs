
using UnityEngine;

public class FaultManager : MonoBehaviour
{
    public ColliderTest gripperController;

    public void InjectGripperFault()
    {
        gripperController.gripperFault = true;
        Debug.Log("FAULT: Gripper air hose disconnected.");
    }

    public void RepairGripperFault()
    {
        gripperController.gripperFault = false;
        Debug.Log("REPAIR: Gripper air hose reconnected.");
    }
}
