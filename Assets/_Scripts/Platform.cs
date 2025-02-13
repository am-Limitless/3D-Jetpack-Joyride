using System.Collections;
using UnityEngine;

public class Platform : MonoBehaviour
{
    private Transform player;
    private PlatformPool platformPool;
    public float returnDelay = 20f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Ensure your player has the "Player" tag
        platformPool = FindAnyObjectByType<PlatformPool>();            // Get the pool reference
    }

    private void Update()
    {
        // If the player reference is lost, try to find it again
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform; // Reassign the new player
            }
            else
            {
                return; // Player is still missing, exit update
            }
        }


        if (player.position.z - transform.position.z > 50f)
        {
            StartCoroutine(DelayedReturn());
        }
    }

    private IEnumerator DelayedReturn()
    {
        yield return new WaitForSeconds(returnDelay);
        platformPool.ReturnPlatform(gameObject);
    }
}
