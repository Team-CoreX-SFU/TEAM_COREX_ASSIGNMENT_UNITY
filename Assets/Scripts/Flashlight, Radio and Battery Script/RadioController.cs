using System.Collections;
using UnityEngine;

/// <summary>
/// Handles powering and toggling the radio audio, mirroring the flashlight behaviour.
/// </summary>
public class RadioController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource radioAudioSource;
    [Range(0f, 1f)] public float targetVolume = 0.85f;
    public bool loopRadioClip = true;

    [Header("Distance-Based Volume")]
    public bool useDistanceFalloff = true;
    public float minDistance = 1f;
    public float maxDistance = 10f;
    public float maxAudibleDistance = 20f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public bool stopPlaybackWhenTooFar = false;
    [Tooltip("Optional override (player camera). If empty, auto-find XR Origin / Main Camera.")]
    public Transform listenerTarget;

    [Header("Power State")]
    public bool hasBattery = false;
    public bool isOn = false;
    public bool playerInRange = false;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.35f;
    [Tooltip("Delay in seconds before playing sound after toggling on.")]
    public float playDelay = 5f;

    [Header("Visual Feedback")]
    [Tooltip("ActiveLight GameObject that shows emission when radio is on. Should be a child object with a material that has emission enabled.")]
    public GameObject activeLight;
    [Tooltip("Emission color when radio is on.")]
    public Color emissionColor = Color.green;
    [Tooltip("Emission intensity when radio is on.")]
    [Range(0f, 5f)]
    public float emissionIntensity = 1f;

    private Coroutine fadeRoutine;
    private float fadeVolume = 0f;
    private float distanceMultiplier = 1f;
    private Material activeLightMaterial;
    private bool emissionWasEnabled = false;

    void Awake()
    {
        if (radioAudioSource == null)
        {
            radioAudioSource = GetComponent<AudioSource>();
        }

        if (radioAudioSource != null)
        {
            radioAudioSource.loop = loopRadioClip;
            radioAudioSource.playOnAwake = false;
            fadeVolume = 0f;
            Apply3DSettings();
            ApplyVolume();
        }

        if (listenerTarget == null)
        {
            FindListener();
        }

        // Setup ActiveLight
        SetupActiveLight();
    }

    void SetupActiveLight()
    {
        if (activeLight == null)
        {
            // Try to find ActiveLight as a child
            Transform activeLightTransform = transform.Find("ActiveLight");
            if (activeLightTransform != null)
            {
                activeLight = activeLightTransform.gameObject;
            }
        }

        if (activeLight != null)
        {
            Renderer renderer = activeLight.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                // Create instance material to avoid modifying shared material
                activeLightMaterial = renderer.material;
                
                // Check if emission is already enabled
                if (activeLightMaterial.IsKeywordEnabled("_EMISSION"))
                {
                    emissionWasEnabled = true;
                }
                
                // Ensure emission is initially off
                SetEmission(false);
            }
            else
            {
                Debug.LogWarning($"RadioController: ActiveLight '{activeLight.name}' has no Renderer or Material!");
            }
        }
    }

    void SetEmission(bool enabled)
    {
        if (activeLightMaterial == null) return;

        if (enabled)
        {
            // Enable emission
            activeLightMaterial.EnableKeyword("_EMISSION");
            activeLightMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            
            // Make sure the material is set to use emission
            activeLightMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            // Disable emission
            activeLightMaterial.DisableKeyword("_EMISSION");
            activeLightMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    void Update()
    {
        if (useDistanceFalloff)
        {
            UpdateDistanceMultiplier();
            ApplyVolume();
        }
    }

    public void ToggleRadio()
    {
        if (!hasBattery || !playerInRange || radioAudioSource == null)
        {
            return;
        }

        isOn = !isOn;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        // If turning off, immediately disable emission
        if (!isOn)
        {
            SetEmission(false);
        }

        fadeRoutine = StartCoroutine(isOn ? FadeIn() : FadeOut());
    }

    public void InsertBattery()
    {
        hasBattery = true;
    }

    /// <summary>
    /// Returns true if the radio sound is actually playing (after delay).
    /// Use this instead of isOn to check if sound is audible.
    /// </summary>
    public bool IsSoundPlaying()
    {
        return radioAudioSource != null && radioAudioSource.isPlaying && fadeVolume > 0f;
    }

    public void SetPlayerInRange(bool value, bool autoPowerOff = true)
    {
        playerInRange = value;

        if (!playerInRange && autoPowerOff && isOn)
        {
            PowerOff();
        }
    }

    /// <summary>
    /// Forces the radio to turn off (used when player leaves trigger or for scripted events).
    /// </summary>
    public void PowerOff(bool fade = true)
    {
        if (radioAudioSource == null)
        {
            return;
        }

        isOn = false;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (fade && fadeOutDuration > 0f)
        {
            fadeRoutine = StartCoroutine(FadeOut());
        }
        else
        {
            fadeVolume = 0f;
            ApplyVolume();
            radioAudioSource.Stop();
            SetEmission(false);
        }
    }

    IEnumerator FadeIn()
    {
        // Enable ActiveLight emission immediately when toggled on
        SetEmission(true);

        // Wait for the delay before playing sound
        if (playDelay > 0f)
        {
            yield return new WaitForSeconds(playDelay);
        }

        // Check if radio is still on (might have been toggled off during delay)
        if (!isOn)
        {
            SetEmission(false);
            yield break;
        }

        // Start playing audio
        if (!radioAudioSource.isPlaying)
        {
            radioAudioSource.Play();
        }

        // Fade in volume
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            fadeVolume = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            ApplyVolume();
            yield return null;
        }

        fadeVolume = targetVolume;
        ApplyVolume();
    }

    IEnumerator FadeOut()
    {
        if (radioAudioSource == null)
        {
            SetEmission(false);
            yield break;
        }

        float startVolume = fadeVolume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            fadeVolume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
            ApplyVolume();
            yield return null;
        }

        fadeVolume = 0f;
        ApplyVolume();
        radioAudioSource.Stop();
        
        // Disable ActiveLight emission after fade out
        SetEmission(false);
    }

    void UpdateDistanceMultiplier()
    {
        if (!useDistanceFalloff)
        {
            distanceMultiplier = 1f;
            return;
        }

        if (listenerTarget == null)
        {
            FindListener();
        }

        if (listenerTarget == null)
        {
            distanceMultiplier = 1f;
            return;
        }

        float distance = Vector3.Distance(listenerTarget.position, transform.position);

        if (maxAudibleDistance > 0f && distance > maxAudibleDistance)
        {
            distanceMultiplier = 0f;
            if (stopPlaybackWhenTooFar && radioAudioSource.isPlaying)
            {
                PowerOff();
            }
            return;
        }

        float range = Mathf.Max(0.01f, maxDistance - minDistance);
        float t = Mathf.Clamp01((distance - minDistance) / range);
        distanceMultiplier = 1f - t;
    }

    void ApplyVolume()
    {
        if (radioAudioSource == null)
        {
            return;
        }

        radioAudioSource.volume = Mathf.Clamp01(fadeVolume) * Mathf.Clamp01(distanceMultiplier);
    }

    void Apply3DSettings()
    {
        if (radioAudioSource == null)
        {
            return;
        }

        radioAudioSource.spatialBlend = spatialBlend;
        radioAudioSource.rolloffMode = rolloffMode;
        radioAudioSource.minDistance = Mathf.Max(0.01f, minDistance);
        radioAudioSource.maxDistance = Mathf.Max(radioAudioSource.minDistance + 0.01f, maxDistance);
    }

    void FindListener()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            return;
        }

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

#if UNITY_EDITOR
    void OnValidate()
    {
        Apply3DSettings();
    }
#endif
}

