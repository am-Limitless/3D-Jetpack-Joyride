using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private ObstaclePool obstaclePool;

    private void Start()
    {
        obstaclePool = FindAnyObjectByType<ObstaclePool>();
    }

    private void Update()
    {
        if (transform.position.z < Camera.main.transform.position.z - 10f)
        {
            obstaclePool.ReturnObstacle(gameObject);
        }
    }
}
