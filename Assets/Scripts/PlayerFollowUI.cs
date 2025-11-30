using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Position options for notifications on screen
/// </summary>
public enum NotificationPosition
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

/// <summary>
/// Main script that manages a UI canvas that follows the player's head/camera.
/// Supports multiple UI elements like timers, notifications, etc.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class PlayerFollowUI : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The camera/head transform to follow. If null, will try to find Main Camera.")]
    public Transform targetCamera;
    
    [Header("Follow Settings")]
    [Tooltip("Distance from camera in world units")]
    public float distanceFromCamera = 2f;
    
    [Tooltip("Height offset from camera (positive = above, negative = below)")]
    public float heightOffset = 0.2f;
    
    [Tooltip("Should UI always face the camera?")]
    public bool billboardMode = true;
    
    [Tooltip("Smoothing speed for position updates")]
    public float followSmoothing = 10f;
    
    [Header("UI Position Settings")]
    [Tooltip("Where on screen should notifications appear")]
    public NotificationPosition notificationPosition = NotificationPosition.BottomRight;
    
    [Tooltip("Where on screen should timers appear")]
    public NotificationPosition timerPosition = NotificationPosition.TopLeft;
    
    [Tooltip("Offset from screen edge for notifications (in pixels)")]
    public Vector2 notificationScreenOffset = new Vector2(20, 50);
    
    [Tooltip("Offset from screen edge for timers (in pixels)")]
    public Vector2 timerScreenOffset = new Vector2(20, 50);
    
    [Tooltip("Spacing between multiple notifications (in pixels)")]
    public float notificationSpacing = 10f;
    
    [Tooltip("Spacing between multiple timers (in pixels)")]
    public float timerSpacing = 10f;
    
    [Tooltip("Size of notification UI elements (width, height in pixels)")]
    public Vector2 notificationSize = new Vector2(300, 80);
    
    [Tooltip("Size of timer UI elements (width, height in pixels)")]
    public Vector2 timerSize = new Vector2(250, 70);
    
    [Tooltip("Font size for notification text (0 = auto-calculate based on notification size)")]
    public float notificationFontSize = 0f;
    
    [Tooltip("Font size for timer text (0 = auto-calculate based on timer size)")]
    public float timerFontSize = 0f;
    
    [Header("Canvas Settings")]
    [Tooltip("Canvas scale for VR (typically 0.001-0.002). Auto-adjusted for non-VR.")]
    public float canvasScale = 0.002f;
    
    [Tooltip("Canvas scale for non-VR/editor mode (typically 0.001-0.005)")]
    public float nonVRCanvasScale = 0.003f;
    
    [Header("UI References")]
    [Tooltip("Container for notifications")]
    public Transform notificationContainer;
    
    [Tooltip("Container for timers")]
    public Transform timerContainer;
    
    [Tooltip("Prefab for notification items")]
    public GameObject notificationPrefab;
    
    [Tooltip("Prefab for timer items")]
    public GameObject timerPrefab;

    private Canvas canvas;
    private Camera mainCamera;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        
        // Use Screen Space - Overlay for VR/XR (always visible, no camera needed)
        // This works best for XR Device Simulator and actual VR
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Make sure it's on top
        
        // Set canvas scale based on VR mode
        RectTransform rectTransform = GetComponent<RectTransform>();
        
        // Set a reasonable canvas size
        if (rectTransform.sizeDelta == Vector2.zero)
        {
            rectTransform.sizeDelta = new Vector2(1920, 1080); // Standard HD size
        }
        
        // For Screen Space Overlay, scale doesn't matter (it's screen-relative)
        // But we log it for reference
        float scale = IsVRMode() ? canvasScale : nonVRCanvasScale;
        
        // Log initial setup
        Debug.Log($"[PlayerFollowUI] Canvas initialized. Render Mode: {canvas.renderMode}, VR Mode: {IsVRMode()}, Sorting Order: {canvas.sortingOrder}");
        
        // Create containers if they don't exist
        if (notificationContainer == null)
        {
            GameObject notifContainer = new GameObject("NotificationContainer");
            notifContainer.transform.SetParent(transform);
            RectTransform notifRect = notifContainer.AddComponent<RectTransform>();
            SetupContainerPosition(notifRect, notificationPosition, notificationScreenOffset);
            notificationContainer = notifContainer.transform;
        }
        else
        {
            // Update existing container position
            RectTransform notifRect = notificationContainer.GetComponent<RectTransform>();
            if (notifRect == null)
            {
                notifRect = notificationContainer.gameObject.AddComponent<RectTransform>();
            }
            SetupContainerPosition(notifRect, notificationPosition, notificationScreenOffset);
        }
        
        if (timerContainer == null)
        {
            GameObject timerCont = new GameObject("TimerContainer");
            timerCont.transform.SetParent(transform);
            RectTransform timerRect = timerCont.AddComponent<RectTransform>();
            SetupContainerPosition(timerRect, timerPosition, timerScreenOffset);
            timerContainer = timerCont.transform;
        }
        else
        {
            // Update existing container position
            RectTransform timerRect = timerContainer.GetComponent<RectTransform>();
            if (timerRect == null)
            {
                timerRect = timerContainer.gameObject.AddComponent<RectTransform>();
            }
            SetupContainerPosition(timerRect, timerPosition, timerScreenOffset);
        }
    }
    
    void Start()
    {
        // Start with canvas hidden/disabled until camera is found
        canvas.enabled = false;
        
        // Find camera in Start() to allow XR system to initialize first
        FindCamera();
        
        // Setup canvas based on camera
        if (targetCamera != null)
        {
            SetupCanvasForCamera();
            canvas.enabled = true;
        }
        else
        {
            // Retry finding camera after a short delay
            StartCoroutine(RetryFindCamera());
        }
    }
    
    private System.Collections.IEnumerator RetryFindCamera()
    {
        int attempts = 0;
        while (targetCamera == null && attempts < 10)
        {
            yield return new WaitForSeconds(0.1f);
            FindCamera();
            attempts++;
        }
        
        if (targetCamera != null)
        {
            SetupCanvasForCamera();
            canvas.enabled = true;
            Debug.Log("[PlayerFollowUI] Camera found after retry, canvas enabled!");
        }
        else
        {
            Debug.LogError("[PlayerFollowUI] Failed to find camera after multiple attempts!");
        }
    }
    
    void SetupCanvasForCamera()
    {
        // For Screen Space Overlay, we don't need camera setup
        // But we keep this method for potential future use or if switching modes
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (targetCamera == null || mainCamera == null) 
            {
                Debug.LogWarning("[PlayerFollowUI] Cannot setup canvas - camera is null!");
                return;
            }
            
            canvas.worldCamera = mainCamera;
            canvas.planeDistance = distanceFromCamera;
            Debug.Log($"[PlayerFollowUI] Canvas setup for camera: {mainCamera.name}, Render Mode: {canvas.renderMode}, Plane Distance: {canvas.planeDistance}");
        }
        else
        {
            Debug.Log($"[PlayerFollowUI] Canvas using {canvas.renderMode} mode - no camera setup needed");
        }
    }
    
    void PositionCanvasNearCamera()
    {
        if (targetCamera == null) 
        {
            Debug.LogWarning("[PlayerFollowUI] Cannot position canvas - targetCamera is null!");
            return;
        }
        
        // Only needed for World Space mode
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Vector3 cameraForward = targetCamera.forward;
            Vector3 cameraPosition = targetCamera.position;
            
            // Set initial position immediately (no smoothing)
            Vector3 initialPos = cameraPosition + cameraForward * distanceFromCamera;
            initialPos.y += heightOffset;
            transform.position = initialPos;
            
            // Set initial rotation
            if (billboardMode)
            {
                Vector3 directionToCamera = (cameraPosition - transform.position).normalized;
                if (directionToCamera != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(-directionToCamera);
                }
            }
            
            Debug.Log($"[PlayerFollowUI] Canvas positioned at: {transform.position}, Camera at: {cameraPosition}, Distance: {Vector3.Distance(transform.position, cameraPosition)}");
        }
    }
    
    void FindCamera()
    {
        // If camera is already assigned, use it
        if (targetCamera != null)
        {
            mainCamera = targetCamera.GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = targetCamera.GetComponentInChildren<Camera>();
            }
            return;
        }
        
        // Method 1: Try Camera.main (works in both VR and non-VR)
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            targetCamera = mainCamera.transform;
            Debug.Log("[PlayerFollowUI] Found camera via Camera.main: " + mainCamera.name);
            PositionCanvasNearCamera();
            return;
        }
        
        // Method 2: Find XR Origin and get camera from it (for VR)
        GameObject xrOrigin = GameObject.FindGameObjectWithTag("Player");
        if (xrOrigin == null)
        {
            // Try finding by name
            xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            if (xrOrigin == null)
            {
                xrOrigin = GameObject.Find("XR Origin");
            }
        }
        
        if (xrOrigin != null)
        {
            // Look for camera in XR Origin hierarchy
            Camera xrCamera = xrOrigin.GetComponentInChildren<Camera>();
            if (xrCamera != null)
            {
                mainCamera = xrCamera;
                targetCamera = xrCamera.transform;
                Debug.Log("[PlayerFollowUI] Found camera in XR Origin: " + xrCamera.name);
                PositionCanvasNearCamera();
                return;
            }
        }
        
        // Method 3: Find any active camera with MainCamera tag
        Camera[] allCameras = FindObjectsOfType<Camera>(true); // Include inactive
        foreach (Camera cam in allCameras)
        {
            if (cam.CompareTag("MainCamera") || cam.name.Contains("Main Camera"))
            {
                if (cam.gameObject.activeInHierarchy)
                {
                    mainCamera = cam;
                    targetCamera = cam.transform;
                    Debug.Log("[PlayerFollowUI] Found camera by tag/name: " + cam.name);
                    PositionCanvasNearCamera();
                    return;
                }
            }
        }
        
        // Method 4: Find any active camera (last resort)
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.activeInHierarchy && cam.enabled)
            {
                mainCamera = cam;
                targetCamera = cam.transform;
                Debug.Log("[PlayerFollowUI] Found any active camera: " + cam.name);
                PositionCanvasNearCamera();
                return;
            }
        }
        
        Debug.LogWarning("[PlayerFollowUI] Could not find camera! UI will not follow player. Make sure camera is tagged as 'MainCamera' or assign manually.");
    }
    
    /// <summary>
    /// Setup container position based on position setting
    /// </summary>
    void SetupContainerPosition(RectTransform rect, NotificationPosition position, Vector2 offset)
    {
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        
        // Set anchor and pivot based on position
        switch (position)
        {
            case NotificationPosition.TopLeft:
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(offset.x, -offset.y);
                break;
                
            case NotificationPosition.TopCenter:
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 1);
                rect.anchoredPosition = new Vector2(offset.x, -offset.y);
                break;
                
            case NotificationPosition.TopRight:
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 1);
                rect.anchoredPosition = new Vector2(-offset.x, -offset.y);
                break;
                
            case NotificationPosition.MiddleLeft:
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);
                rect.anchoredPosition = new Vector2(offset.x, offset.y);
                break;
                
            case NotificationPosition.MiddleCenter:
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(offset.x, offset.y);
                break;
                
            case NotificationPosition.MiddleRight:
                rect.anchorMin = new Vector2(1, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                rect.pivot = new Vector2(1, 0.5f);
                rect.anchoredPosition = new Vector2(-offset.x, offset.y);
                break;
                
            case NotificationPosition.BottomLeft:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);
                rect.anchoredPosition = new Vector2(offset.x, offset.y);
                break;
                
            case NotificationPosition.BottomCenter:
                rect.anchorMin = new Vector2(0.5f, 0);
                rect.anchorMax = new Vector2(0.5f, 0);
                rect.pivot = new Vector2(0.5f, 0);
                rect.anchoredPosition = new Vector2(offset.x, offset.y);
                break;
                
            case NotificationPosition.BottomRight:
                rect.anchorMin = new Vector2(1, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(1, 0);
                rect.anchoredPosition = new Vector2(-offset.x, offset.y);
                break;
        }
    }
    
    /// <summary>
    /// Check if running in VR mode or simulator/editor mode
    /// </summary>
    private bool IsVRMode()
    {
        // Check if XR Origin exists (indicates VR setup)
        // In simulator/editor mode, XR Origin still exists but we want larger UI
        // So we check if we're actually in play mode with XR device
        #if UNITY_EDITOR
        // In editor, assume non-VR (simulator mode) for better visibility
        // User can manually adjust if needed
        return false;
        #else
        // In build, check for XR Origin
        GameObject xrOrigin = GameObject.FindGameObjectWithTag("Player");
        if (xrOrigin == null)
        {
            xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        }
        if (xrOrigin == null)
        {
            xrOrigin = GameObject.Find("XR Origin");
        }
        return xrOrigin != null;
        #endif
    }

    void LateUpdate()
    {
        // Only update position for World Space mode
        // Screen Space Camera mode doesn't need position updates
        if (canvas.renderMode != RenderMode.WorldSpace)
        {
            // Make sure camera is still assigned
            if (canvas.worldCamera == null && mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
            }
            return;
        }
        
        // Retry finding camera if not found yet (for delayed XR initialization)
        if (targetCamera == null)
        {
            FindCamera();
            if (targetCamera == null) return;
        }
        
        // Re-validate camera is still valid
        if (targetCamera == null || !targetCamera.gameObject.activeInHierarchy)
        {
            FindCamera();
            if (targetCamera == null) return;
        }
        
        // Calculate target position
        Vector3 cameraForward = targetCamera.forward;
        Vector3 cameraPosition = targetCamera.position;
        
        targetPosition = cameraPosition + cameraForward * distanceFromCamera;
        targetPosition.y += heightOffset;
        
        // For first frame or if canvas is far away, snap to position immediately
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget > 10f || Time.frameCount < 5)
        {
            // Snap immediately if too far away or on first few frames
            transform.position = targetPosition;
        }
        else
        {
            // Smoothly move to target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSmoothing);
        }
        
        // Rotate to face camera if billboard mode
        if (billboardMode)
        {
            Vector3 directionToCamera = (cameraPosition - transform.position).normalized;
            if (directionToCamera != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(-directionToCamera);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSmoothing);
            }
        }
    }
    
    /// <summary>
    /// Show a notification
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="duration">Duration before auto-hiding. Use 0 or negative to never auto-hide (default: 3f)</param>
    /// <param name="icon">Optional icon sprite</param>
    public UINotification ShowNotification(string message, float duration = 3f, Sprite icon = null)
    {
        if (notificationPrefab == null)
        {
            Debug.LogWarning("Notification prefab not assigned! Creating default notification.");
            return CreateDefaultNotification(message, duration, icon);
        }
        
        GameObject notifObj = Instantiate(notificationPrefab, notificationContainer);
        UINotification notification = notifObj.GetComponent<UINotification>();
        
        if (notification == null)
        {
            notification = notifObj.AddComponent<UINotification>();
        }
        
        notification.Setup(message, duration, icon);
        return notification;
    }
    
    /// <summary>
    /// Show a timer that counts down and hides when done
    /// </summary>
    public UINotification ShowTimer(string label, float duration, System.Action onComplete = null)
    {
        if (timerPrefab == null)
        {
            Debug.LogWarning("Timer prefab not assigned! Creating default timer.");
            return CreateDefaultTimer(label, duration, onComplete);
        }
        
        GameObject timerObj = Instantiate(timerPrefab, timerContainer);
        UINotification timer = timerObj.GetComponent<UINotification>();
        
        if (timer == null)
        {
            timer = timerObj.AddComponent<UINotification>();
        }
        
        timer.SetupTimer(label, duration, onComplete);
        return timer;
    }
    
    private UINotification CreateDefaultNotification(string message, float duration, Sprite icon)
    {
        // Create a simple default notification UI
        GameObject notifObj = new GameObject("Notification");
        notifObj.transform.SetParent(notificationContainer);
        
        // Add RectTransform
        RectTransform rect = notifObj.AddComponent<RectTransform>();
        rect.sizeDelta = notificationSize;
        
        // Position based on container anchor (will stack automatically)
        // For top positions, stack downward; for bottom, stack upward
        bool isTopPosition = notificationPosition == NotificationPosition.TopLeft || 
                             notificationPosition == NotificationPosition.TopCenter || 
                             notificationPosition == NotificationPosition.TopRight;
        
        if (isTopPosition)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            int childCount = notificationContainer.childCount;
            float yOffset = -childCount * (rect.sizeDelta.y + notificationSpacing);
            rect.anchoredPosition = new Vector2(0, yOffset);
        }
        else if (notificationPosition == NotificationPosition.BottomLeft || 
                 notificationPosition == NotificationPosition.BottomCenter || 
                 notificationPosition == NotificationPosition.BottomRight)
        {
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            int childCount = notificationContainer.childCount;
            float yOffset = childCount * (rect.sizeDelta.y + notificationSpacing);
            rect.anchoredPosition = new Vector2(0, yOffset);
        }
        else
        {
            // Middle positions - center anchor
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            int childCount = notificationContainer.childCount;
            float yOffset = childCount * (rect.sizeDelta.y + notificationSpacing);
            rect.anchoredPosition = new Vector2(0, yOffset);
        }
        
        // Add background image
        Image bg = notifObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(notifObj.transform);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = message;
        // Use custom font size if set, otherwise auto-calculate based on notification size
        if (notificationFontSize > 0)
        {
            text.fontSize = notificationFontSize;
        }
        else
        {
            text.fontSize = Mathf.Max(16, Mathf.Min(32, notificationSize.y * 0.3f));
        }
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true; // Allow text to wrap if too long
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        UINotification notification = notifObj.AddComponent<UINotification>();
        notification.Setup(message, duration, icon);
        return notification;
    }
    
    private UINotification CreateDefaultTimer(string label, float duration, System.Action onComplete)
    {
        // Create a simple default timer UI
        GameObject timerObj = new GameObject("Timer");
        timerObj.transform.SetParent(timerContainer);
        
        // Add RectTransform
        RectTransform rect = timerObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 80);
        
        // Position based on timer container anchor (separate from notifications)
        bool isTopPosition = timerPosition == NotificationPosition.TopLeft || 
                             timerPosition == NotificationPosition.TopCenter || 
                             timerPosition == NotificationPosition.TopRight;
        
        if (isTopPosition)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            int childCount = timerContainer.childCount;
            float yOffset = -childCount * (rect.sizeDelta.y + timerSpacing);
            rect.anchoredPosition = new Vector2(0, yOffset);
        }
        else if (timerPosition == NotificationPosition.BottomLeft || 
                 timerPosition == NotificationPosition.BottomCenter || 
                 timerPosition == NotificationPosition.BottomRight)
        {
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            int childCount = timerContainer.childCount;
            float yOffset = childCount * (rect.sizeDelta.y + timerSpacing);
            rect.anchoredPosition = new Vector2(0, yOffset);
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            int childCount = timerContainer.childCount;
            float yOffset = childCount * (rect.sizeDelta.y + timerSpacing);
            rect.anchoredPosition = new Vector2(0, yOffset);
        }
        
        // Add background image
        Image bg = timerObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(timerObj.transform);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = $"{label}: {duration:F1}s";
        // Use custom font size if set, otherwise auto-calculate based on timer size
        if (timerFontSize > 0)
        {
            text.fontSize = timerFontSize;
        }
        else
        {
            text.fontSize = Mathf.Max(16, Mathf.Min(32, timerSize.y * 0.35f));
        }
        text.color = Color.yellow;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true; // Allow text to wrap if too long
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        UINotification timer = timerObj.AddComponent<UINotification>();
        timer.SetupTimer(label, duration, onComplete);
        return timer;
    }
}

