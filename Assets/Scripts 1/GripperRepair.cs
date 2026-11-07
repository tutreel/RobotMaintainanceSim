
using UnityEngine;
using TMPro;

public class GripperRepair : MonoBehaviour
{
    public ColliderTest gripperController;
    public PneumaticHoseVisual hoseVisual;
    public TMP_Text repairStatus;
    public FaultDiagnosis faultDiagnosis;

    public void RepairGripper()
    {
        if (faultDiagnosis == null)
        {
            Debug.LogError("FaultDiagnosis reference missing!");
            return;
        }

        if (!faultDiagnosis.diagnosisCompleted)
        {
            if (repairStatus != null)
                repairStatus.text = "Diagnose the fault first!";
            return;
        }

        if (gripperController == null)
        {
            Debug.LogError("ColliderTest reference missing!");
            return;
        }

        gripperController.gripperFault = false;

        faultDiagnosis.actuatorFaultActive = false;
        faultDiagnosis.diagnosisCompleted = false;
        faultDiagnosis.repairCompleted = true;

        faultDiagnosis.statusText.text = "System status: Normal";
        faultDiagnosis.diagnosisText.text = "Fault repaired successfully.";

        if (repairStatus != null)
            repairStatus.text = "Gripper hose reconnected!";
    }
}
