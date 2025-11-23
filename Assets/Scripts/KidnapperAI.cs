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
    public float fieldOfView = 120f; // Increased default FOV to 120 degrees (60 on each side)
    public float patrolWaitTime = 2f;
    [Tooltip("If true, can detect player from any angle (360 degrees). If false, uses Field of View.")]
    public bool detectFromAllAngles = false;

    [Header("Patrol Points")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Player Detection")]
    public LayerMask playerLayer = 1; // Default layer
    public LayerMask obstacleLayer = 1; // For line of sight checks

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
        Searching
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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        // Find player by tag
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
        
        // Debug log to verify setup
        Debug.Log($"KidnapperAI initialized. Patrol points: {patrolPoints.Length}, Player found: {player != null}, Agent enabled: {agent.enabled}, Agent on NavMesh: {agent.isOnNavMesh}");
    }

    void Update()
    {
        // Check for player detection
        CheckForPlayer();

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
        }

        // Update animations
        UpdateAnimations();
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

        // Check if player is in detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Check if player is in field of view
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            // Debug FOV check
            if (Time.frameCount % 30 == 0) // Every 30 frames
            {
                Debug.Log($"Player check - Distance: {distanceToPlayer:F2}, Angle: {angleToPlayer:F2}, FOV limit: {fieldOfView / 2f:F2}, In range: {distanceToPlayer <= detectionRange}, In FOV: {angleToPlayer <= fieldOfView / 2f}");
            }

            // Check FOV - allow detection from all angles if enabled, or if very close
            bool inFOV = detectFromAllAngles || angleToPlayer <= fieldOfView / 2f;
            bool veryClose = distanceToPlayer <= attackRange * 2f; // Can detect from behind if very close
            
            if (inFOV || veryClose)
            {
                // If player is very close, detect them even through walls (for gameplay)
                bool canDetectThroughWalls = distanceToPlayer <= attackRange * 3f; // Within 6 units, detect through walls
                
                // Check line of sight - use a layer mask that includes everything
                RaycastHit hit;
                Vector3 rayStart = transform.position + Vector3.up * 1.5f; // Eye level
                Vector3 rayDirection = directionToPlayer;
                
                // Cast ray to check line of sight - use all layers
                int layerMask = ~0; // All layers
                bool hasLineOfSight = false;
                
                if (Physics.Raycast(rayStart, rayDirection, out hit, detectionRange, layerMask))
                {
                    // Check if we hit the player or something with Player tag
                    bool hitPlayer = hit.collider.CompareTag("Player") || 
                                    hit.collider.transform == player || 
                                    hit.collider.transform.IsChildOf(player) ||
                                    (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag("Player")) ||
                                    hit.collider.transform.root.CompareTag("Player");
                    
                    if (hitPlayer)
                    {
                        hasLineOfSight = true;
                    }
                    else
                    {
                        // Hit something else - check if we can detect through it
                        if (canDetectThroughWalls)
                        {
                            // If very close, detect anyway (player might be on other side of thin wall)
                            hasLineOfSight = true;
                            Debug.Log($"Player very close ({distanceToPlayer:F2}), detecting through obstacle: {hit.collider.name}");
                        }
                        else
                        {
                            // Debug what we hit instead
                            if (Time.frameCount % 60 == 0)
                            {
                                Debug.Log($"Line of sight blocked by: {hit.collider.name}, Tag: {hit.collider.tag}, Distance to obstacle: {hit.distance:F2}");
                            }
                        }
                    }
                }
                else
                {
                    // No hit at all - might be too far or something wrong
                    // If very close, assume we can see them
                    hasLineOfSight = canDetectThroughWalls;
                    if (Time.frameCount % 60 == 0 && !hasLineOfSight)
                    {
                        Debug.Log($"Raycast didn't hit anything. Distance: {distanceToPlayer}, Range: {detectionRange}");
                    }
                }
                
                // If we have line of sight (or can detect through walls), detect the player
                if (hasLineOfSight)
                {
                    playerDetected = true;
                    lastKnownPlayerPosition = player.position;

                    if (distanceToPlayer <= attackRange && currentState != AIState.Attacking)
                    {
                        Debug.Log("Player in attack range! Switching to attack.");
                        currentState = AIState.Attacking;
                    }
                    else if (currentState != AIState.Attacking && currentState != AIState.Chasing)
                    {
                        Debug.Log($"Player detected! Distance: {distanceToPlayer:F2}, Angle: {angleToPlayer:F2}, Switching to chase.");
                        currentState = AIState.Chasing;
                    }
                    return;
                }
            }
        }

        // If player was detected but now lost, start searching
        if (playerDetected && (currentState == AIState.Chasing || currentState == AIState.Attacking))
        {
            // Only switch to searching if player is out of range
            if (distanceToPlayer > detectionRange * 1.5f)
            {
                Debug.Log("Player lost! Switching to search.");
                playerDetected = false;
                currentState = AIState.Searching;
                searchTimer = 0f;
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

        if (player != null && playerDetected)
        {
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
                    // Player is on NavMesh, set destination directly
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
                        // Can't find NavMesh near player, try direct destination anyway
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
            // Move to last known position
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(lastKnownPlayerPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                if (Vector3.Distance(hit.position, lastDestination) > 0.5f)
                {
                    agent.SetDestination(hit.position);
                    lastDestination = hit.position;
                }
            }
            else
            {
                agent.SetDestination(lastKnownPlayerPosition);
                lastDestination = lastKnownPlayerPosition;
            }
        }
        else
        {
            Debug.LogWarning("Chase state but no player or last known position!");
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

            // Check if still in attack range
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange * 1.5f)
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

        // Here you can add logic to damage player or trigger game over
        // For example: GameManager.Instance.PlayerCaught();

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
    }
}

