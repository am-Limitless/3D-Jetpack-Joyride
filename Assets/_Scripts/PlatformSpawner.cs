using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public Transform player;
    public PlatformPool platformPool;

    private float spawnZ = 0f;
    private float platformLength = 100f;
    private int intialPlatforms = 5;

    private void Start()
    {
        for (int i = 0; i < intialPlatforms; i++)
        {
            SpawnPlatform();
        }
    }

    private void Update()
    {
        if (player.position.z > spawnZ - (platformLength * (intialPlatforms - 1)))
        {
            SpawnPlatform();
        }
    }

    private void SpawnPlatform()
    {
        Vector3 spawnPostion = new Vector3(0, 0, spawnZ);
        GameObject platform = platformPool.GetPlatform(spawnPostion);
        spawnZ += platformLength;
    }
}
