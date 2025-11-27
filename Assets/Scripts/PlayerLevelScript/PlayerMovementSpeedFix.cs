using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Fixes common issues that cause player movement speed to be reduced.
/// Attach this to the XR Origin GameObject.
/// </summary>
public class PlayerMovementSpeedFix : MonoBehaviour
{
    [Header("Speed Protection")]
    [Tooltip("Prevent move speed from being changed below this value")]
    public float minimumMoveSpeed = 0.5f;
    
    [Tooltip("Check and restore move speed periodically")]
    public bool monitorSpeedChanges = true;
    public float checkInterval = 0.5f;
    
    [Header("Character Controller Fixes")]
    [Tooltip("Enable to prevent CharacterController from slowing down on collisions")]
    public bool preventCollisionSlowdown = true;
    
    [Header("Debug")]
    [Tooltip("Log when speed is being corrected")]
    public bool logCorrections = false;
    
    private ActionBasedContinuousMoveProvider moveProvider;
    private CharacterController characterController;
    private float originalMoveSpeed;
    private float lastCheckTime = 0f;
    private float storedMoveSpeed;
    
    void Start()
    {
        // Find Continuous Move Provider
        moveProvider = FindObjectOfType<ActionBasedContinuousMoveProvider>();
        if (moveProvider != null)
        {
            originalMoveSpeed = moveProvider.moveSpeed;
            storedMoveSpeed = originalMoveSpeed;
            Debug.Log($"[PlayerMovementSpeedFix] Monitoring move speed - Original: {originalMoveSpeed:F2}");
        }
        else
        {
            Debug.LogWarning("[PlayerMovementSpeedFix] Continuous Move Provider not found!");
            enabled = false;
            return;
        }
        
        // Find Character Controller
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = GetComponentInChildren<CharacterController>();
        }
        
        if (characterController != null && preventCollisionSlowdown)
        {
            // Ensure Character Controller settings are optimal
            // Increase step offset to prevent getting stuck on small obstacles
            if (characterController.stepOffset < 0.3f)
            {
                characterController.stepOffset = 0.3f;
                if (logCorrections)
                {
                    Debug.Log("[PlayerMovementSpeedFix] Increased Character Controller step offset to prevent slowdown");
                }
            }
            
            // Ensure skin width is appropriate
            if (characterController.skinWidth < 0.01f)
            {
                characterController.skinWidth = 0.01f;
                if (logCorrections)
                {
                    Debug.Log("[PlayerMovementSpeedFix] Adjusted Character Controller skin width");
                }
            }
        }
    }
    
    void Update()
    {
        if (!monitorSpeedChanges || moveProvider == null) return;
        
        // Periodically check and restore move speed
        if (Time.time - lastCheckTime >= checkInterval)
        {
            float currentMoveSpeed = moveProvider.moveSpeed;
            
            // Check if speed was reduced unexpectedly
            if (currentMoveSpeed < minimumMoveSpeed || currentMoveSpeed < storedMoveSpeed * 0.8f)
            {
                // Restore to stored speed
                moveProvider.moveSpeed = storedMoveSpeed;
                
                if (logCorrections)
                {
                    Debug.LogWarning($"[PlayerMovementSpeedFix] ⚠️ Move speed was reduced to {currentMoveSpeed:F2}, restored to {storedMoveSpeed:F2}");
                }
            }
            else if (Mathf.Abs(currentMoveSpeed - storedMoveSpeed) > 0.01f)
            {
                // Speed changed but still acceptable - update stored value
                storedMoveSpeed = currentMoveSpeed;
                if (logCorrections)
                {
                    Debug.Log($"[PlayerMovementSpeedFix] Move speed updated to {storedMoveSpeed:F2}");
                }
            }
            
            lastCheckTime = Time.time;
        }
        
        // Check Time.timeScale
        if (Time.timeScale < 1f && Time.timeScale > 0f)
        {
            Debug.LogWarning($"[PlayerMovementSpeedFix] ⚠️ Time.timeScale is {Time.timeScale:F2} - This will slow down movement!");
        }
    }
    
    /// <summary>
    /// Manually restore the original move speed
    /// </summary>
    public void RestoreOriginalSpeed()
    {
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = originalMoveSpeed;
            storedMoveSpeed = originalMoveSpeed;
            Debug.Log($"[PlayerMovementSpeedFix] Restored original move speed: {originalMoveSpeed:F2}");
        }
    }
    
    /// <summary>
    /// Set a new move speed and store it
    /// </summary>
    public void SetMoveSpeed(float newSpeed)
    {
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = newSpeed;
            storedMoveSpeed = newSpeed;
            Debug.Log($"[PlayerMovementSpeedFix] Set move speed to: {newSpeed:F2}");
        }
    }
    
    void OnEnable()
    {
        // Restore speed when component is enabled
        if (moveProvider != null && storedMoveSpeed > 0)
        {
            moveProvider.moveSpeed = storedMoveSpeed;
        }
    }
}

