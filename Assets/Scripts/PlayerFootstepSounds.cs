using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerFootstepSounds : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource walkingAudioSource;
    public AudioSource runningAudioSource;
    
    [Header("Footstep Sounds")]
    public AudioClip[] walkingSounds;
    public AudioClip[] runningSounds;
    
    [Header("Settings")]
    public float walkingSpeedThreshold = 0.1f; // Minimum speed to play walking sound
    public float runningSpeedThreshold = 2.5f; // Speed to switch from walking to running
    public float footstepInterval = 0.5f; // Time between footsteps (walking)
    public float runningFootstepInterval = 0.3f; // Time between footsteps (running)
    [Tooltip("Smoothing factor for speed calculation (0-1). Higher = smoother, prevents sudden stops.")]
    [Range(0f, 1f)]
    public float speedSmoothing = 0.7f; // Smooth speed changes to prevent sudden stops
    
    [Header("Volume")]
    [Range(0f, 1f)]
    public float walkingVolume = 0.5f;
    [Range(0f, 1f)]
    public float runningVolume = 0.7f;
    
    [Header("Movement Detection")]
    [Tooltip("How to detect movement: Position (tracks XR Origin), Input (tracks movement input), or Both")]
    public MovementDetectionMode detectionMode = MovementDetectionMode.Position;
    
    public enum MovementDetectionMode
    {
        Position,    // Track XR Origin position (works for both WASD and Continuous Move)
        Input,       // Track movement input from Continuous Move Provider
        Both         // Use both methods
    }
    
    private Vector3 lastPosition;
    private float currentSpeed = 0f;
    private float smoothedSpeed = 0f; // Smoothed speed to prevent sudden drops
    private float footstepTimer = 0f;
    private bool isWalking = false;
    private bool isRunning = false;
    
    // XR Components
    private Transform xrOrigin;
    private ActionBasedContinuousMoveProvider moveProvider;
    private CharacterController characterController;
    
    void Start()
    {
        // XR Origin is this GameObject (XR Origin (XR Rig))
        xrOrigin = transform;
        lastPosition = xrOrigin.position;
        
        // Try to find Continuous Move Provider (for input-based detection)
        moveProvider = FindObjectOfType<ActionBasedContinuousMoveProvider>();
        if (moveProvider != null)
        {
            Debug.Log("PlayerFootstepSounds: Found ActionBasedContinuousMoveProvider for input detection.");
        }
        
        // Try to find Character Controller (for velocity-based detection)
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = GetComponentInChildren<CharacterController>();
        }
        if (characterController != null)
        {
            Debug.Log("PlayerFootstepSounds: Found CharacterController for velocity detection.");
        }
        
        // Log detection mode
        Debug.Log($"PlayerFootstepSounds: Using {detectionMode} mode for movement detection.");
        Debug.Log($"PlayerFootstepSounds: XR Origin position tracking initialized at {xrOrigin.position}");
        
        // Setup Audio Sources if not assigned
        if (walkingAudioSource == null)
        {
            walkingAudioSource = gameObject.AddComponent<AudioSource>();
            walkingAudioSource.playOnAwake = false;
            walkingAudioSource.loop = false;
            walkingAudioSource.spatialBlend = 0f; // 2D sound (or set to 1f for 3D)
        }
        
        if (runningAudioSource == null && runningSounds.Length > 0)
        {
            runningAudioSource = gameObject.AddComponent<AudioSource>();
            runningAudioSource.playOnAwake = false;
            runningAudioSource.loop = false;
            runningAudioSource.spatialBlend = 0f; // 2D sound (or set to 1f for 3D)
        }
        
        // Set volumes
        if (walkingAudioSource != null)
        {
            walkingAudioSource.volume = walkingVolume;
        }
        if (runningAudioSource != null)
        {
            runningAudioSource.volume = runningVolume;
        }
    }
    
    void Update()
    {
        float deltaTime = Time.deltaTime;
        float positionSpeed = 0f;
        float inputSpeed = 0f;
        
        // Method 1: Track XR Origin position (works for both WASD and Continuous Move)
        if (detectionMode == MovementDetectionMode.Position || detectionMode == MovementDetectionMode.Both)
        {
            Vector3 currentPosition = xrOrigin.position;
            if (deltaTime > 0)
            {
                Vector3 movement = currentPosition - lastPosition;
                movement.y = 0f; // Ignore vertical movement (jumping, crouching)
                positionSpeed = movement.magnitude / deltaTime;
            }
            lastPosition = currentPosition;
        }
        
        // Method 2: Track movement input from Continuous Move Provider
        if ((detectionMode == MovementDetectionMode.Input || detectionMode == MovementDetectionMode.Both) && moveProvider != null)
        {
            // Get movement input from the provider
            // Note: ActionBasedContinuousMoveProvider doesn't expose input directly,
            // so we'll use a workaround by checking if it's enabled and moving
            // For now, we'll rely on position tracking, but this gives us the structure
            
            // Alternative: Use Character Controller velocity if available
            if (characterController != null)
            {
                Vector3 velocity = characterController.velocity;
                velocity.y = 0f; // Ignore vertical
                inputSpeed = velocity.magnitude;
            }
        }
        
        // Combine speeds based on detection mode
        if (detectionMode == MovementDetectionMode.Both)
        {
            currentSpeed = Mathf.Max(positionSpeed, inputSpeed);
        }
        else if (detectionMode == MovementDetectionMode.Input)
        {
            currentSpeed = inputSpeed;
            // Fallback to position if input not available
            if (currentSpeed < 0.01f)
            {
                Vector3 currentPosition = xrOrigin.position;
                if (deltaTime > 0)
                {
                    Vector3 movement = currentPosition - lastPosition;
                    movement.y = 0f;
                    currentSpeed = movement.magnitude / deltaTime;
                }
                lastPosition = currentPosition;
            }
        }
        else
        {
            currentSpeed = positionSpeed;
        }
        
        // Smooth speed to prevent sudden drops (which stop footsteps)
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, currentSpeed, 1f - speedSmoothing);
        
        // Use smoothed speed for state determination (prevents brief stops from cutting off footsteps)
        float speedForState = smoothedSpeed;
        
        // Debug speed occasionally
        if (Time.frameCount % 60 == 0) // Every 60 frames
        {
            Debug.Log($"PlayerFootstepSounds: Raw Speed = {currentSpeed:F2}, Smoothed Speed = {smoothedSpeed:F2}, Position Speed = {positionSpeed:F2}, Input Speed = {inputSpeed:F2}, Is Walking = {isWalking}, Is Running = {isRunning}");
        }
        
        // Determine state using smoothed speed
        bool wasWalking = isWalking;
        bool wasRunning = isRunning;
        
        isWalking = speedForState >= walkingSpeedThreshold && speedForState < runningSpeedThreshold;
        isRunning = speedForState >= runningSpeedThreshold;
        
        // Update footstep timer
        if (isWalking || isRunning)
        {
            footstepTimer += deltaTime;
            
            float interval = isRunning ? runningFootstepInterval : footstepInterval;
            
            if (footstepTimer >= interval)
            {
                PlayFootstep(isRunning);
                footstepTimer = 0f;
            }
        }
        else
        {
            // Reset timer when not moving
            footstepTimer = 0f;
        }
        
        // Stop sounds when not moving (but only if speed has been low for a moment)
        // This prevents brief stops from cutting off footsteps
        if (!isWalking && !isRunning)
        {
            // Only stop if we've been stopped for a short time (prevents cutting off during brief pauses)
            if (smoothedSpeed < walkingSpeedThreshold * 0.5f) // Only stop if speed is really low
            {
                if (walkingAudioSource != null && walkingAudioSource.isPlaying)
                {
                    walkingAudioSource.Stop();
                }
                if (runningAudioSource != null && runningAudioSource.isPlaying)
                {
                    runningAudioSource.Stop();
                }
            }
        }
    }
    
    void PlayFootstep(bool isRunningSound)
    {
        AudioClip[] soundArray = isRunningSound ? runningSounds : walkingSounds;
        AudioSource audioSource = isRunningSound ? runningAudioSource : walkingAudioSource;
        
        if (soundArray.Length == 0 || audioSource == null)
        {
            return;
        }
        
        // Pick random sound from array
        AudioClip clipToPlay = soundArray[Random.Range(0, soundArray.Length)];
        
        // Play sound
        audioSource.PlayOneShot(clipToPlay);
    }
    
    // Public method to manually trigger footstep (useful for testing)
    public void PlayFootstepManually(bool isRunning = false)
    {
        PlayFootstep(isRunning);
    }
    
    // Get current movement speed (for debugging)
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

}

