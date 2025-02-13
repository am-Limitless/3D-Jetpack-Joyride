using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObstaclePool : MonoBehaviour
{
    public GameObject obstaclePrefab; // Assign in Inspector
    public int poolSize = 5;

    private ObjectPool<GameObject> obstaclePool;
    private List<GameObject> activeObstacles = new List<GameObject>();

    private void Awake()
    {
        obstaclePool = new ObjectPool<GameObject>(
          createFunc: () => Instantiate(obstaclePrefab),
          actionOnGet: (obstacle) =>
          {
              obstacle.SetActive(true);
              activeObstacles.Add(obstacle); // Track active obstacle
          },
          actionOnRelease: (obstacle) =>
          {
              obstacle.SetActive(false);
              activeObstacles.Remove(obstacle); // Remove from active list
          },
          actionOnDestroy: (obstacle) => Destroy(obstacle),
          collectionCheck: false,
          defaultCapacity: poolSize,
          maxSize: poolSize * 2
      );
    }

    public GameObject GetObstacle(Vector3 position)
    {
        GameObject obstacle = obstaclePool.Get();
        obstacle.transform.position = position;
        return obstacle;
    }

    public void ReturnObstacle(GameObject obstacle)
    {
        obstaclePool.Release(obstacle);
    }

    // 🚀 Reset all obstacles
    public void ResetAllObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            ReturnObstacle(activeObstacles[i]); // Return all active obstacles to the pool
        }
    }
}
