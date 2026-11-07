
using UnityEngine;
using TMPro;

public class InspectionUI : MonoBehaviour
{
    public static InspectionUI Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowPart(string partName, string description)
    {
        titleText.text = partName;
        descriptionText.text = description;
    }
}
