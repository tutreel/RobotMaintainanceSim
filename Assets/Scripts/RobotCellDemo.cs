using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Self-contained, asset-free demonstration for the MTE 4442 industrial robot
/// inspection and fault-diagnosis project. Attach to an empty GameObject in a
/// new Unity 6 3D scene, then press Play.
/// Uses primitive geometry and OnGUI so no packages or imported assets are needed.
/// </summary>
public class RobotCellDemo : MonoBehaviour
{
    private class PartInfo
    {
        public string title;
        public string condition;
        public string details;
        public GameObject visual;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public Vector3 originalLocalPosition;
        public Quaternion originalLocalRotation;
        public bool isActuator;
    }

    private readonly List<PartInfo> parts = new List<PartInfo>();
    private readonly List<Transform> movingJoints = new List<Transform>();

    private Camera sceneCamera;
    private Light warningLight;
    private Material normalMat, metalMat, darkMat, accentMat, warningMat, greenMat;
    private PartInfo selectedPart;
    private Transform baseJoint, shoulderJoint, elbowJoint, wristJoint, gripper;
    private float robotClock;
    private float faultSeverity = 0.0f;
    private bool faultActive;
    private bool inspectionMode;
    private bool explodedView;
    private bool actuatorReplaced;
    private bool maintenanceComplete;
    private bool robotRunning = true;
    private bool testPassed;
    private string statusMessage = "System ready. Start the robot, then inject a fault to begin the exercise.";
    private string diagnosisMessage = "No diagnosis performed yet.";
    private string lastTestMessage = "Verification has not been run.";
    private Vector3 cameraHomePosition;
    private Quaternion cameraHomeRotation;

    private void Start()
    {
        CreateMaterials();
        BuildEnvironment();
        BuildRobot();
        BuildHUDCamera();
    }

    private void Update()
    {
        robotClock += Time.deltaTime;

        // The pause control must stop all robot animation, including fault judder.
        if (robotRunning)
        {
            if (faultActive)
                AnimateFaultedRobot();
            else
                AnimateRobot();
        }

        if (Input.GetMouseButtonDown(0))
            HandleWorldClick();

        if (warningLight != null)
            warningLight.enabled = faultActive;

        UpdatePartColors();
    }

    private void CreateMaterials()
    {
        normalMat = MakeMaterial(new Color(0.20f, 0.48f, 0.72f));
        metalMat = MakeMaterial(new Color(0.62f, 0.68f, 0.72f));
        darkMat = MakeMaterial(new Color(0.10f, 0.13f, 0.16f));
        accentMat = MakeMaterial(new Color(0.95f, 0.55f, 0.12f));
        warningMat = MakeMaterial(new Color(0.95f, 0.12f, 0.08f));
        greenMat = MakeMaterial(new Color(0.12f, 0.72f, 0.30f));
    }

    private Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private GameObject Primitive(string name, PrimitiveType type, Vector3 position,
        Vector3 scale, Material material, Transform parent = null)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = scale;
        if (parent != null)
            go.transform.SetParent(parent, true);

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.material = material;
        return go;
    }

    private void BuildEnvironment()
    {
        Primitive("Factory Floor", PrimitiveType.Cube, new Vector3(0, -0.25f, 0),
            new Vector3(15, 0.5f, 12), darkMat);
        Primitive("Workstation", PrimitiveType.Cube, new Vector3(2.5f, 0.55f, 0),
            new Vector3(3.2f, 0.35f, 2.2f), metalMat);
        Primitive("Workpiece", PrimitiveType.Cube, new Vector3(2.5f, 0.95f, 0),
            new Vector3(0.55f, 0.45f, 0.55f), accentMat);
        Primitive("Safety Barrier Left", PrimitiveType.Cube, new Vector3(-4.7f, 1.1f, 0),
            new Vector3(0.12f, 2.2f, 7.0f), accentMat);
        Primitive("Safety Barrier Back", PrimitiveType.Cube, new Vector3(0, 1.1f, 3.5f),
            new Vector3(9.5f, 2.2f, 0.12f), accentMat);
        Primitive("Control Cabinet", PrimitiveType.Cube, new Vector3(-3.3f, 0.9f, -2.2f),
            new Vector3(1.0f, 1.8f, 0.8f), normalMat);
        Primitive("Cabinet Screen", PrimitiveType.Cube, new Vector3(-2.78f, 1.1f, -2.2f),
            new Vector3(0.05f, 0.45f, 0.45f), greenMat);

        // Simple lighting and a warning beacon.
        GameObject lightObject = new GameObject("Factory Main Light");
        Light mainLight = lightObject.AddComponent<Light>();
        mainLight.type = LightType.Directional;
        mainLight.intensity = 1.25f;
        lightObject.transform.rotation = Quaternion.Euler(48, -28, 0);

        GameObject beacon = Primitive("Fault Warning Beacon", PrimitiveType.Sphere,
            new Vector3(-3.3f, 1.95f, -2.2f), new Vector3(0.22f, 0.22f, 0.22f), warningMat);
        warningLight = beacon.AddComponent<Light>();
        warningLight.type = LightType.Point;
        warningLight.range = 3.5f;
        warningLight.intensity = 2.5f;
        warningLight.enabled = false;
    }

    private void BuildRobot()
    {
        Primitive("Robot Base", PrimitiveType.Cylinder, new Vector3(-0.8f, 0.25f, 0),
            new Vector3(1.0f, 0.25f, 1.0f), darkMat);

        baseJoint = new GameObject("Base Rotation Joint").transform;
        baseJoint.position = new Vector3(-0.8f, 0.55f, 0);
        AddPart("Base rotation actuator", "Healthy", "Rotates the robot base around its vertical axis.", null, true);

        GameObject shoulder = Primitive("Shoulder Link", PrimitiveType.Cube,
            new Vector3(-0.8f, 1.35f, 0), new Vector3(0.55f, 1.25f, 0.55f), normalMat, baseJoint);
        shoulder.transform.localPosition = new Vector3(0, 0.7f, 0);
        shoulderJoint = new GameObject("Shoulder Joint").transform;
        shoulderJoint.SetParent(baseJoint, false);
        shoulderJoint.localPosition = new Vector3(0, 1.15f, 0);
        AddPart("Shoulder actuator", "Healthy", "Controls the main lifting movement of the arm.", shoulder, false);

        GameObject upperArm = Primitive("Upper Arm Link", PrimitiveType.Cube,
            shoulderJoint.position + new Vector3(0.0f, 0.7f, 0.0f),
            new Vector3(0.35f, 1.5f, 0.38f), normalMat, shoulderJoint);
        upperArm.transform.localPosition = new Vector3(0, 0.7f, 0);
        elbowJoint = new GameObject("Elbow Joint").transform;
        elbowJoint.SetParent(shoulderJoint, false);
        elbowJoint.localPosition = new Vector3(0, 1.45f, 0);
        AddPart("Elbow actuator", "Healthy", "Drives the elbow joint. This is the simulated fault target.", upperArm, true);

        GameObject forearm = Primitive("Forearm Link", PrimitiveType.Cube,
            elbowJoint.position + new Vector3(0, 0.5f, 0),
            new Vector3(0.28f, 1.05f, 0.3f), metalMat, elbowJoint);
        forearm.transform.localPosition = new Vector3(0, 0.5f, 0);
        wristJoint = new GameObject("Wrist Joint").transform;
        wristJoint.SetParent(elbowJoint, false);
        wristJoint.localPosition = new Vector3(0, 1.0f, 0);
        AddPart("Wrist actuator", "Healthy", "Fine positioning and end-effector orientation.", forearm, false);

        gripper = new GameObject("Gripper").transform;
        gripper.SetParent(wristJoint, false);
        gripper.localPosition = new Vector3(0, 0.25f, 0);
        Primitive("Gripper Palm", PrimitiveType.Cube, gripper.position,
            new Vector3(0.45f, 0.2f, 0.35f), darkMat, gripper);
        Primitive("Gripper Finger A", PrimitiveType.Cube, gripper.position + new Vector3(0.0f, -0.22f, 0.12f),
            new Vector3(0.12f, 0.35f, 0.12f), accentMat, gripper);
        Primitive("Gripper Finger B", PrimitiveType.Cube, gripper.position + new Vector3(0.0f, -0.22f, -0.12f),
            new Vector3(0.12f, 0.35f, 0.12f), accentMat, gripper);
        AddPart("End effector / gripper", "Healthy", "Holds and releases the simulated workpiece.", gripper.gameObject, false);

        // Record a few more selectable cell components.
        AddPart("Control cabinet", "Healthy", "Provides the simulated control and status interface.", GameObject.Find("Control Cabinet"), false);
        AddPart("Workpiece", "Healthy", "Object handled by the robot during the pick-and-place cycle.", GameObject.Find("Workpiece"), false);
    }

    private void AddPart(string title, string condition, string details, GameObject visual, bool actuator)
    {
        parts.Add(new PartInfo {
            title = title, condition = condition, details = details, visual = visual,
            isActuator = actuator,
            originalPosition = visual != null ? visual.transform.position : Vector3.zero,
            originalRotation = visual != null ? visual.transform.rotation : Quaternion.identity,
            originalLocalPosition = visual != null ? visual.transform.localPosition : Vector3.zero,
            originalLocalRotation = visual != null ? visual.transform.localRotation : Quaternion.identity
        });
    }

    private void BuildHUDCamera()
    {
        GameObject cameraObject = new GameObject("Demo Camera");
        sceneCamera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(7.6f, 6.3f, -10.8f);
        cameraObject.transform.LookAt(new Vector3(-0.2f, 1.1f, 0));
        cameraHomePosition = cameraObject.transform.position;
        cameraHomeRotation = cameraObject.transform.rotation;
        sceneCamera.fieldOfView = 52;
        sceneCamera.clearFlags = CameraClearFlags.Skybox;
        sceneCamera.backgroundColor = new Color(0.15f, 0.19f, 0.23f);
    }

    private void AnimateRobot()
    {
        float t = robotClock * 1.2f;
        baseJoint.localRotation = Quaternion.Euler(0, Mathf.Sin(t) * 28f, 0);
        shoulderJoint.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 0.8f) * 22f);
        elbowJoint.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 0.8f + 1.1f) * 34f);
        wristJoint.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 1.2f) * 18f);
    }

    private void AnimateFaultedRobot()
    {
        // The arm develops a visible hesitation/judder when the actuator fault is active.
        float judder = Mathf.Sin(robotClock * 19f) * 2.2f;
        elbowJoint.localRotation = Quaternion.Euler(0, 0, -18f + judder);
        shoulderJoint.localRotation = Quaternion.Euler(0, 0, 7f);
    }

    private void HandleWorldClick()
    {
        if (sceneCamera == null) return;
        Ray ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            foreach (PartInfo part in parts)
            {
                if (part.visual == null) continue;
                if (hit.transform == part.visual.transform || hit.transform.IsChildOf(part.visual.transform))
                {
                    selectedPart = part;
                    inspectionMode = true;
                    statusMessage = "Inspecting: " + part.title;
                    return;
                }
            }
            // Parent joint links can be selected by clicking their named transforms too.
            foreach (PartInfo part in parts)
            {
                if (part.title == "Elbow actuator" && hit.transform.name.Contains("Elbow"))
                {
                    selectedPart = part;
                    inspectionMode = true;
                    statusMessage = "Inspecting elbow actuator.";
                    return;
                }
            }
        }
    }

    private void UpdatePartColors()
    {
        foreach (PartInfo part in parts)
        {
            if (part.visual == null) continue;
            Renderer renderer = part.visual.GetComponent<Renderer>();
            if (renderer == null) continue;
            if (part.isActuator && faultActive && part.title == "Elbow actuator")
                renderer.material.color = warningMat.color;
            else if (selectedPart == part)
                renderer.material.color = new Color(1f, 0.85f, 0.25f);
            else
                renderer.material.color = part.condition == "Faulty" ? warningMat.color :
                    (part.title.Contains("Workpiece") ? accentMat.color : normalMat.color);
        }
    }

    private void InjectFault()
    {
        faultActive = true;
        faultSeverity = 0.72f;
        robotRunning = true; // Start the fault scenario so its abnormal motion is visible.
        actuatorReplaced = false;
        maintenanceComplete = false;
        testPassed = false;
        diagnosisMessage = "Fault not diagnosed. Inspect symptoms and run the diagnostic assistant.";
        statusMessage = "FAULT: Elbow actuator feedback is inconsistent. Robot motion is unstable.";
        PartInfo elbow = parts.Find(p => p.title == "Elbow actuator");
        if (elbow != null) elbow.condition = "Faulty";
    }

    private void RunDiagnosis()
    {
        if (!faultActive)
        {
            diagnosisMessage = "No active fault detected. System indicators are normal.";
            return;
        }
        diagnosisMessage = "Likely cause: elbow actuator feedback / servo response fault. Evidence: joint hesitation, position error, and elevated simulated fault score (" +
            Mathf.RoundToInt(faultSeverity * 100f) + "%). Recommended action: isolate the cell, inspect the elbow actuator, replace it, then verify motion.";
        statusMessage = "Diagnostic assistant completed. Review the suspected component before maintenance.";
        selectedPart = parts.Find(p => p.title == "Elbow actuator");
        inspectionMode = true;
    }

    private void ReplaceActuator()
    {
        if (!faultActive)
        {
            statusMessage = "No faulty actuator is present. Inject a fault first to demonstrate maintenance.";
            return;
        }

        // A repair cannot leave the robot in an exploded or displaced state.
        RestoreExplodedView();
        ResetJointPose();

        actuatorReplaced = true;
        maintenanceComplete = true;
        faultActive = false;
        faultSeverity = 0;
        testPassed = false;
        robotRunning = true;

        PartInfo elbow = parts.Find(p => p.title == "Elbow actuator");
        if (elbow != null) elbow.condition = "Replaced / healthy";

        diagnosisMessage = "Maintenance complete: elbow actuator replacement recorded. Run verification to confirm recovery.";
        statusMessage = "Replacement completed. Robot assembly restored. Run the verification test.";
        lastTestMessage = "Verification has not been run.";
    }

    private void VerifyRecovery()
    {
        // Verification always starts from the assembled configuration.
        RestoreExplodedView();
        ResetJointPose();

        bool assemblyAvailable =
            baseJoint != null && shoulderJoint != null &&
            elbowJoint != null && wristJoint != null && gripper != null;

        if (maintenanceComplete && actuatorReplaced && !faultActive && assemblyAvailable && !explodedView)
        {
            // Resume the normal operation so recovery is visible rather than inferred only from a flag.
            testPassed = true;
            robotRunning = true;
            lastTestMessage = "PASS — assembly restored, actuator fault cleared, and normal robot motion resumed.";
            statusMessage = "Verification successful. Robot assembly is restored and normal operation has resumed.";
        }
        else
        {
            testPassed = false;
            lastTestMessage = "FAIL — complete actuator replacement and clear the fault before verification.";
            statusMessage = "Verification failed. Complete diagnosis and maintenance first.";
        }
    }

    private void ResetJointPose()
    {
        if (baseJoint != null) baseJoint.localRotation = Quaternion.identity;
        if (shoulderJoint != null) shoulderJoint.localRotation = Quaternion.identity;
        if (elbowJoint != null) elbowJoint.localRotation = Quaternion.identity;
        if (wristJoint != null) wristJoint.localRotation = Quaternion.identity;
    }

    private void ToggleExplodedView()
    {
        if (explodedView)
        {
            RestoreExplodedView();
            statusMessage = "Exploded view closed. Robot assembly restored.";
            return;
        }

        explodedView = true;

        // Move only the visible links, not the joint transforms. This preserves
        // the hierarchy and prevents downstream parts from flying off-screen.
        string[] robotPartNames =
        {
            "Shoulder actuator",
            "Elbow actuator",
            "Wrist actuator",
            "End effector / gripper"
        };

        Vector3[] offsets =
        {
            new Vector3(-0.28f, 0.16f, 0.00f),
            new Vector3(0.22f, 0.20f, 0.00f),
            new Vector3(-0.22f, 0.18f, 0.00f),
            new Vector3(0.20f, 0.12f, 0.00f)
        };

        for (int i = 0; i < robotPartNames.Length; i++)
        {
            PartInfo part = parts.Find(p => p.title == robotPartNames[i]);
            if (part == null || part.visual == null) continue;

            part.visual.transform.localPosition = part.originalLocalPosition + offsets[i];
        }

        statusMessage = "Exploded view enabled. Robot links are slightly separated for inspection.";
    }

    private void RestoreExplodedView()
    {
        if (!explodedView) return;

        foreach (PartInfo part in parts)
        {
            if (part.visual == null) continue;

            if (part.title == "Shoulder actuator" ||
                part.title == "Elbow actuator" ||
                part.title == "Wrist actuator" ||
                part.title == "End effector / gripper")
            {
                part.visual.transform.localPosition = part.originalLocalPosition;
                part.visual.transform.localRotation = part.originalLocalRotation;
            }
        }

        explodedView = false;
    }

    private void ResetDemo()
    {
        RestoreExplodedView();

        faultActive = false;
        inspectionMode = false;
        actuatorReplaced = false;
        maintenanceComplete = false;
        testPassed = false;
        robotRunning = true;
        robotClock = 0;
        faultSeverity = 0;

        ResetJointPose();

        diagnosisMessage = "No diagnosis performed yet.";
        lastTestMessage = "Verification has not been run.";
        statusMessage = "Demo reset. System ready.";

        foreach (PartInfo part in parts)
            part.condition = "Healthy";

        selectedPart = null;
        if (sceneCamera != null)
        {
            sceneCamera.transform.position = cameraHomePosition;
            sceneCamera.transform.rotation = cameraHomeRotation;
        }
    }

    private void OnGUI()
    {
        GUI.skin.box.fontSize = 14;
        GUI.skin.label.fontSize = 13;
        GUI.skin.button.fontSize = 13;

        GUI.Box(new Rect(12, 12, 350, 190), "INDUSTRIAL ROBOT CELL  |  DEMO");
        GUI.Label(new Rect(26, 42, 325, 24), "Cell status: " + (faultActive ? "FAULT ACTIVE" : "NORMAL"));
        GUI.Label(new Rect(26, 66, 325, 24), "Actuator: " + (faultActive ? "ELBOW — FAULTY" : (actuatorReplaced ? "ELBOW — REPLACED" : "ELBOW — OK")));
        GUI.Label(new Rect(26, 90, 325, 24), "Fault score: " + Mathf.RoundToInt(faultSeverity * 100f) + "%");
        GUI.Label(new Rect(26, 112, 325, 36), statusMessage);
        if (GUI.Button(new Rect(22, 151, 158, 30), faultActive ? "Fault Injected" : "Inject Actuator Fault"))
            InjectFault();
        if (GUI.Button(new Rect(190, 151, 158, 30), robotRunning ? "Pause Robot" : "Start Robot"))
        {
            robotRunning = !robotRunning;
            statusMessage = robotRunning
                ? (faultActive ? "Fault scenario resumed. Abnormal actuator motion is active." : "Robot operation started.")
                : "Robot operation paused. Robot motion is stopped.";
        }

        GUI.Box(new Rect(12, 210, 350, 292), "INSPECTION & DIAGNOSTICS");
        if (GUI.Button(new Rect(22, 240, 158, 30), "Toggle Exploded View"))
            ToggleExplodedView();
        if (GUI.Button(new Rect(190, 240, 158, 30), "Run Diagnosis"))
            RunDiagnosis();
        if (GUI.Button(new Rect(22, 276, 326, 30), "Replace Suspected Actuator"))
            ReplaceActuator();
        if (GUI.Button(new Rect(22, 312, 326, 30), "Verify Normal Operation"))
            VerifyRecovery();
        if (GUI.Button(new Rect(22, 348, 326, 30), "Reset Demonstration"))
            ResetDemo();
        GUI.Label(new Rect(22, 384, 326, 106), diagnosisMessage);

        GUI.Box(new Rect(Screen.width - 360, 12, 348, 260), "COMPONENT INSPECTOR");
        if (selectedPart != null)
        {
            GUI.Label(new Rect(Screen.width - 344, 44, 320, 28), "Component: " + selectedPart.title);
            GUI.Label(new Rect(Screen.width - 344, 74, 320, 28), "Condition: " + selectedPart.condition);
            GUI.Label(new Rect(Screen.width - 344, 104, 320, 115), selectedPart.details);
            if (GUI.Button(new Rect(Screen.width - 344, 226, 320, 30), "Close Inspection"))
            {
                selectedPart = null;
                inspectionMode = false;
            }
        }
        else
        {
            GUI.Label(new Rect(Screen.width - 344, 48, 320, 54), "Click a robot component or workpiece to inspect it.");
            GUI.Label(new Rect(Screen.width - 344, 104, 320, 70), "Use the buttons on the left to inject a fault, diagnose, repair, and verify recovery.");
        }

        GUI.Box(new Rect(Screen.width - 360, 284, 348, 126), "MAINTENANCE VERIFICATION");
        GUI.Label(new Rect(Screen.width - 344, 316, 320, 76), (testPassed ? "Status: PASS\n" : "") + lastTestMessage);
        GUI.Label(new Rect(12, Screen.height - 34, 800, 24),
            "MTE 4442 Prototype | Mouse: click a component to inspect | Keyboard/mouse prototype; gamepad and VR can be added later");
    }
}
