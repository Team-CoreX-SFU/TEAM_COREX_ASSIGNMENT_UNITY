using UnityEngine;

/// <summary>
/// Singleton manager for easy access to PlayerFollowUI from anywhere in the codebase.
/// Provides static methods to show notifications and timers.
/// </summary>
public class PlayerFollowUIManager : MonoBehaviour
{
    public static PlayerFollowUIManager Instance { get; private set; }
    
    [Header("UI Reference")]
    [Tooltip("The PlayerFollowUI component. Auto-found if null.")]
    public PlayerFollowUI playerFollowUI;

    [Header("Exit Door Keypad Pins (Optional)")]
    [Tooltip("Correct keypad code for the Exit Door (e.g. 152)")]
    public string exitDoorKeypadCode = "152";

    [Tooltip("Character shown for unknown keypad digits before they are revealed")]
    public char exitDoorHiddenCharacter = '_';

    [Tooltip("Prefix text for the Exit Door keypad notification")]
    public string exitDoorNotificationPrefix = "Exit Door Code";

    // Internal tracking for keypad pins
    private bool[] exitDoorCollectedDigits;
    private UINotification exitDoorNotification;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Find PlayerFollowUI if not assigned
        if (playerFollowUI == null)
        {
            playerFollowUI = FindObjectOfType<PlayerFollowUI>();
            
            if (playerFollowUI == null)
            {
                Debug.LogWarning("PlayerFollowUIManager: PlayerFollowUI not found! Creating one...");
                CreatePlayerFollowUI();
            }
        }

        // Initialise keypad pins tracking
        if (!string.IsNullOrEmpty(exitDoorKeypadCode))
        {
            exitDoorCollectedDigits = new bool[exitDoorKeypadCode.Length];
        }
    }
    
    private void CreatePlayerFollowUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("PlayerFollowUI");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Set canvas size
        RectTransform rectTransform = canvasObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(1920, 1080);
        }
        
        // Add CanvasScaler for proper scaling
        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
        
        // Add GraphicRaycaster
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Set layer to UI
        canvasObj.layer = LayerMask.NameToLayer("UI");
        
        // For Screen Space Overlay, positioning doesn't matter
        // Canvas will render on screen regardless of world position
        // So we don't need to position it near camera
        
        // Add PlayerFollowUI component
        playerFollowUI = canvasObj.AddComponent<PlayerFollowUI>();
    }
    
    /// <summary>
    /// Show a notification
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="duration">Duration before auto-hiding. Use 0 or negative to never auto-hide (default: 3f)</param>
    /// <param name="icon">Optional icon sprite</param>
    public static UINotification ShowNotification(string message, float duration = 3f, Sprite icon = null)
    {
        if (Instance == null || Instance.playerFollowUI == null)
        {
            Debug.LogWarning("PlayerFollowUIManager not initialized! Cannot show notification.");
            return null;
        }
        
        return Instance.playerFollowUI.ShowNotification(message, duration, icon);
    }
    
    /// <summary>
    /// Show a collection notification (common use case)
    /// Does not auto-hide by default - stays until manually hidden or new notification replaces it
    /// </summary>
    public static UINotification ShowCollectionNotification(string itemName, float duration = 0f)
    {
        // Duration of 0 means don't auto-hide
        return ShowNotification($"Collected: {itemName}", duration);
    }
    
    /// <summary>
    /// Show a timer that counts down
    /// </summary>
    public static UINotification ShowTimer(string label, float duration, System.Action onComplete = null)
    {
        if (Instance == null || Instance.playerFollowUI == null)
        {
            Debug.LogWarning("PlayerFollowUIManager not initialized! Cannot show timer.");
            return null;
        }
        
        return Instance.playerFollowUI.ShowTimer(label, duration, onComplete);
    }
    
    /// <summary>
    /// Show a hide timer (countdown until something hides)
    /// </summary>
    public static UINotification ShowHideTimer(float duration, System.Action onComplete = null)
    {
        return ShowTimer("Hiding in", duration, onComplete);
    }

    // ---------------- EXIT DOOR KEYPAD PINS HELPERS ----------------

    /// <summary>
    /// Register that a keypad pin with a specific digit value (e.g. 1, 5, 2) has been revealed.
    /// Order is determined by exitDoorKeypadCode, not by collection order.
    /// </summary>
    public static void RegisterExitDoorPin(int pinValue)
    {
        if (Instance == null) return;
        char digitChar = pinValue.ToString()[0];
        Instance.RegisterExitDoorPinInternal(digitChar);
    }

    private void RegisterExitDoorPinInternal(char digitChar)
    {
        if (string.IsNullOrEmpty(exitDoorKeypadCode))
            return;

        if (exitDoorCollectedDigits == null || exitDoorCollectedDigits.Length != exitDoorKeypadCode.Length)
        {
            exitDoorCollectedDigits = new bool[exitDoorKeypadCode.Length];
        }

        // Find the first matching slot for this digit that isn't already revealed
        for (int i = 0; i < exitDoorKeypadCode.Length; i++)
        {
            if (!exitDoorCollectedDigits[i] && exitDoorKeypadCode[i] == digitChar)
            {
                exitDoorCollectedDigits[i] = true;
                break;
            }
        }

        // If the notification is visible, update its text
        if (exitDoorNotification != null)
        {
            exitDoorNotification.UpdateMessage(BuildExitDoorKeypadDisplayText());
        }
    }

    /// <summary>
    /// Show or update the Exit Door keypad pins notification.
    /// Uses duration = 0 so it never auto-hides.
    /// </summary>
    public static void ShowExitDoorKeypadNotification()
    {
        if (Instance == null) return;
        Instance.ShowExitDoorKeypadNotificationInternal();
    }

    private void ShowExitDoorKeypadNotificationInternal()
    {
        string message = BuildExitDoorKeypadDisplayText();

        if (exitDoorNotification == null)
        {
            // duration 0 => persistent notification
            exitDoorNotification = ShowNotification(message, 0f);
        }
        else
        {
            exitDoorNotification.UpdateMessage(message);
        }
    }

    /// <summary>
    /// Build display text like "Exit Door Code: 1 5 _" based on collected digits.
    /// </summary>
    private string BuildExitDoorKeypadDisplayText()
    {
        if (string.IsNullOrEmpty(exitDoorKeypadCode))
            return exitDoorNotificationPrefix + ": (no code configured)";

        if (exitDoorCollectedDigits == null || exitDoorCollectedDigits.Length != exitDoorKeypadCode.Length)
        {
            exitDoorCollectedDigits = new bool[exitDoorKeypadCode.Length];
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(exitDoorNotificationPrefix);
        sb.Append(": ");

        for (int i = 0; i < exitDoorKeypadCode.Length; i++)
        {
            bool revealed = exitDoorCollectedDigits[i];
            char c = revealed ? exitDoorKeypadCode[i] : exitDoorHiddenCharacter;
            sb.Append(c);

            if (i < exitDoorKeypadCode.Length - 1)
                sb.Append(' ');
        }

        return sb.ToString();
    }
}

