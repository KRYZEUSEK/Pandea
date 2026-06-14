using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public EndlessTerrain endlessTerrain;

    // 1. ADD THIS: A reference to your camera script
    public TopDownFollowCamera cameraController;

    public GameObject playerPrefab;

    void Start()
    {
        Vector3 spawnPosition = mapGenerator.GetPlayerSpawnPosition();
        GameObject spawnedPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        if (endlessTerrain != null)
        {
            endlessTerrain.viewer = spawnedPlayer.transform;
        }

        // 2. ADD THIS: Tell the camera who to follow!
        if (cameraController != null)
        {
            cameraController.SetTarget(spawnedPlayer.transform);
        }
    }
}