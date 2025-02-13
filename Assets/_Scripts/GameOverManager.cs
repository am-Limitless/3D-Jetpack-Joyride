using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject secondChanceCanvas;
    [SerializeField] private GameObject inGameCanvas;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Button flyButton;
    [SerializeField] private TMP_Text distanceText;
    public ObstacleSpawner obstacleSpawner;

    private Vector3 lastPosition;
    private GameObject currentPlayer;
    private bool secondChanceUsed = false;

    private float savedDistance = 0f; // Store traveled distance before respawn

    private void Start()
    {
        secondChanceCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
    }

    public void SaveLastPosition(Vector3 position, GameObject player)
    {
        lastPosition = position;
        currentPlayer = player;
    }

    public void UseSecondChance()
    {
        //if (secondChanceUsed)
        //{
        //    return;
        //}

        // Resume game UI
        inGameCanvas.SetActive(true);
        secondChanceCanvas.SetActive(false);

        secondChanceUsed = true;

        GameObject newPlayer = Instantiate(playerPrefab, lastPosition, Quaternion.identity);

        JetpackController jetpackController = newPlayer.GetComponent<JetpackController>();
        if (jetpackController != null)
        {
            jetpackController.ResetPlayer();
            jetpackController.SetDistanceTraveled(savedDistance);
            jetpackController.AssignDistanceText(distanceText);
            jetpackController.onPlayerHit.AddListener(GameOver);
        }

        obstacleSpawner.player = newPlayer.transform;

        PlatformSpawner platformSpawner = FindAnyObjectByType<PlatformSpawner>();
        if (platformSpawner != null)
        {
            platformSpawner.player = newPlayer.transform;
        }

        StartCoroutine(DisableObstacleCollisionTemporarily(newPlayer, 3f));

        // Update the current player reference
        currentPlayer = newPlayer;
        StartCoroutine(AssignCameraAfterDelay(newPlayer));

        // Reassign Fly Button Events
        if (flyButton != null)
        {
            flyButton.onClick.RemoveAllListeners(); // Clear old events

            EventTrigger trigger = flyButton.GetComponent<EventTrigger>();
            if (trigger != null)
            {
                trigger.triggers.Clear(); // Clear existing event triggers
            }
            else
            {
                trigger = flyButton.gameObject.AddComponent<EventTrigger>();
            }

            // Add new event listeners for Pointer Down & Pointer Up
            AddEventTrigger(trigger, EventTriggerType.PointerDown, (eventData) => jetpackController.StartFlying());
            AddEventTrigger(trigger, EventTriggerType.PointerUp, (eventData) => jetpackController.StopFlying());
        }


    }

    public void GameOver()
    {
        inGameCanvas.SetActive(false);
        audioSource.Play();

        if (currentPlayer != null)
        {
            JetpackController jetpackController = currentPlayer.GetComponent<JetpackController>();
            if (jetpackController != null)
            {
                savedDistance = jetpackController.GetDistanceInKilometers();
            }

            SaveLastPosition(currentPlayer.transform.position, currentPlayer);
            Destroy(currentPlayer);
            currentPlayer = null;
        }

        if (secondChanceUsed)
        {
            gameOverCanvas.SetActive(true);
            secondChanceCanvas.SetActive(false);
        }
        else
        {
            secondChanceCanvas.SetActive(true);
            gameOverCanvas.SetActive(false);
            secondChanceUsed = true;
        }
    }

    private IEnumerator DisableObstacleCollisionTemporarily(GameObject player, float duration)
    {
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.detectCollisions = false; // Disable collision detection
            yield return new WaitForSeconds(duration);
            controller.detectCollisions = true; // Re-enable collision detection
        }
    }

    private IEnumerator EnableCharacterControllerAfterDelay(CharacterController controller, float delay)
    {
        yield return new WaitForSeconds(delay);
        controller.enabled = true;
    }

    private IEnumerator AssignCameraAfterDelay(GameObject newPlayer)
    {
        yield return new WaitForEndOfFrame();

        if (newPlayer != null && newPlayer.transform.childCount > 0)
        {
            Transform firstChild = newPlayer.transform.GetChild(0);
            cinemachineCamera.Follow = firstChild;
            cinemachineCamera.LookAt = firstChild;
        }
    }
    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener((data) => action.Invoke(data));
        trigger.triggers.Add(entry);
    }

    public void QuitGame()
    {
        gameOverCanvas.SetActive(true);
        secondChanceCanvas.SetActive(false);
    }
}
