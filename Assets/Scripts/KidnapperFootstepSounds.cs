using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Plays randomized footstep sounds for Kidnapper NPCs based on their NavMeshAgent velocity or Animator speed.
/// Attach this to the Kidnapper prefab (same GameObject that holds the NavMeshAgent/KidnapperAI).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class KidnapperFootstepSounds : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource footstepSource;
    public AudioClip[] walkingClips;
    public AudioClip[] runningClips;
    [Range(0f, 1f)] public float walkingVolume = 0.7f;
    [Range(0f, 1f)] public float runningVolume = 0.85f;
    [Tooltip("Random pitch variation to avoid repetitive sounds.")]
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("3D Audio Settings")]
    [Tooltip("Volume stays full until this distance.")]
    public float minDistance = 1f;
    [Tooltip("Volume becomes 0 beyond this distance.")]
    public float maxDistance = 8f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
    [Tooltip("If > 0, footsteps are skipped entirely when the player is farther than this distance.")]
    public float maxAudibleDistance = 15f;

    [Header("Timing")]
    public float walkingInterval = 0.55f;
    public float runningInterval = 0.35f;
    [Tooltip("Higher smoothing = slower reaction to sudden speed changes.")]
    [Range(0f, 0.95f)]
    public float speedSmoothing = 0.6f;

    [Header("Speed Thresholds (units/second)")]
    public float walkingSpeedThreshold = 0.2f;
    public float runningSpeedThreshold = 2.2f;

    [Header("Optional References")]
    public Animator animator;
    public string animatorSpeedParameter = "Speed";
    public KidnapperAI kidnapperAI;
    [Tooltip("Optional listener override (e.g., player's Main Camera). If null, script tries to find the Player tag.")]
    public Transform listenerTarget;

    private NavMeshAgent agent;
    private float smoothedSpeed = 0f;
    private float stepTimer = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (kidnapperAI == null)
        {
            kidnapperAI = GetComponent<KidnapperAI>();
        }
        if (animator == null && kidnapperAI != null)
        {
            animator = kidnapperAI.animator;
        }
        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.loop = false;
            footstepSource.spatialBlend = 1f; // 3D sound
        }

        Apply3DSettings();

        if (listenerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                // Try to grab main camera if available
                Transform cameraOffset = playerObj.transform.Find("Camera Offset");
                if (cameraOffset != null)
                {
                    Transform mainCamera = cameraOffset.Find("Main Camera");
                    listenerTarget = mainCamera != null ? mainCamera : cameraOffset;
                }
                else
                {
                    listenerTarget = playerObj.transform;
                }
            }
        }

        // Auto-configure thresholds from Kidnapper speeds if available
        if (kidnapperAI != null)
        {
            if (walkingSpeedThreshold <= 0f)
            {
                walkingSpeedThreshold = Mathf.Max(0.1f, kidnapperAI.patrolSpeed * 0.5f);
            }
            if (runningSpeedThreshold <= walkingSpeedThreshold)
            {
                runningSpeedThreshold = kidnapperAI.chaseSpeed * 0.8f;
            }
        }
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f || agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        float speed = agent.velocity.magnitude;

        // Animator speed often matches the blend tree movement, so include it if available
        if (animator != null && !string.IsNullOrEmpty(animatorSpeedParameter))
        {
            float animSpeed = animator.GetFloat(animatorSpeedParameter);
            speed = Mathf.Max(speed, animSpeed);
        }

        smoothedSpeed = Mathf.Lerp(smoothedSpeed, speed, 1f - speedSmoothing);

        bool isRunning = smoothedSpeed >= runningSpeedThreshold;
        bool isWalking = smoothedSpeed >= walkingSpeedThreshold && !isRunning;

        if (!isWalking && !isRunning)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += deltaTime;
        float interval = isRunning ? runningInterval : walkingInterval;

        if (stepTimer >= interval)
        {
            PlayFootstep(isRunning);
            stepTimer = 0f;
        }
    }

    private void PlayFootstep(bool isRunningStep)
    {
        if (footstepSource == null)
        {
            return;
        }

        if (maxAudibleDistance > 0f && listenerTarget != null)
        {
            float distanceToListener = Vector3.Distance(listenerTarget.position, transform.position);
            if (distanceToListener > maxAudibleDistance)
            {
                return; // Player too far away, skip playback
            }
        }

        AudioClip[] pool = isRunningStep && runningClips.Length > 0 ? runningClips : walkingClips;
        if (pool == null || pool.Length == 0)
        {
            return;
        }

        AudioClip clip = pool[Random.Range(0, pool.Length)];
        if (clip == null)
        {
            return;
        }

        footstepSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        footstepSource.volume = isRunningStep ? runningVolume : walkingVolume;
        footstepSource.PlayOneShot(clip);
    }

    private void Apply3DSettings()
    {
        if (footstepSource == null)
        {
            return;
        }

        footstepSource.spatialBlend = 1f;
        footstepSource.minDistance = Mathf.Max(0.1f, minDistance);
        footstepSource.maxDistance = Mathf.Max(footstepSource.minDistance + 0.1f, maxDistance);
        footstepSource.rolloffMode = rolloffMode;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        Apply3DSettings();
    }
#endif
}

