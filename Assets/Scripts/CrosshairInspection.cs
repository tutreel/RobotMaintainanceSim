
using UnityEngine;

public class CrosshairInspection : MonoBehaviour
{
    public Camera playerCamera;
    public float interactionDistance = 5f;

    private RobotPart currentPart;

    void Update()
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit,
            interactionDistance))
        {
            RobotPart part = hit.collider.GetComponentInParent<RobotPart>();

            if (part != currentPart)
            {
                currentPart = part;

                if (currentPart != null)
                    Debug.Log("Looking at: " + currentPart.partName);
            }

            if (part != null && Input.GetKeyDown(KeyCode.E))
            {
                part.SelectPart();
            }
        }
        else
        {
            currentPart = null;
        }
    }
}
