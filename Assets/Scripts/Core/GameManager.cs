using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Main game manager that handles game state, scene transitions, and save/load functionality
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    public string mainSceneName = "MainScene";
    public string gameManagerSceneName = "GameManagerScene";
    public string gameOverSceneName = "GameOverScene";

    [Header("UI References")]
    [Tooltip("Button that triggers save game")]
    public Button saveFileButton;

    [Header("Caught Sequence Settings")]
    [Tooltip("Slow motion time scale when player is caught (0.1 = 10% speed)")]
    [Range(0.1f, 0.5f)]
    public float caughtSlowMotionScale = 0.3f;

    [Tooltip("Duration to wait for kidnapper attack animation before loading game over scene (in real seconds, not affected by slow motion)")]
    public float caughtSequenceDuration = 2f;

    [Header("Damage Overlay Settings")]
    [Tooltip("Color of the damage overlay when caught")]
    public Color damageOverlayColor = new Color(1f, 0f, 0f, 0.5f); // Red with 50% opacity

    [Tooltip("Fade in duration for damage overlay")]
    public float overlayFadeInDuration = 0.5f;

    [Tooltip("Fade out duration for damage overlay (before game over)")]
    public float overlayFadeOutDuration = 0.3f;

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
    private bool isCaughtSequenceActive = false;
    private bool punchCompleted = false;
    private GameObject damageOverlayObject;
    private UnityEngine.UI.Image damageOverlayImage;

    /// <summary>
    /// Ensure there is always a GameManager in the scene before anything runs.
    /// This makes sure KidnapperAI and other scripts can safely call GameManager.Instance.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void BootstrapGameManager()
    {
        if (Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
    }

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

        // Load game data if in MainScene (whether coming from GameManagerScene or returning via portal)
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[GAME MANAGER] Start() called. Current scene: {currentScene}, MainScene name: {mainSceneName}");

        if (currentScene == mainSceneName)
        {
            Debug.Log("[GAME MANAGER] MainScene detected! Starting load logic...");
            StartCoroutine(LoadGameOnReturnDelayed());
        }
        else
        {
            Debug.Log($"[GAME MANAGER] Not MainScene (current: {currentScene}), skipping MainScene load logic");
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
    /// <param name="kidnapperPosition">Position of the kidnapper who caught the player (optional)</param>
    public void PlayerCaught(Vector3? kidnapperPosition = null)
    {
        if (isCaughtSequenceActive)
        {
            // Already in caught sequence, ignore duplicate calls
            return;
        }

        OnPlayerCaught?.Invoke();

        // Start caught sequence with slow motion and camera rotation
        if (kidnapperPosition.HasValue)
        {
            StartCoroutine(CaughtSequence(kidnapperPosition.Value));
        }
        else
        {
            // If no kidnapper position provided, try to find the closest kidnapper
            KidnapperAI[] kidnappers = FindObjectsOfType<KidnapperAI>();
            Vector3 kidnapperPos = Vector3.zero;
            if (kidnappers != null && kidnappers.Length > 0)
            {
                // Find closest kidnapper to player
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    float closestDistance = float.MaxValue;
                    foreach (var kidnapper in kidnappers)
                    {
                        if (kidnapper != null)
                        {
                            float distance = Vector3.Distance(player.transform.position, kidnapper.transform.position);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                kidnapperPos = kidnapper.transform.position;
                            }
                        }
                    }
                }
            }

            if (kidnapperPos != Vector3.zero)
            {
                StartCoroutine(CaughtSequence(kidnapperPos));
            }
            else
            {
                // No kidnapper found, just go straight to game over
                GameOver();
            }
        }
    }

    /// <summary>
    /// Trigger game over state
    /// </summary>
    private void GameOver()
    {
        CurrentState = GameState.GameOver;
        OnGameOver?.Invoke();
        Debug.Log("Game Over - Player was caught!");
        // Load the dedicated Game Over scene with the respawn countdown
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogError("GameOver scene name is not set on GameManager!");
        }
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

        // Load non-portal game data if it exists (flashlight, batteries, rope state, etc.)
        if (saveSystem != null && saveSystem.SaveFileExists())
        {
            Debug.Log("[GAME MANAGER] Loading save file (non-portal data only)...");
            saveSystem.LoadGameWithoutPlayerPosition();
        }

        // If we have never used a portal this session (fresh game start),
        // do NOT reposition the player – just keep the scene's default spawn.
        // But we still applied non-portal data above (rope cut state, items, etc.).
        if (portalManager.LastUsedPortalIndex < 0)
        {
            Debug.Log("[GAME MANAGER] No last used portal index set (fresh start). Skipping return-to-portal teleport.");
            return;
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

    /// <summary>
    /// Called by KidnapperAI when the punch/attack animation is complete
    /// </summary>
    public void OnPunchCompleted()
    {
        punchCompleted = true;
        Debug.Log("[GAME MANAGER] Punch completed signal received");
    }

    /// <summary>
    /// Coroutine that handles the caught sequence: slow motion and red overlay, waits for punch to complete
    /// </summary>
    private System.Collections.IEnumerator CaughtSequence(Vector3 kidnapperPosition)
    {
        isCaughtSequenceActive = true;
        punchCompleted = false; // Reset punch completion flag
        CurrentState = GameState.GameOver;

        Debug.Log($"[GAME MANAGER] Starting caught sequence. Kidnapper position: {kidnapperPosition}");

        // Apply slow motion immediately
        Time.timeScale = caughtSlowMotionScale;
        Debug.Log($"[GAME MANAGER] Applied slow motion: {caughtSlowMotionScale}");

        // Show red damage overlay
        StartCoroutine(ShowDamageOverlay());

        // Wait for kidnapper punch to complete
        // Check every frame until punch is completed
        float maxWaitTime = 10f; // Safety timeout (in real seconds)
        float elapsed = 0f;

        while (!punchCompleted && elapsed < maxWaitTime)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (punchCompleted)
        {
            Debug.Log("[GAME MANAGER] Punch completed, transitioning to game over");
        }
        else
        {
            Debug.LogWarning($"[GAME MANAGER] Punch completion timeout after {maxWaitTime} seconds, transitioning anyway");
        }

        // Small delay after punch to let it sink in
        yield return new WaitForSecondsRealtime(0.5f);

        // Fade out damage overlay before scene change
        yield return StartCoroutine(HideDamageOverlay());

        // Restore time scale before scene change
        Time.timeScale = 1f;
        Debug.Log("[GAME MANAGER] Restored time scale");

        isCaughtSequenceActive = false;
        punchCompleted = false; // Reset for next time

        // Load game over scene
        GameOver();
    }

    /// <summary>
    /// Creates and shows a red damage overlay on screen
    /// </summary>
    private System.Collections.IEnumerator ShowDamageOverlay()
    {
        // Find or create canvas for overlay
        Canvas overlayCanvas = FindObjectOfType<Canvas>();
        if (overlayCanvas == null || overlayCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            // Create a new canvas for the overlay
            GameObject canvasObj = new GameObject("DamageOverlayCanvas");
            overlayCanvas = canvasObj.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 9999; // Make sure it's on top of everything

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Create overlay image if it doesn't exist
        if (damageOverlayObject == null)
        {
            damageOverlayObject = new GameObject("DamageOverlay");
            damageOverlayObject.transform.SetParent(overlayCanvas.transform, false);

            RectTransform rectTransform = damageOverlayObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            damageOverlayImage = damageOverlayObject.AddComponent<UnityEngine.UI.Image>();
            damageOverlayImage.color = new Color(damageOverlayColor.r, damageOverlayColor.g, damageOverlayColor.b, 0f); // Start transparent
        }
        else
        {
            // Reuse existing overlay
            damageOverlayObject.transform.SetParent(overlayCanvas.transform, false);
        }

        // Fade in the overlay
        float elapsed = 0f;
        Color targetColor = damageOverlayColor;
        Color startColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);

        while (elapsed < overlayFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time so fade works during slow motion
            float t = Mathf.Clamp01(elapsed / overlayFadeInDuration);

            damageOverlayImage.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        // Ensure final color is exact
        damageOverlayImage.color = targetColor;
        Debug.Log("[GAME MANAGER] Damage overlay shown");
    }

    /// <summary>
    /// Fades out and removes the damage overlay
    /// </summary>
    private System.Collections.IEnumerator HideDamageOverlay()
    {
        if (damageOverlayImage == null || damageOverlayObject == null)
        {
            yield break;
        }

        // Fade out the overlay
        float elapsed = 0f;
        Color startColor = damageOverlayImage.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsed < overlayFadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / overlayFadeOutDuration);

            damageOverlayImage.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        // Ensure final color is transparent
        damageOverlayImage.color = targetColor;

        // Destroy overlay object
        if (damageOverlayObject != null)
        {
            Destroy(damageOverlayObject);
            damageOverlayObject = null;
            damageOverlayImage = null;
        }

        Debug.Log("[GAME MANAGER] Damage overlay hidden");
    }
}

