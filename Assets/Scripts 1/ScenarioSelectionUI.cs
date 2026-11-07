
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScenarioSelectionUI : MonoBehaviour
{
    public ScenarioManager scenarioManager;
    public TMP_Text scenarioNameText;

    public string simulationSceneName = "backup robot arvr";

    private bool scenarioSelected = false;

    public void RandomizeScenario()
    {
        scenarioManager.RandomizeScenario();
        scenarioSelected = true;

        scenarioNameText.text = "Scenario selected. Good luck!";
    }

    public void StartMission()
    {
        if (!scenarioSelected)
        {
            scenarioNameText.text = "Please randomize a scenario first!";
            return;
        }

        SceneManager.LoadScene(simulationSceneName);
    }
}
