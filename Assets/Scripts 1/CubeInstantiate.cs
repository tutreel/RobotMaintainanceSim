
using UnityEngine;

public class CubeInstantiate : MonoBehaviour
{
    public GameObject myPrefabRed;
    public GameObject myPrefabGreen;
    public GameObject myPrefabBlue;

    public Transform spawnPoint;

    public void InstantiateRed()
    {
        Instantiate(myPrefabRed, spawnPoint.position, spawnPoint.rotation);
    }

    public void InstantiateGreen()
    {
        Instantiate(myPrefabGreen, spawnPoint.position, spawnPoint.rotation);
    }

    public void InstantiateBlue()
    {
        Instantiate(myPrefabBlue, spawnPoint.position, spawnPoint.rotation);
    }
}
