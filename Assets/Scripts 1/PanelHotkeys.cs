
using UnityEngine;

public class PanelHotkeys : MonoBehaviour
{
    public GameObject inspectionPanel;
    public GameObject repairPanel;
    public GameObject faultPanel;
    public GameObject robotControlsPanel;

    void Start()
    {
        CloseAll();
        LockMouse();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ToggleInspection();

        if (Input.GetKeyDown(KeyCode.F))
            ToggleFault();

        if (Input.GetKeyDown(KeyCode.R))
            ToggleRobotControls();

        // Escape closes menus and returns to first-person
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAll();
            LockMouse();
        }
    }

    void CloseAll()
    {
        inspectionPanel.SetActive(false);
        repairPanel.SetActive(false);
        faultPanel.SetActive(false);
        robotControlsPanel.SetActive(false);
    }

    void ToggleInspection()
    {
        bool opening = !inspectionPanel.activeSelf;
        CloseAll();
        inspectionPanel.SetActive(opening);
    }

    void ToggleFault()
    {
        bool opening = !faultPanel.activeSelf;
        CloseAll();
        faultPanel.SetActive(opening);
        repairPanel.SetActive(opening);
        SetMouse(opening);
    }

    void ToggleRobotControls()
    {
        bool opening = !robotControlsPanel.activeSelf;
        CloseAll();
        robotControlsPanel.SetActive(opening);
        SetMouse(opening);
    }

    void SetMouse(bool unlocked)
    {
        Cursor.lockState = unlocked
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = unlocked;
    }

    void LockMouse()
    {
        SetMouse(false);
    }
}
