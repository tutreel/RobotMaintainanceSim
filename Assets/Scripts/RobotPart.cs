
using UnityEngine;

public class RobotPart : MonoBehaviour
{
    public string partName = "Robot Part";

    [TextArea(3, 6)]
    public string description = "Part description";

    public Color highlightColor = Color.yellow;

    private Renderer[] partRenderers;
    private Material[][] originalMaterials;

    private static RobotPart selectedPart;

    private void Awake()
    {
        partRenderers = GetComponentsInChildren<Renderer>(true);
        originalMaterials = new Material[partRenderers.Length][];

        for (int i = 0; i < partRenderers.Length; i++)
        {
            originalMaterials[i] = partRenderers[i].sharedMaterials;
        }
    }

    public void SelectPart()
    {
        if (selectedPart != null)
        {
            selectedPart.RestoreMaterials();
        }

        selectedPart = this;
        Highlight();

        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.ShowPart(partName, description);
        }
        else
        {
            Debug.LogError("InspectionUI.Instance is missing!");
        }
    }

    private void Highlight()
    {
        for (int i = 0; i < partRenderers.Length; i++)
        {
            Material[] materials = partRenderers[i].materials;

            foreach (Material material in materials)
            {
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", highlightColor);

                if (material.HasProperty("_Color"))
                    material.SetColor("_Color", highlightColor);
            }
        }
    }

    private void RestoreMaterials()
    {
        for (int i = 0; i < partRenderers.Length; i++)
        {
            partRenderers[i].sharedMaterials = originalMaterials[i];
        }
    }
}
