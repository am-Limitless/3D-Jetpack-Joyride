using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject inGameCanvas;

    [SerializeField] private AudioSource audioSource;

    public void GameOver()
    {
        Debug.Log("Game Over! Player has hit an obstacle.");
        inGameCanvas.SetActive(false);
        gameOverCanvas.SetActive(true);
        audioSource.Play();
    }
}
