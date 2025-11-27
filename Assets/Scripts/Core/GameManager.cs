using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main game manager that handles game state, scene transitions, and save/load functionality
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    public string mainSceneName = "MainScene";
    public string gameManagerSceneName = "GameManagerScene";

    [Header("UI References")]
    [Tooltip("Button that triggers save game")]
    public Button saveFileButton;

    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Victory,
        InMenu
    }

    [Header("Game State")]
    public GameState startingState = GameState.Playing;

    public GameState CurrentState { get; private set; }

    // Events
    public System.Action OnGameOver;
    public System.Action OnPlayerCaught;
    public System.Action OnGameSaved;
    public System.Action OnGameLoaded;

    private SaveSystem saveSystem;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentState = startingState;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Get or create SaveSystem
        saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem == null)
        {
            GameObject saveSystemObj = new GameObject("SaveSystem");
            saveSystem = saveSystemObj.AddComponent<SaveSystem>();
        }
    }

    void Start()
    {
        // Setup save button if in GameManager scene
        if (saveFileButton != null)
        {
            saveFileButton.onClick.AddListener(SaveGame);
        }

        // Auto-find save button if not assigned
        if (saveFileButton == null)
        {
            GameObject saveButtonObj = GameObject.Find("SaveFileButton");
            if (saveButtonObj == null)
            {
                // Try alternative names
                saveButtonObj = GameObject.Find("SaveFilew");
            }

            if (saveButtonObj != null)
            {
                saveFileButton = saveButtonObj.GetComponent<Button>();
                if (saveFileButton != null)
                {
                    saveFileButton.onClick.AddListener(SaveGame);
                }
            }
        }

        // Load game data if returning from GameManager scene
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[GAME MANAGER] Start() called. Current scene: {currentScene}, MainScene name: {mainSceneName}");

        if (currentScene == mainSceneName)
        {
            Debug.Log("[GAME MANAGER] MainScene detected! Starting return logic...");
            StartCoroutine(LoadGameOnReturnDelayed());
        }
        else
        {
            Debug.Log($"[GAME MANAGER] Not MainScene (current: {currentScene}), skipping return logic");
        }
    }

    private System.Collections.IEnumerator LoadGameOnReturnDelayed()
    {
        Debug.Log("[GAME MANAGER] LoadGameOnReturnDelayed started, waiting for initialization...");
        // Wait a frame for everything to initialize
        yield return null;
        yield return null;
        yield return new WaitForSeconds(0.1f);

        Debug.Log("[GAME MANAGER] Calling LoadGameOnReturn()...");
        LoadGameOnReturn();
    }

    /// <summary>
    /// Called when player is caught by kidnapper
    /// </summary>
    public void PlayerCaught()
    {
        OnPlayerCaught?.Invoke();
        GameOver();
    }

    /// <summary>
    /// Trigger game over state
    /// </summary>
    private void GameOver()
    {
        CurrentState = GameState.GameOver;
        OnGameOver?.Invoke();
        Debug.Log("Game Over - Player was caught!");
        // You can add UI display, scene transition, etc. here
    }

    /// <summary>
    /// Save the current game state
    /// </summary>
    public void SaveGame()
    {
        if (saveSystem != null)
        {
            bool success = saveSystem.SaveGame();
            if (success)
            {
                OnGameSaved?.Invoke();
                Debug.Log("Game saved successfully!");

                // Show feedback (you can add UI notification here)
                ShowSaveFeedback(true);
            }
            else
            {
                Debug.LogError("Failed to save game!");
                ShowSaveFeedback(false);
            }
        }
        else
        {
            Debug.LogError("SaveSystem not found!");
        }
    }

    /// <summary>
    /// Load the saved game state
    /// </summary>
    public void LoadGame()
    {
        if (saveSystem != null)
        {
            bool success = saveSystem.LoadGame();
            if (success)
            {
                // Apply save data to current scene
                saveSystem.ApplySaveData();
                OnGameLoaded?.Invoke();
                Debug.Log("Game loaded successfully!");
            }
            else
            {
                Debug.LogWarning("No save file found or failed to load!");
            }
        }
    }

    /// <summary>
    /// Load game when returning to MainScene from GameManagerScene
    /// </summary>
    private void LoadGameOnReturn()
    {
        Debug.Log("[GAME MANAGER] LoadGameOnReturn() called!");

        // Get or create PortalManager
        PortalManager portalManager = FindObjectOfType<PortalManager>();
        if (portalManager == null)
        {
            Debug.Log("[GAME MANAGER] PortalManager not found, creating new one...");
            GameObject managerObj = new GameObject("PortalManager");
            portalManager = managerObj.AddComponent<PortalManager>();
        }
        else
        {
            Debug.Log("[GAME MANAGER] PortalManager found!");
        }

        // Load non-portal game data if it exists (flashlight, batteries, etc.)
        if (saveSystem != null && saveSystem.SaveFileExists())
        {
            Debug.Log("[GAME MANAGER] Loading save file (non-portal data only)...");
            saveSystem.LoadGameWithoutPlayerPosition();
        }

        // Wait one frame for scene to fully load, then return player to portal (using in-memory lastUsedPortalIndex)
        Debug.Log("[GAME MANAGER] Starting ReturnPlayerToPortalDelayed coroutine...");
        StartCoroutine(ReturnPlayerToPortalDelayed(portalManager));
    }

    private System.Collections.IEnumerator ReturnPlayerToPortalDelayed(PortalManager portalManager)
    {
        // Minimal wait to let scene objects initialize (no visible delay)
        yield return null;

        // Return player to the portal they entered from (or default portal)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && portalManager != null)
        {
            int portalIndex = portalManager.LastUsedPortalIndex;
            Debug.Log($"[RETURN] Attempting to return player to portal. Last used index: {portalIndex}");

            // Get portal position before teleporting
            Portal returnPortal = portalManager.GetReturnPortal();
            if (returnPortal != null)
            {
                Vector3 targetPos = returnPortal.transform.position;
                Debug.Log($"[RETURN] Target portal position: {targetPos}, Current player position: {player.transform.position}");

                // Teleport player
                portalManager.ReturnPlayerToPortal(player);

                // Verify position was set
                yield return null; // Wait one frame
                Debug.Log($"[RETURN] After teleport - Player position: {player.transform.position}, Target was: {targetPos}");

                // If position didn't stick, try again
                if (Vector3.Distance(player.transform.position, targetPos) > 0.5f)
                {
                    Debug.Log($"[RETURN] Position didn't stick, trying again...");
                    player.transform.position = targetPos;
                    yield return null;
                    player.transform.position = targetPos;
                    Debug.Log($"[RETURN] Final position after retry: {player.transform.position}");
                }
            }
            else
            {
                Debug.LogError("[RETURN] Could not get return portal!");
            }
        }
        else
        {
            Debug.LogWarning($"[RETURN] Player or PortalManager not found. Player: {player != null}, PortalManager: {portalManager != null}");
        }
    }

    /// <summary>
    /// Transition to GameManager scene
    /// </summary>
    public void GoToGameManagerScene()
    {
        SceneManager.LoadScene(gameManagerSceneName);
    }

    /// <summary>
    /// Transition to Main game scene
    /// </summary>
    public void GoToMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    /// <summary>
    /// Show save feedback (you can customize this with UI)
    /// </summary>
    private void ShowSaveFeedback(bool success)
    {
        // TODO: Add UI notification here
        // For now, just log
        if (success)
        {
            Debug.Log("✓ Game Saved!");
        }
        else
        {
            Debug.LogError("✗ Save Failed!");
        }
    }

    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }
}

