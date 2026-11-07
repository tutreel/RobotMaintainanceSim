
using TMPro;
using UnityEngine;

public class FaultDiagnosis : MonoBehaviour
{
    public TMP_Text statusText;
    public TMP_Text diagnosisText;
    public ColliderTest gripperController;

    public bool diagnosisCompleted = false;
    public bool repairCompleted = false;

    public bool actuatorFaultActive;

    public void InjectActuatorFault()
    {
        if (gripperController == null)
        {
            Debug.LogError(
                "FaultDiagnosis: Assign the AnimationEE ColliderTest."
            );
            return;
        }

        actuatorFaultActive = true;
        gripperController.gripperFault = true;

        statusText.text = "System status: FAULT DETECTED";
        diagnosisText.text = "Fault injected. Run diagnosis.";
    }

    public void RunDiagnosis()
    {
        if (!actuatorFaultActive)
        {
            diagnosisCompleted = false;
            statusText.text = "System status: Normal";
            diagnosisText.text = "No active fault detected.";
            return;
        }

        diagnosisCompleted = true;

        diagnosisText.text =
            "Diagnosis: Simulated gripper malfunction.\n" +
            "Symptom: Cube is not secured during pickup.\n" +
            "Recommended action: Inspect the gripper mechanism " +
            "and its actuation.";
    }

    public void ResetFault()
    {
        actuatorFaultActive = false;

        if (gripperController != null)
            gripperController.gripperFault = false;

        statusText.text = "System status: Normal";
        diagnosisText.text = "No active fault.";
    }
}
