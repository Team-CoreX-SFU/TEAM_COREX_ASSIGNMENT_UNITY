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
}

