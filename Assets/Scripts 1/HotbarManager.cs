using UnityEngine;
using UnityEngine.UI;

public class HotbarManager : MonoBehaviour
{
    public enum ItemType { Notebook, Hammer }

    [Header("Hotbar UI")]
    public Image notebookSlot;
    public Image hammerSlot;
    public Color selectedColor = new Color(0.35f, 0.75f, 1f, 1f);
    public Color normalColor = Color.white;

    [Header("Notebook")]
    public GameObject notebookPanel;

    [Header("Hammer")]
    public Camera playerCamera;
    public float hammerRange = 4f;
    public LayerMask repairTargetLayers = ~0;

    private ItemType selectedItem = ItemType.Notebook;
    private bool notebookOpen = false;

    void Start()
    {
        if (notebookPanel != null)
            notebookPanel.SetActive(false);

        UpdateHotbarVisuals();

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            SelectItem(ItemType.Notebook);

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            SelectItem(ItemType.Hammer);

        if (Input.GetMouseButtonDown(1))
            UseSelectedItem();
    }

    public void SelectItem(ItemType item)
    {
        selectedItem = item;
        UpdateHotbarVisuals();
    }

    private void UpdateHotbarVisuals()
    {
        if (notebookSlot != null)
            notebookSlot.color = selectedItem == ItemType.Notebook ? selectedColor : normalColor;

        if (hammerSlot != null)
            hammerSlot.color = selectedItem == ItemType.Hammer ? selectedColor : normalColor;
    }

    private void UseSelectedItem()
    {
        if (selectedItem == ItemType.Notebook)
        {
            notebookOpen = !notebookOpen;
            if (notebookPanel != null)
                notebookPanel.SetActive(notebookOpen);

            // Unlock cursor while notebook is open; lock it again when closed.
            Cursor.lockState = notebookOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = notebookOpen;
            return;
        }

        if (selectedItem == ItemType.Hammer)
            UseHammer();
    }

    private void UseHammer()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("HotbarManager: Player Camera is not assigned.");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, hammerRange, repairTargetLayers))
        {
            RepairTarget target = hit.collider.GetComponentInParent<RepairTarget>();
            if (target != null)
                target.HitWithHammer();
            else
                Debug.Log("Hammer hit a component without a RepairTarget.");
        }
        else
        {
            Debug.Log("Hammer hit nothing.");
        }
    }
}
