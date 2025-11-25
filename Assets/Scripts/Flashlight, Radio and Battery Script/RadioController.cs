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

    private Coroutine fadeRoutine;
    private float fadeVolume = 0f;
    private float distanceMultiplier = 1f;

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

        fadeRoutine = StartCoroutine(isOn ? FadeIn() : FadeOut());
    }

    public void InsertBattery()
    {
        hasBattery = true;
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
        }
    }

    IEnumerator FadeIn()
    {
        if (!radioAudioSource.isPlaying)
        {
            radioAudioSource.Play();
        }

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

