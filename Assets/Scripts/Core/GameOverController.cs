using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the Game Over screen countdown and restarting the game.
/// Attach this to a GameObject in the GameOverScene (e.g. the root Canvas),
/// then hook up the countdown text in the inspector.
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Header("Countdown Settings")]
    [Tooltip("How many seconds before restarting the game.")]
    public float countdownSeconds = 8f;

    [Header("UI References")]
    [Tooltip("Standard UI Text used for the countdown (if not using TextMeshPro).")]
    public Text countdownText;

    [Tooltip("TextMeshProUGUI used for the countdown (if using TextMeshPro).")]
    public TextMeshProUGUI countdownTMPText;

    private float remainingTime;

    private void Start()
    {
        // Ensure time is running normally in the GameOver scene
        Time.timeScale = 1f;

        remainingTime = countdownSeconds;
        UpdateCountdownText();
    }

    private void Update()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime < 0f)
        {
            remainingTime = 0f;
        }

        UpdateCountdownText();

        if (remainingTime <= 0f)
        {
            RestartGame();
        }
    }

    private void UpdateCountdownText()
    {
        int displaySeconds = Mathf.CeilToInt(remainingTime);
        string message = $"Respawning in {displaySeconds} secs";

        if (countdownText != null)
        {
            countdownText.text = message;
        }

        if (countdownTMPText != null)
        {
            countdownTMPText.text = message;
        }
    }

    private void RestartGame()
    {
        // Use GameManager if it exists, otherwise load MainScene directly.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainScene();
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null in GameOverScene, loading \"MainScene\" directly.");
            SceneManager.LoadScene("MainScene");
        }
    }
}


