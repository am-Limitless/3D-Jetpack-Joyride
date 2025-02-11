using UnityEngine;
using UnityEngine.Pool;

public class ObstaclePool : MonoBehaviour
{
    public GameObject obstaclePrefab; // Assign in Inspector
    public int poolSize = 5;

    private ObjectPool<GameObject> obstaclePool;

    private void Awake()
    {
        obstaclePool = new ObjectPool<GameObject>(
           createFunc: () => Instantiate(obstaclePrefab),
           actionOnGet: (obstacle) => obstacle.SetActive(true),
           actionOnRelease: (obstacle) => obstacle.SetActive(false),
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
}
