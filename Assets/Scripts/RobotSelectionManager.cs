
using UnityEngine;
using UnityEngine.EventSystems;

public class RobotSelectionManager : MonoBehaviour
{
    public Camera playerCamera;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (playerCamera == null)
        {
            Debug.LogError("Assign a camera to RobotSelectionManager.");
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            RobotPart part = hit.collider.GetComponentInParent<RobotPart>();

            if (part != null)
            {
                part.SelectPart();
                Debug.Log("Selected robot part: " + part.partName);
            }
        }
    }
}
