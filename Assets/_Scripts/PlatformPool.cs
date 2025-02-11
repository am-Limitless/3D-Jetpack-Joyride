using UnityEngine;
using UnityEngine.Pool;

public class PlatformPool : MonoBehaviour
{
    public GameObject platformPrefab;
    public int poolSize = 10;

    public ObjectPool<GameObject> platformPool;

    private void Awake()
    {
        platformPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(platformPrefab),             // Create new instance
            actionOnGet: (platform) => platform.SetActive(true),       // Activate on retrieval
            actionOnRelease: (platform) => platform.SetActive(false),  // Deactivate on return
            actionOnDestroy: (platform) => Destroy(platform),          // Cleanup when destroyed
            collectionCheck: false,                                    // Avoid extra performance checks
            defaultCapacity: poolSize,
            maxSize: poolSize * 2                                      // Double the pool size if needed
        );
    }

    public GameObject GetPlatform(Vector3 position)
    {
        GameObject platform = platformPool.Get();
        platform.transform.position = position;
        return platform;
    }

    public void ReturnPlatform(GameObject platform)
    {
        platformPool.Release(platform);
    }
}
