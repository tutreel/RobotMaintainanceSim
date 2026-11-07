using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance;
    public enum ScenarioType { NormalOperation, PneumaticFault, JointFault, SensorFault }
    public enum MissionState { Active, Success, Failed }

    [Header("Scenario")]
    public ScenarioType currentScenario;
    public MissionState missionState = MissionState.Active;

    [Header("Robot References (auto-filled when simulation loads)")]
    public ColliderTest gripperController;
    public RobotMove robotMove;

    [Header("Fault States")]
    public bool faultRepaired = false;
    public bool jointFaultActive = false;
    public bool sensorFaultActive = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else if (Instance != this) { Destroy(gameObject); }
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "backup robot arvr") return;

        gripperController = FindAnyObjectByType<ColliderTest>();
        robotMove = FindAnyObjectByType<RobotMove>();

        if (gripperController == null) Debug.LogError("ColliderTest not found in simulation scene!");
        else Debug.Log("Robot gripper found!");

        if (robotMove == null) Debug.LogError("RobotMove not found in simulation scene!");
        else Debug.Log("RobotMove found!");

        StartScenario();
    }

    public void RandomizeScenario()
    {
        currentScenario = (ScenarioType)Random.Range(0, 4);
        Debug.Log("Scenario selected secretly: " + currentScenario);
    }

    public void StartScenario()
    {
        missionState = MissionState.Active;
        faultRepaired = false;
        jointFaultActive = false;
        sensorFaultActive = false;

        if (gripperController != null) gripperController.gripperFault = false;
        if (robotMove != null) robotMove.RepairJointFault();

        switch (currentScenario)
        {
            case ScenarioType.NormalOperation:
                Debug.Log("Normal operation scenario.");
                break;
            case ScenarioType.PneumaticFault:
                if (gripperController != null) gripperController.gripperFault = true;
                Debug.Log("Pneumatic fault injected.");
                break;
            case ScenarioType.JointFault:
                jointFaultActive = true;
                if (robotMove != null) robotMove.InjectJointFault();
                Debug.Log("Joint fault scenario started.");
                break;
            case ScenarioType.SensorFault:
                sensorFaultActive = true;
                Debug.Log("Sensor fault injected.");
                break;
        }
    }

    public void CompleteRepair()
    {
        faultRepaired = true;
        if (gripperController != null) gripperController.gripperFault = false;
        if (robotMove != null) robotMove.RepairJointFault();
        jointFaultActive = false;
        sensorFaultActive = false;
        Debug.Log("Fault repaired.");
    }

    public void CompleteMission() { missionState = MissionState.Success; Debug.Log("MISSION SUCCESS!"); }
    public void FailMission() { missionState = MissionState.Failed; Debug.Log("MISSION FAILED!"); }
}
