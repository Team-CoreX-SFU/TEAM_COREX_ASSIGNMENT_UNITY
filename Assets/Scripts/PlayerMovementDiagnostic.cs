using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Diagnostic script to track and log player movement speed issues.
/// Attach this to the XR Origin to diagnose movement speed problems.
/// </summary>
public class PlayerMovementDiagnostic : MonoBehaviour
{
    [Header("Diagnostics")]
    [Tooltip("Enable to log detailed movement information")]
    public bool enableLogging = true;
    [Tooltip("Interval between logs (seconds)")]
    public float logInterval = 2f;
    
    [Header("Speed Monitoring")]
    [Tooltip("Minimum speed before warning is logged")]
    public float minimumExpectedSpeed = 0.5f;
    
    private ActionBasedContinuousMoveProvider moveProvider;
    private CharacterController characterController;
    private Transform xrOrigin;
    
    private float lastLogTime = 0f;
    private Vector3 lastPosition;
    private float actualSpeed = 0f;
    private float moveSpeedSetting = 0f;
    
    void Start()
    {
        xrOrigin = transform;
        lastPosition = xrOrigin.position;
        
        // Find Continuous Move Provider
        moveProvider = FindObjectOfType<ActionBasedContinuousMoveProvider>();
        if (moveProvider != null)
        {
            moveSpeedSetting = moveProvider.moveSpeed;
            Debug.Log($"[PlayerMovementDiagnostic] Found Continuous Move Provider with moveSpeed: {moveSpeedSetting}");
        }
        else
        {
            Debug.LogWarning("[PlayerMovementDiagnostic] Continuous Move Provider not found!");
        }
        
        // Find Character Controller
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = GetComponentInChildren<CharacterController>();
        }
        
        if (characterController != null)
        {
            Debug.Log($"[PlayerMovementDiagnostic] Found Character Controller - Radius: {characterController.radius}, Height: {characterController.height}");
        }
        else
        {
            Debug.LogWarning("[PlayerMovementDiagnostic] Character Controller not found!");
        }
    }
    
    void Update()
    {
        if (!enableLogging) return;
        
        // Calculate actual movement speed
        Vector3 currentPosition = xrOrigin.position;
        float deltaTime = Time.deltaTime;
        
        if (deltaTime > 0)
        {
            Vector3 movement = currentPosition - lastPosition;
            movement.y = 0f; // Ignore vertical movement
            actualSpeed = movement.magnitude / deltaTime;
        }
        
        lastPosition = currentPosition;
        
        // Check if move speed setting changed
        if (moveProvider != null)
        {
            float currentMoveSpeed = moveProvider.moveSpeed;
            if (Mathf.Abs(currentMoveSpeed - moveSpeedSetting) > 0.01f)
            {
                Debug.LogWarning($"[PlayerMovementDiagnostic] ⚠️ Move Speed Changed! Old: {moveSpeedSetting:F2}, New: {currentMoveSpeed:F2}");
                moveSpeedSetting = currentMoveSpeed;
            }
        }
        
        // Check Character Controller velocity
        float controllerVelocity = 0f;
        if (characterController != null)
        {
            Vector3 velocity = characterController.velocity;
            velocity.y = 0f;
            controllerVelocity = velocity.magnitude;
        }
        
        // Log diagnostics
        if (Time.time - lastLogTime >= logInterval)
        {
            string status = "✓ Normal";
            if (actualSpeed < minimumExpectedSpeed && controllerVelocity > 0.1f)
            {
                status = "⚠️ Speed Reduced (Character Controller moving but position speed low)";
            }
            else if (actualSpeed < minimumExpectedSpeed)
            {
                status = "⚠️ Speed Reduced (No movement detected)";
            }
            
            Debug.Log($"[PlayerMovementDiagnostic] {status}\n" +
                     $"  Position Speed: {actualSpeed:F3} m/s\n" +
                     $"  Controller Velocity: {controllerVelocity:F3} m/s\n" +
                     $"  Move Speed Setting: {moveSpeedSetting:F2}\n" +
                     $"  Time Scale: {Time.timeScale:F2}\n" +
                     $"  Character Controller Enabled: {characterController != null && characterController.enabled}");
            
            lastLogTime = Time.time;
        }
        
        // Check for common issues
        if (Time.timeScale != 1f && Time.timeScale > 0f)
        {
            Debug.LogWarning($"[PlayerMovementDiagnostic] ⚠️ Time.timeScale is {Time.timeScale:F2} (not 1.0) - This affects movement speed!");
        }
    }
    
    /// <summary>
    /// Get the current actual movement speed
    /// </summary>
    public float GetActualSpeed()
    {
        return actualSpeed;
    }
    
    /// <summary>
    /// Get the move speed setting from the Continuous Move Provider
    /// </summary>
    public float GetMoveSpeedSetting()
    {
        return moveSpeedSetting;
    }
}

