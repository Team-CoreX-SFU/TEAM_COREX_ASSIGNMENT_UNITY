using System.Collections;
using UnityEngine;

/// <summary>
/// Toggles a Light component on/off with per-instance random on-time.
/// - Light stays ON for a random duration between <see cref="minOnTime"/> and <see cref="maxOnTime"/>.
/// - Then Light turns OFF for a fixed duration (<see cref="offTime"/>, default 3 seconds).
/// - Repeats forever. Each instance gets its own random timings.
/// Attach this to any GameObject that has a Light component (on itself or a child).
/// </summary>
public class GlichedLight : MonoBehaviour
{
    [Header("On-time (random per cycle)")]
    [Tooltip("Minimum time the light stays ON before turning OFF.")]
    [Min(0f)]
    public float minOnTime = 5f;

    [Tooltip("Maximum time the light stays ON before turning OFF.")]
    [Min(0f)]
    public float maxOnTime = 10f;

    [Header("Off-time (fixed)")]
    [Tooltip("Time the light stays OFF before turning ON again.")]
    [Min(0f)]
    public float offTime = 3f;

    [Header("Startup")]
    [Tooltip("Random initial delay before starting the cycle (helps desync multiple lights). Set 0 to start immediately.")]
    [Min(0f)]
    public float maxInitialJitter = 0.5f;

    private Light _light;
    private Coroutine _routine;

    private void Awake()
    {
        // Prefer Light on the same GameObject; fall back to children if needed.
        _light = GetComponent<Light>();
        if (_light == null)
            _light = GetComponentInChildren<Light>(includeInactive: true);

        if (minOnTime > maxOnTime)
        {
            // Keep values sane if set incorrectly in Inspector.
            var tmp = minOnTime;
            minOnTime = maxOnTime;
            maxOnTime = tmp;
        }
    }

    private void OnEnable()
    {
        if (_routine == null)
            _routine = StartCoroutine(LightRoutine());
    }

    private void OnDisable()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }

    private IEnumerator LightRoutine()
    {
        if (_light == null)
            yield break; // No Light found.

        // Optional small random start offset to avoid all lights syncing.
        if (maxInitialJitter > 0f)
            yield return new WaitForSeconds(Random.Range(0f, maxInitialJitter));

        while (true)
        {
            // Ensure light is ON, then wait a random ON duration.
            SetLightEnabled(true);
            float onDuration = (minOnTime <= maxOnTime)
                ? Random.Range(minOnTime, maxOnTime)
                : minOnTime; // fallback
            if (onDuration > 0f)
                yield return new WaitForSeconds(onDuration);
            else
                yield return null; // next frame

            // Turn OFF for fixed time (default 3s).
            SetLightEnabled(false);
            if (offTime > 0f)
                yield return new WaitForSeconds(offTime);
            else
                yield return null; // next frame
        }
    }

    private void SetLightEnabled(bool enabled)
    {
        // If someone removed/disabled the Light at runtime, try to re-find it once.
        if (_light == null)
        {
            _light = GetComponent<Light>();
            if (_light == null)
                _light = GetComponentInChildren<Light>(includeInactive: true);
            if (_light == null) return;
        }

        _light.enabled = enabled;
    }
}
