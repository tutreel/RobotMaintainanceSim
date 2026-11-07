
using UnityEngine;

public class LocalRobotController : MonoBehaviour
{
    [Header("Cube Prefabs")]
    public GameObject redCubePrefab;
    public GameObject greenCubePrefab;
    public GameObject blueCubePrefab;

    [Header("Robot")]
    public RobotMove robotMove;

    [Header("Spawn Position")]
    public Vector3 spawnPosition =
        new Vector3(-5.474f, 1.11f, 20.0969f);

    public Vector3 spawnRotation =
        new Vector3(0f, -24.176f, 0f);

    private GameObject activeCube;
    private bool operationRunning;

    public void SpawnRed()
    {
        SpawnCube(redCubePrefab);
    }

    public void SpawnGreen()
    {
        SpawnCube(greenCubePrefab);
    }

    public void SpawnBlue()
    {
        SpawnCube(blueCubePrefab);
    }

    private void SpawnCube(GameObject prefab)
    {
        if (operationRunning)
        {
            Debug.Log("Wait for the current robot operation to finish.");
            return;
        }

        if (prefab == null)
        {
            Debug.LogError("Assign the required cube prefab.");
            return;
        }

        if (activeCube != null)
        {
            Debug.Log("A cube is already active.");
            return;
        }

        activeCube = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.Euler(spawnRotation)
        );

        // Prevent conveyor movement until a local
        // conveyor controller is configured.
        CubeMove movement = activeCube.GetComponent<CubeMove>();
        if (movement != null)
            movement.enabled = false;
    }

    public void PickAndPlaceRed()
    {
        PickAndPlace(0);
    }

    public void PickAndPlaceGreen()
    {
        PickAndPlace(1);
    }

    public void PickAndPlaceBlue()
    {
        PickAndPlace(2);
    }

    private void PickAndPlace(int colour)
    {
        if (operationRunning)
            return;

        if (activeCube == null)
        {
            Debug.LogWarning("Spawn a cube before starting the robot.");
            return;
        }

        if (robotMove == null)
        {
            Debug.LogError("Assign the RobotMove component.");
            return;
        }

        operationRunning = true;

        switch (colour)
        {
            case 0:
                robotMove.GrabRed();
                break;
            case 1:
                robotMove.GrabGreen();
                break;
            case 2:
                robotMove.GrabBlue();
                break;
        }

        // This only prevents overlapping button commands.
        // We will add proper completion detection next.
        Invoke(nameof(AllowNextOperation), 8f);
    }

    private void AllowNextOperation()
    {
        operationRunning = false;
        activeCube = null;
    }
}
