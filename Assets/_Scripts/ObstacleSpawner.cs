using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public Transform player;
    public ObstaclePool obstaclePool;
    public float spawnDistance = 30f;
    public float spawnInterval = 5f; // Spawn every 5 seconds
    public float minHeight = 5f;     // Minimum height above ground
    public float maxHeight = 35f;    // Maximum height

    private void Start()
    {
        StartCoroutine(SpawnObstacles());
    }

    private IEnumerator SpawnObstacles()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            Vector3 spawnPosition = new Vector3(
               Random.Range(-20f, 20f),  // Random X position
               Random.Range(minHeight, maxHeight), // Random Y position (in air)
               player.position.z + spawnDistance // Spawn in front of player
           );

            obstaclePool.GetObstacle(spawnPosition);
        }
    }
}