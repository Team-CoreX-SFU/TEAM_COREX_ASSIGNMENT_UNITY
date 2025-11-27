using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class KidnapperAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    [Tooltip("Buffer added to attack range to account for floating point precision and ensure attack triggers reliably.")]
    public float attackRangeBuffer = 0.15f;
    [Tooltip("Distance threshold for immediate detection regardless of FOV/line of sight. Player within this range will be detected immediately.")]
    public float veryCloseDetectionThreshold = 3f;
    [Tooltip("Additional distance before attack range where agent stops to prevent pushing player. Set to 0 to disable.")]
    public float safeDistanceOffset = 0.5f;
    [Tooltip("Safe distance to maintain from targets (player or sound sources) to prevent pushing. Used for both player detection/chasing and sound investigation.")]
    public float safeDistance = 2.5f;
    [Tooltip("Multiplier for attack range to detect from behind when very close (default: 2.0 = can detect from behind within 2x attack range).")]
    public float detectFromBehindMultiplier = 2f;
    [Tooltip("Multiplier for attack range to detect extremely close players regardless of angle (default: 1.5 = within 1.5x attack range).")]
    public float extremelyCloseMultiplier = 1.5f;
    [Tooltip("Multiplier for detection range when chasing - extended range to keep chasing even if temporarily out of range (default: 1.5 = 1.5x detection range).")]
    public float chaseDetectionRangeMultiplier = 1.5f;
    [Tooltip("Multiplier for attack range to check if player moved too far away during attack (default: 1.5 = switch back to chase if player > 1.5x attack range).")]
    public float attackDisengageMultiplier = 1.5f;
    public float fieldOfView = 120f; // Increased default FOV to 120 degrees (60 on each side)
    public float patrolWaitTime = 2f;
    [Tooltip("If true, can detect player from any angle (360 degrees). If false, uses Field of View.")]
    public bool detectFromAllAngles = false;

    [Header("Sound Detection")]
    [Tooltip("Maximum distance to hear player running footsteps.")]
    public float footstepHearingRange = 15f;
    [Tooltip("Maximum distance to hear radio sounds.")]
    public float radioHearingRange = 25f;
    [Tooltip("Time to investigate footstep sounds before returning to patrol if nothing found.")]
    public float footstepInvestigationDuration = 5f;
    [Tooltip("Time to investigate radio sounds before returning to patrol (only if radio stops).")]
    public float radioInvestigationDuration = 8f;
    [Tooltip("If true, this kidnapper will investigate radio sounds. If false, will ignore radio sounds.")]
    public bool canInvestigateRadio = true;

    [Header("Patrol Points")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Player Detection")]
    public LayerMask playerLayer = 1; // Default layer
    public LayerMask obstacleLayer = 1; // For line of sight checks
    [Tooltip("Layers that should block line of sight to player (e.g., Wall layer).")]
    public LayerMask wallLayer = 1 << 7; // Wall layer (index 7) - blocks detection

    [Header("Animation")]
    public Animator animator;
    private string animSpeedParam = "Speed";
    private string animAttackParam = "Attack";
    private string animIdleParam = "Idle";

    [Header("Components")]
    private NavMeshAgent agent;
    private Transform player;
    private bool playerDetected = false;
    private bool isAttacking = false;

    public enum AIState
    {
        Patrolling,
        Chasing,
        Attacking,
        Searching,
        InvestigatingSound
    }

    [Header("State")]
    public AIState currentState = AIState.Patrolling;

    private Vector3 lastKnownPlayerPosition;
    private float searchTimer = 0f;
    private float searchDuration = 5f;
    private bool isWaitingAtPatrolPoint = false; // Prevent multiple coroutines
    private float pathUpdateInterval = 0.25f; // Update path every 0.25 seconds for smoother following
    private float lastPathUpdateTime = 0f;
    private Vector3 lastDestination = Vector3.zero;

    // Sound detection
    private PlayerFootstepSounds playerFootsteps;
    private RadioController[] radios;
    private Vector3 soundSourcePosition;
    private float soundInvestigationTimer = 0f;
    private bool isInvestigatingSound = false;
    private enum SoundType { Footstep, Radio }
    private SoundType currentSoundType = SoundType.Footstep;
    private RadioController investigatingRadio = null; // Track which radio we're investigating

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        // Find player by tag (reusable for multiple purposes)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            // Try to find the camera offset or main camera as player reference
            Transform cameraOffset = playerObj.transform.Find("Camera Offset");
            if (cameraOffset != null)
            {
                Transform mainCamera = cameraOffset.Find("Main Camera");
                if (mainCamera != null)
                {
                    player = mainCamera;
                }
                else
                {
                    player = cameraOffset;
                }
            }
            else
            {
                player = playerObj.transform;
            }
        }

        // Setup NavMesh Agent
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.5f; // Changed from attackRange to allow closer approach
        agent.height = 2f;
        agent.radius = 0.5f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // Multi-floor navigation settings
        agent.baseOffset = 0f; // Adjust if agent is floating or sinking
        agent.acceleration = 12f; // Increased for smoother acceleration
        agent.angularSpeed = 240f; // Increased for faster, smoother turning
        agent.autoBraking = false; // Disable auto-braking for smoother movement
        agent.autoRepath = true; // Automatically recalculate path if blocked
        agent.autoTraverseOffMeshLink = true; // Automatically use off-mesh links (jumps, etc.)

        // Make sure agent is enabled
        agent.enabled = true;

        // Warn if agent is not on NavMesh
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"Kidnapper NavMesh Agent is NOT on NavMesh! Position: {transform.position}. Move the kidnapper to a blue NavMesh area.");
        }

        // Warn about obstacles
        Debug.Log("KidnapperAI: Make sure all obstacles (furniture, walls) are marked as 'Navigation Static' and NavMesh is re-baked, or the kidnapper will walk through them!");

        // Setup Animator
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Start patrolling
        if (patrolPoints.Length > 0 && patrolPoints[currentPatrolIndex] != null)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            Debug.Log($"Initial destination set to patrol point {currentPatrolIndex} at {patrolPoints[currentPatrolIndex].position}");
        }
        else
        {
            Debug.LogWarning("No valid patrol points assigned!");
        }

        // Find PlayerFootstepSounds component (reuse playerObj from earlier)
        if (playerObj != null)
        {
            playerFootsteps = playerObj.GetComponent<PlayerFootstepSounds>();
            if (playerFootsteps == null)
            {
                playerFootsteps = playerObj.GetComponentInChildren<PlayerFootstepSounds>();
            }
            if (playerFootsteps != null)
            {
                Debug.Log("KidnapperAI: Found PlayerFootstepSounds component for sound detection.");
            }
        }

        // Find all RadioController components in the scene
        radios = FindObjectsOfType<RadioController>();
        if (radios != null && radios.Length > 0)
        {
            Debug.Log($"KidnapperAI: Found {radios.Length} RadioController(s) for sound detection.");
        }

        // Debug log to verify setup
        Debug.Log($"KidnapperAI initialized. Patrol points: {patrolPoints.Length}, Player found: {player != null}, Agent enabled: {agent.enabled}, Agent on NavMesh: {agent.isOnNavMesh}");
    }

    void Update()
    {
        // Check for player detection FIRST (highest priority - visual detection overrides sound investigation)
        CheckForPlayer();
        
        // If player detected and in attack range, don't check sounds (already attacking/chasing)
        // Otherwise, check for sounds (unless already chasing or attacking)
        if (currentState != AIState.Chasing && currentState != AIState.Attacking)
        {
            CheckForSounds();
        }

        // Update state machine
        switch (currentState)
        {
            case AIState.Patrolling:
                Patrol();
                break;
            case AIState.Chasing:
                Chase();
                break;
            case AIState.Attacking:
                Attack();
                break;
            case AIState.Searching:
                Search();
                break;
            case AIState.InvestigatingSound:
                InvestigateSound();
                break;
        }

        // Update animations
        UpdateAnimations();
    }

    void CheckForSounds()
    {
        bool soundDetected = false;
        Vector3 detectedSoundPosition = Vector3.zero;

        // Check for player running footstep sounds
        if (playerFootsteps != null && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Check if player is running (speed above running threshold)
            // When player is running, footstep sounds are playing, so kidnappers can hear them
            float playerSpeed = playerFootsteps.GetCurrentSpeed();
            float runningThreshold = playerFootsteps.runningSpeedThreshold;
            bool isPlayerRunning = playerSpeed >= runningThreshold;

            if (isPlayerRunning && distanceToPlayer <= footstepHearingRange)
            {
                soundDetected = true;
                detectedSoundPosition = player.position;
                
                if (!isInvestigatingSound || currentState != AIState.InvestigatingSound || currentSoundType != SoundType.Footstep)
                {
                    Debug.Log($"KidnapperAI: Heard player running footsteps at distance {distanceToPlayer:F2}. Investigating!");
                    isInvestigatingSound = true;
                    soundSourcePosition = player.position;
                    soundInvestigationTimer = 0f;
                    currentSoundType = SoundType.Footstep;
                    investigatingRadio = null; // Not investigating radio
                    currentState = AIState.InvestigatingSound;
                    lastKnownPlayerPosition = player.position; // Update last known position
                }
                else if (currentSoundType == SoundType.Footstep)
                {
                    // Update sound position if already investigating footsteps
                    soundSourcePosition = player.position;
                    lastKnownPlayerPosition = player.position;
                    soundInvestigationTimer = 0f; // Reset timer while still hearing footsteps
                }
            }
        }

        // Check for radio sounds (only if this kidnapper can investigate radio)
        if (canInvestigateRadio && radios != null && radios.Length > 0)
        {
            foreach (RadioController radio in radios)
            {
                if (radio == null) continue;

                // Check if radio sound is actually playing (after delay)
                if (radio.IsSoundPlaying())
                {
                    float distanceToRadio = Vector3.Distance(transform.position, radio.transform.position);
                    
                    if (distanceToRadio <= radioHearingRange)
                    {
                        soundDetected = true;
                        Vector3 radioPosition = radio.transform.position;
                        
                        // Use radio position - radio takes priority over footsteps
                        if (!isInvestigatingSound || currentState != AIState.InvestigatingSound)
                        {
                            detectedSoundPosition = radioPosition;
                            Debug.Log($"KidnapperAI: Heard radio sound at distance {distanceToRadio:F2}. Investigating!");
                            isInvestigatingSound = true;
                            soundSourcePosition = radioPosition;
                            soundInvestigationTimer = 0f;
                            currentSoundType = SoundType.Radio;
                            investigatingRadio = radio; // Track which radio
                            currentState = AIState.InvestigatingSound;
                        }
                        else if (currentSoundType == SoundType.Radio && investigatingRadio == radio)
                        {
                            // Still investigating same radio, update position
                            soundSourcePosition = radioPosition;
                            soundInvestigationTimer = 0f; // Reset timer while radio is playing
                        }
                        else if (currentSoundType == SoundType.Footstep)
                        {
                            // Switch from footstep to radio (radio takes priority)
                            Debug.Log($"KidnapperAI: Switching from footstep to radio investigation at distance {distanceToRadio:F2}.");
                            soundSourcePosition = radioPosition;
                            soundInvestigationTimer = 0f;
                            currentSoundType = SoundType.Radio;
                            investigatingRadio = radio;
                            currentState = AIState.InvestigatingSound;
                        }
                        else if (Vector3.Distance(transform.position, radioPosition) < Vector3.Distance(transform.position, soundSourcePosition))
                        {
                            // Different radio is closer, investigate that instead
                            detectedSoundPosition = radioPosition;
                            soundSourcePosition = radioPosition;
                            currentSoundType = SoundType.Radio;
                            investigatingRadio = radio;
                            soundInvestigationTimer = 0f;
                        }
                    }
                }
            }
        }

        // Handle investigation timeout based on sound type
        if (currentState == AIState.InvestigatingSound)
        {
            if (currentSoundType == SoundType.Footstep)
            {
                // For footsteps: stop investigating if player stopped running
                if (!soundDetected)
                {
                    // Player stopped running, start timer
                    soundInvestigationTimer += Time.deltaTime;
                    if (soundInvestigationTimer >= footstepInvestigationDuration)
                    {
                        Debug.Log("KidnapperAI: Footstep investigation complete (no running detected). Returning to patrol.");
                        isInvestigatingSound = false;
                        currentState = AIState.Patrolling;
                        soundInvestigationTimer = 0f;
                        currentSoundType = SoundType.Footstep;
                        investigatingRadio = null;
                    }
                }
                // If soundDetected is true, timer was reset above, so continue investigating
            }
            else if (currentSoundType == SoundType.Radio)
            {
                // For radio: check if the radio we're investigating is still playing
                if (investigatingRadio != null)
                {
                    if (!investigatingRadio.IsSoundPlaying())
                    {
                        // Radio stopped playing
                        soundInvestigationTimer += Time.deltaTime;
                        if (soundInvestigationTimer >= radioInvestigationDuration)
                        {
                            Debug.Log("KidnapperAI: Radio stopped playing. Investigation complete. Returning to patrol.");
                            isInvestigatingSound = false;
                            currentState = AIState.Patrolling;
                            soundInvestigationTimer = 0f;
                            investigatingRadio = null;
                            currentSoundType = SoundType.Footstep;
                        }
                    }
                    else
                    {
                        // Radio still playing, reset timer
                        soundInvestigationTimer = 0f;
                    }
                }
                else
                {
                    // Radio reference lost, return to patrol after delay
                    soundInvestigationTimer += Time.deltaTime;
                    if (soundInvestigationTimer >= radioInvestigationDuration)
                    {
                        Debug.Log("KidnapperAI: Radio investigation complete (reference lost). Returning to patrol.");
                        isInvestigatingSound = false;
                        currentState = AIState.Patrolling;
                        soundInvestigationTimer = 0f;
                        investigatingRadio = null;
                        currentSoundType = SoundType.Footstep;
                    }
                }
            }
        }
    }

    // Helper function to check if there's a wall between kidnapper and target position
    private bool HasWallBetween(Vector3 start, Vector3 end, float maxDistance)
    {
        Vector3 direction = (end - start).normalized;
        RaycastHit[] hits = Physics.RaycastAll(start, direction, maxDistance);
        
        // Sort hits by distance
        for (int i = 0; i < hits.Length - 1; i++)
        {
            for (int j = i + 1; j < hits.Length; j++)
            {
                if (hits[i].distance > hits[j].distance)
                {
                    RaycastHit temp = hits[i];
                    hits[i] = hits[j];
                    hits[j] = temp;
                }
            }
        }
        
        // Check all hits to see if a wall is between kidnapper and target
        foreach (RaycastHit hit in hits)
        {
            int wallLayerIndex = LayerMask.NameToLayer("Wall");
            bool isWall = hit.collider.gameObject.layer == wallLayerIndex ||
                          (wallLayer.value & (1 << hit.collider.gameObject.layer)) != 0 ||
                          hit.collider.CompareTag("Wall");
            
            bool isPlayer = hit.collider.CompareTag("Player") ||
                           hit.collider.transform == player ||
                           hit.collider.transform.IsChildOf(player) ||
                           (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag("Player")) ||
                           hit.collider.transform.root.CompareTag("Player");
            
            // If we hit a wall before the player, wall blocks
            if (isWall && !isPlayer)
            {
                return true; // Wall found, blocks line of sight
            }
            else if (isPlayer)
            {
                return false; // Player found before wall, no wall blocking
            }
        }
        
        return false; // No wall found
    }

    void CheckForPlayer()
    {
        if (player == null)
        {
            // Try to find player again if lost
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Transform cameraOffset = playerObj.transform.Find("Camera Offset");
                if (cameraOffset != null)
                {
                    Transform mainCamera = cameraOffset.Find("Main Camera");
                    if (mainCamera != null)
                    {
                        player = mainCamera;
                    }
                    else
                    {
                        player = cameraOffset;
                    }
                }
                else
                {
                    player = playerObj.transform;
                }
            }
            else
            {
                return; // Player not found
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Note: Attack range check is now handled in the main detection section below
        // This ensures walls are checked first before any detection/attack
        
        // Check if player is in detection range (removed distance restriction for initial detection)
        if (true) // Always check for player detection regardless of distance
        {
            // Check if player is in field of view
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            // Debug FOV check
            if (Time.frameCount % 30 == 0) // Every 30 frames
            {
                Debug.Log($"Player check - Distance: {distanceToPlayer:F2}, Angle: {angleToPlayer:F2}, FOV limit: {fieldOfView / 2f:F2}, In range: {distanceToPlayer <= detectionRange}, In FOV: {angleToPlayer <= fieldOfView / 2f}");
            }

            // Check FOV - allow detection from all angles if enabled
            bool inFOV = detectFromAllAngles || angleToPlayer <= fieldOfView / 2f;

            // Check if player is in field of view
            if (inFOV)
            {
                // Check line of sight - walls should ALWAYS block detection, even if very close
                // Use RaycastAll to check all objects between kidnapper and player
                Vector3 rayStart = transform.position + Vector3.up * 1.5f; // Eye level
                Vector3 rayDirection = directionToPlayer;

                // Cast ray to check line of sight - check all layers to detect walls
                int layerMask = ~0; // All layers
                bool hasLineOfSight = false;
                bool wallDetected = false;

                // Cast ray toward player position (use distance to player as max distance)
                // Use RaycastAll to get all objects between kidnapper and player
                RaycastHit[] hits = Physics.RaycastAll(rayStart, rayDirection, distanceToPlayer, layerMask);
                
                // Sort hits by distance to check closest objects first
                // This ensures we check if a wall is hit before the player
                for (int i = 0; i < hits.Length - 1; i++)
                {
                    for (int j = i + 1; j < hits.Length; j++)
                    {
                        if (hits[i].distance > hits[j].distance)
                        {
                            RaycastHit temp = hits[i];
                            hits[i] = hits[j];
                            hits[j] = temp;
                        }
                    }
                }
                
                // Check all hits in order of distance to see if a wall is between kidnapper and player
                // Walls ALWAYS block detection, regardless of distance - even if very close
                bool playerHit = false;
                foreach (RaycastHit hit in hits)
                {
                    // Check if we hit a wall (check both layer and tag)
                    int wallLayerIndex = LayerMask.NameToLayer("Wall");
                    bool isWall = hit.collider.gameObject.layer == wallLayerIndex ||
                                  (wallLayer.value & (1 << hit.collider.gameObject.layer)) != 0 ||
                                  hit.collider.CompareTag("Wall");

                    // Check if we hit the player or something with Player tag/component
                    bool isPlayer = hit.collider.CompareTag("Player") ||
                                   hit.collider.transform == player ||
                                   hit.collider.transform.IsChildOf(player) ||
                                   (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag("Player")) ||
                                   hit.collider.transform.root.CompareTag("Player");

                    // If we hit a wall BEFORE hitting the player, wall ALWAYS blocks line of sight
                    if (isWall && !playerHit)
                    {
                        wallDetected = true;
                        hasLineOfSight = false;
                        if (Time.frameCount % 60 == 0)
                        {
                            Debug.Log($"Line of sight blocked by WALL: {hit.collider.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}) at distance {hit.distance:F2}. Player cannot be detected through walls, even if very close!");
                        }
                        break; // Wall found before player - wall ALWAYS blocks detection
                    }
                    else if (isPlayer)
                    {
                        // Hit player - if we got here without hitting a wall first, we have line of sight
                        playerHit = true;
                        if (!wallDetected)
                        {
                            hasLineOfSight = true;
                        }
                    }
                }

                // If no hits at all, we have clear line of sight
                if (hits.Length == 0)
                {
                    hasLineOfSight = true;
                }
                else if (!wallDetected && !playerHit && !hasLineOfSight)
                {
                    // We hit something but it wasn't a wall or player - no line of sight
                    hasLineOfSight = false;
                    if (Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"Line of sight blocked by obstacle (not wall): {hits[0].collider.name}, Tag: {hits[0].collider.tag}, Layer: {LayerMask.LayerToName(hits[0].collider.gameObject.layer)}");
                    }
                }

                // If we have line of sight (or can detect through walls), detect the player
                if (hasLineOfSight)
                {
                    // Check if this is the first time detecting the player
                    bool wasDetectedBefore = playerDetected;
                    playerDetected = true;
                    lastKnownPlayerPosition = player.position;

                    // Trigger game over when player is first detected
                    if (!wasDetectedBefore)
                    {
                        Debug.Log("Player detected! Game Over!");
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.PlayerCaught();
                        }
                        else
                        {
                            Debug.LogWarning("GameManager.Instance is null! Game over cannot be triggered.");
                        }
                    }

                    // Player detected with line of sight (no wall blocking) - trigger game over immediately
                    // Then allow chase/attack states for animation purposes
                    
                    // Determine state based on distance
                    float attackRangeWithBuffer = attackRange + attackRangeBuffer;
                    if (distanceToPlayer <= attackRangeWithBuffer && currentState != AIState.Attacking)
                    {
                        // Player in attack range - switch to attack state for animation
                        Debug.Log($"Player in attack range! Distance: {distanceToPlayer:F2}. Switching to attack state.");
                        
                        // Stop sound investigation if was investigating
                        if (currentState == AIState.InvestigatingSound)
                        {
                            isInvestigatingSound = false;
                            soundInvestigationTimer = 0f;
                        }
                        
                        currentState = AIState.Attacking;
                        agent.isStopped = true;
                    }
                    else if (currentState != AIState.Attacking && currentState != AIState.Chasing)
                    {
                        // Not in attack range, but should chase for animation
                        Debug.Log($"Player detected visually! Distance: {distanceToPlayer:F2}, Angle: {angleToPlayer:F2}. Switching to chase state.");
                        
                        // Stop sound investigation if was investigating
                        if (currentState == AIState.InvestigatingSound)
                        {
                            isInvestigatingSound = false;
                            soundInvestigationTimer = 0f;
                        }
                        
                        currentState = AIState.Chasing;
                    }
                    return;
                }
            }
        }

        // Keep tracking player position during chase/attack for animation purposes
        // Game over already triggered on first detection, this is just for visual/animation
        if (currentState == AIState.Chasing || currentState == AIState.Attacking)
        {
            if (player != null)
            {
                lastKnownPlayerPosition = player.position;
                playerDetected = true; // Keep flag true for chase/attack states
            }
        }
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints.Length == 0)
        {
            // If no patrol points, just idle
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        // Check if agent has a path
        if (!agent.hasPath && !agent.pathPending && !isWaitingAtPatrolPoint)
        {
            // Set destination to current patrol point
            if (patrolPoints[currentPatrolIndex] != null)
            {
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                Debug.Log($"Setting destination to patrol point {currentPatrolIndex} at {patrolPoints[currentPatrolIndex].position}");
            }
        }

        // Check if reached patrol point (only if not already waiting)
        if (!isWaitingAtPatrolPoint && !agent.pathPending && agent.hasPath && agent.remainingDistance < 0.5f && agent.remainingDistance != Mathf.Infinity)
        {
            Debug.Log($"Reached patrol point {currentPatrolIndex}, waiting...");
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    IEnumerator WaitAtPatrolPoint()
    {
        isWaitingAtPatrolPoint = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(patrolWaitTime);

        // Move to next patrol point
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        // Make sure patrol point is valid
        if (patrolPoints[currentPatrolIndex] != null)
        {
            agent.isStopped = false;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            Debug.Log($"Moving to next patrol point {currentPatrolIndex} at {patrolPoints[currentPatrolIndex].position}");
        }
        else
        {
            Debug.LogWarning($"Patrol point {currentPatrolIndex} is null!");
        }

        isWaitingAtPatrolPoint = false;
    }

    void Chase()
    {
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        isWaitingAtPatrolPoint = false; // Reset patrol wait flag

        // Always try to chase if we have a player reference and are in chase state
        // Check both playerDetected and direct distance check to handle line-of-sight blocking
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Player already detected (game over triggered) - chase is for animation only
            // Check attack range and switch to attack state for animation
            float attackRangeWithBuffer = attackRange + attackRangeBuffer;
            if (distanceToPlayer <= attackRangeWithBuffer)
            {
                // Check for wall before attacking (for realism during animation)
                Vector3 rayStart = transform.position + Vector3.up * 1.5f;
                Vector3 playerEyeLevel = player.position + Vector3.up * 1.5f;
                bool wallBlocking = HasWallBetween(rayStart, playerEyeLevel, distanceToPlayer);
                
                if (!wallBlocking && currentState != AIState.Attacking)
                {
                    // No wall - switch to attack state for animation
                    Debug.Log($"Chase: Player in attack range! Switching to attack state. Distance: {distanceToPlayer:F2}");
                    currentState = AIState.Attacking;
                    agent.isStopped = true;
                    return;
                }
                // If wall blocking, continue chasing (player might have moved behind wall)
            }
            
            if (playerDetected) // Chase based on detection flag
            {
            
            // Continue chasing - no distance-based stopping
            agent.isStopped = false;

            // Update path more frequently for smoother following
            float timeSinceLastUpdate = Time.time - lastPathUpdateTime;
            bool shouldUpdatePath = timeSinceLastUpdate >= pathUpdateInterval;

            // Also update if player moved significantly
            float distanceToLastDestination = Vector3.Distance(player.position, lastDestination);
            bool playerMovedSignificantly = distanceToLastDestination > 1f;

            if (shouldUpdatePath || playerMovedSignificantly || !agent.hasPath)
            {
                // Check if player position is on NavMesh
                UnityEngine.AI.NavMeshHit hit;
                bool playerOnNavMesh = UnityEngine.AI.NavMesh.SamplePosition(player.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas);

                Vector3 targetPosition;

                if (playerOnNavMesh)
                {
                    // Player is on NavMesh, set destination directly to player position
                    // Removed safe distance - chase directly to player
                    targetPosition = hit.position;
                }
                else
                {
                    // Player not on NavMesh, find nearest NavMesh point
                    UnityEngine.AI.NavMeshHit nearestHit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(player.position, out nearestHit, 10f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        targetPosition = nearestHit.position;
                    }
                    else
                    {
                        // Can't find NavMesh near player, use player position directly
                        targetPosition = player.position;
                    }
                }

                // Only update if destination changed significantly
                if (Vector3.Distance(targetPosition, lastDestination) > 0.5f || !agent.hasPath)
                {
                    agent.SetDestination(targetPosition);
                    lastDestination = targetPosition;
                    lastPathUpdateTime = Time.time;
                }
            }

            // Check if path is valid and handle issues
            if (!agent.pathPending)
            {
                if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
                {
                    // Path is invalid, try to find alternative
                    if (Time.frameCount % 60 == 0) // Only log occasionally
                    {
                        Debug.LogWarning("Path to player is invalid! Trying to find alternative route.");
                    }

                    // Try finding a point closer to player that IS reachable
                    Vector3 directionToPlayer = (player.position - transform.position).normalized;
                    for (float distance = 2f; distance <= detectionRange; distance += 2f)
                    {
                        Vector3 testPoint = transform.position + directionToPlayer * distance;
                        UnityEngine.AI.NavMeshHit testHit;
                        if (UnityEngine.AI.NavMesh.SamplePosition(testPoint, out testHit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            if (agent.SetDestination(testHit.position))
                            {
                                lastDestination = testHit.position;
                                break;
                            }
                        }
                    }
                }
            }

            // Smooth rotation towards movement direction
            if (agent.velocity.magnitude > 0.1f)
            {
                Vector3 moveDirection = agent.velocity.normalized;
                moveDirection.y = 0; // Keep rotation on horizontal plane
                if (moveDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }
        else if (lastKnownPlayerPosition != Vector3.zero)
        {
                // Move to last known position (player was detected but line of sight blocked or lost)
                Debug.Log($"Chase: Moving to last known player position: {lastKnownPlayerPosition}");
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(lastKnownPlayerPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                    if (Vector3.Distance(hit.position, lastDestination) > 0.5f || !agent.hasPath)
                {
                    agent.SetDestination(hit.position);
                    lastDestination = hit.position;
                        lastPathUpdateTime = Time.time;
                }
            }
            else
                {
                    if (Vector3.Distance(lastKnownPlayerPosition, lastDestination) > 0.5f || !agent.hasPath)
            {
                agent.SetDestination(lastKnownPlayerPosition);
                lastDestination = lastKnownPlayerPosition;
                        lastPathUpdateTime = Time.time;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Chase state but no player reference! Falling back to patrol.");
            // Fall back to patrolling
            currentState = AIState.Patrolling;
        }
    }

    void Attack()
    {
        agent.isStopped = true;

        if (player != null)
        {
            // Look at player
            Vector3 lookDirection = (player.position - transform.position).normalized;
            lookDirection.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);

            // Check if still in attack range - use configurable multiplier
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange * attackDisengageMultiplier)
            {
                currentState = AIState.Chasing;
            }
        }

        // Trigger attack animation
        if (!isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger(animAttackParam);
        }

        // Wait for attack animation (adjust time based on your animation length)
        yield return new WaitForSeconds(1f);

        // Trigger game over when player is caught
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerCaught();
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null! Player was caught but game over cannot be triggered.");
        }

        isAttacking = false;
        currentState = AIState.Chasing;
    }

    void Search()
    {
        agent.speed = patrolSpeed;
        agent.isStopped = false;

        searchTimer += Time.deltaTime;

        // Move to last known position
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 1f)
        {
            agent.SetDestination(lastKnownPlayerPosition);
        }
        else
        {
            // Look around
            transform.Rotate(0, 90f * Time.deltaTime, 0);
        }

        // After search duration, return to patrolling
        if (searchTimer >= searchDuration)
        {
            currentState = AIState.Patrolling;
            if (patrolPoints.Length > 0)
            {
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }

    void InvestigateSound()
    {
        agent.speed = chaseSpeed; // Move quickly toward sound
        agent.isStopped = false;
        isWaitingAtPatrolPoint = false; // Reset patrol wait flag

        // Update sound source position if it's moving (e.g., player footsteps)
        // For radio sounds, update position from the radio object directly
        if (currentSoundType == SoundType.Radio && investigatingRadio != null)
        {
            soundSourcePosition = investigatingRadio.transform.position;
        }
        
        if (isInvestigatingSound && soundSourcePosition != Vector3.zero)
        {
            float distanceToSound = Vector3.Distance(transform.position, soundSourcePosition);
            
            // Update path more frequently for smoother following (similar to Chase method)
            float timeSinceLastUpdate = Time.time - lastPathUpdateTime;
            bool shouldUpdatePath = timeSinceLastUpdate >= pathUpdateInterval;

            // Also update if sound source moved significantly
            float distanceToLastDestination = Vector3.Distance(soundSourcePosition, lastDestination);
            bool soundMovedSignificantly = distanceToLastDestination > 1f;

            // Update path if needed (similar to Chase method)
            if (shouldUpdatePath || soundMovedSignificantly || !agent.hasPath)
            {
                // Check if sound source position is on NavMesh
                UnityEngine.AI.NavMeshHit hit;
                bool soundOnNavMesh = UnityEngine.AI.NavMesh.SamplePosition(soundSourcePosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas);

                Vector3 targetPosition;

                // For radio sounds, go directly to the audio source
                // For footsteps, maintain safe distance to prevent pushing
                if (currentSoundType == SoundType.Radio)
                {
                    // Radio: go directly to the audio source position
                    if (soundOnNavMesh)
                    {
                        targetPosition = hit.position; // Go directly to radio position
                    }
                    else
                    {
                        // Radio not on NavMesh, find nearest NavMesh point
                        UnityEngine.AI.NavMeshHit nearestHit;
                        if (UnityEngine.AI.NavMesh.SamplePosition(soundSourcePosition, out nearestHit, 10f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            targetPosition = nearestHit.position;
                        }
                        else
                        {
                            // Can't find NavMesh, use radio position directly
                            targetPosition = soundSourcePosition;
                        }
                    }
                }
                else
                {
                    // Footsteps: maintain safe distance from sound source to prevent pushing
                    // Use public safeDistance variable
                    Vector3 directionToSound = (soundSourcePosition - transform.position).normalized;
                    
                    if (soundOnNavMesh)
                    {
                        // Sound source is on NavMesh, set destination to safe distance from sound
                        // Calculate position that maintains safe distance
                        Vector3 safeTargetPosition = soundSourcePosition - directionToSound * safeDistance;
                        
                        // Re-sample to ensure safe position is on NavMesh
                        UnityEngine.AI.NavMeshHit safeHit;
                        if (UnityEngine.AI.NavMesh.SamplePosition(safeTargetPosition, out safeHit, 3f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            targetPosition = safeHit.position;
                        }
                        else
                        {
                            // Fallback to hit position if safe position not on NavMesh
                            targetPosition = hit.position;
                        }
                    }
                    else
                    {
                        // Sound source not on NavMesh, find nearest NavMesh point but maintain safe distance
                        UnityEngine.AI.NavMeshHit nearestHit;
                        if (UnityEngine.AI.NavMesh.SamplePosition(soundSourcePosition, out nearestHit, 10f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            // Calculate safe position from nearest NavMesh point
                            Vector3 safeTargetPosition = nearestHit.position - directionToSound * safeDistance;
                            UnityEngine.AI.NavMeshHit safeHit;
                            if (UnityEngine.AI.NavMesh.SamplePosition(safeTargetPosition, out safeHit, 3f, UnityEngine.AI.NavMesh.AllAreas))
                            {
                                targetPosition = safeHit.position;
                            }
                            else
                            {
                                targetPosition = nearestHit.position;
                            }
                        }
                        else
                        {
                            // Can't find NavMesh near sound source, try to find alternative point
                            targetPosition = soundSourcePosition - directionToSound * safeDistance; // Use safe distance
                            
                            // Try finding a point closer that IS reachable
                            for (float distance = safeDistance; distance <= Mathf.Min(distanceToSound, footstepHearingRange); distance += 2f)
                            {
                                Vector3 testPoint = transform.position + directionToSound * (distance - safeDistance);
                                UnityEngine.AI.NavMeshHit testHit;
                                if (UnityEngine.AI.NavMesh.SamplePosition(testPoint, out testHit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                                {
                                    targetPosition = testHit.position;
                                    break;
                                }
                            }
                        }
                    }

                    // Ensure target maintains safe distance from sound source (only for footsteps)
                    float finalDistanceToSound = Vector3.Distance(targetPosition, soundSourcePosition);
                    if (finalDistanceToSound < safeDistance && distanceToSound > safeDistance)
                    {
                        // Adjust target to maintain safe distance
                        targetPosition = soundSourcePosition - directionToSound * safeDistance;
                        UnityEngine.AI.NavMeshHit finalHit;
                        if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out finalHit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            targetPosition = finalHit.position;
                        }
                    }
                }

                // Only update if destination changed significantly
                if (Vector3.Distance(targetPosition, lastDestination) > 0.5f || !agent.hasPath)
                {
                    agent.SetDestination(targetPosition);
                    lastDestination = targetPosition;
                    lastPathUpdateTime = Time.time;
                }
            }

            // Check if path is valid and handle issues
            if (!agent.pathPending)
            {
                if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
                {
                    // Path is invalid, try to find alternative route
                    Vector3 directionToSound = (soundSourcePosition - transform.position).normalized;
                    float maxSearchDistance = Mathf.Min(distanceToSound, Mathf.Max(footstepHearingRange, radioHearingRange));
                    
                    // Try finding a point closer to sound that IS reachable
                    for (float distance = 2f; distance <= maxSearchDistance; distance += 2f)
                    {
                        Vector3 testPoint = transform.position + directionToSound * distance;
                        UnityEngine.AI.NavMeshHit testHit;
                        if (UnityEngine.AI.NavMesh.SamplePosition(testPoint, out testHit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            if (agent.SetDestination(testHit.position))
                            {
                                lastDestination = testHit.position;
                                lastPathUpdateTime = Time.time;
                                break;
                            }
                        }
                    }
                }
            }

            // Smooth rotation towards movement direction (same as Chase method)
            if (agent.velocity.magnitude > 0.1f)
            {
                Vector3 moveDirection = agent.velocity.normalized;
                moveDirection.y = 0; // Keep rotation on horizontal plane
                if (moveDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }

            // Check if we've reached the sound source
            // For radio: go directly to audio source (very close distance - 20% of safeDistance)
            // For footsteps: maintain safe distance
            float soundInvestigationDistance = currentSoundType == SoundType.Radio ? safeDistance * 0.2f : safeDistance;
            if (distanceToSound < soundInvestigationDistance)
            {
                // Stop agent when reached sound location
                agent.isStopped = true;
                
                // Arrived at sound location, look around smoothly
                Vector3 lookDirection = (soundSourcePosition - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
                }
                
                // Timer handling is now done in CheckForSounds() based on sound type
                // Footsteps: will timeout after footstepInvestigationDuration if player stops running
                // Radio: will continue investigating while radio is playing
            }
            else
            {
                // Still moving toward sound
                agent.isStopped = false; // Ensure agent is moving
                
                // For footsteps only: stop before getting too close to prevent pushing
                if (currentSoundType == SoundType.Footstep && distanceToSound < soundInvestigationDistance + 0.5f)
                {
                    agent.isStopped = true;
                }
            }
        }
        else
        {
            // No valid sound position, return to patrol
            Debug.Log("KidnapperAI: No valid sound position. Returning to patrol.");
            isInvestigatingSound = false;
            currentState = AIState.Patrolling;
            soundInvestigationTimer = 0f;
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat(animSpeedParam, speed);

        // Set idle state
        bool isIdle = agent.isStopped || speed < 0.1f;
        animator.SetBool(animIdleParam, isIdle);

        // Debug animation state occasionally
        if (Time.frameCount % 60 == 0) // Every 60 frames
        {
            Debug.Log($"Animation - Speed: {speed:F2}, IsIdle: {isIdle}, State: {currentState}, Agent Stopped: {agent.isStopped}, Has Path: {agent.hasPath}");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw field of view
        Gizmos.color = Color.green;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftBoundary * detectionRange);
        Gizmos.DrawRay(transform.position, rightBoundary * detectionRange);

        // Draw patrol path
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    else if (patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }

        // Draw sound detection ranges
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, footstepHearingRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radioHearingRange);

        // Draw current sound investigation target
        if (currentState == AIState.InvestigatingSound && soundSourcePosition != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(soundSourcePosition, 1f);
            Gizmos.DrawLine(transform.position, soundSourcePosition);
        }
    }

    // Save/Load methods for SaveSystem
    public int GetCurrentPatrolIndex()
    {
        return currentPatrolIndex;
    }

    public Vector3 GetLastKnownPlayerPosition()
    {
        return lastKnownPlayerPosition;
    }

    public bool IsPlayerDetected()
    {
        return playerDetected;
    }

    public void SetCurrentPatrolIndex(int index)
    {
        if (index >= 0 && patrolPoints != null && index < patrolPoints.Length)
        {
            currentPatrolIndex = index;
            if (agent != null && patrolPoints[index] != null)
            {
                agent.SetDestination(patrolPoints[index].position);
            }
        }
    }

    public void SetLastKnownPlayerPosition(Vector3 position)
    {
        lastKnownPlayerPosition = position;
    }

    public void SetPlayerDetected(bool detected)
    {
        playerDetected = detected;
    }
}

