using UnityEngine;

/// <summary>
/// Listens to external events (UI Button, XR interaction, trigger, etc.) to toggle a Light.
/// This component only turns the light ON or OFF; no glitch/random behavior is included.
/// 
/// How to use:
/// - Attach to the same GameObject as the Light, or assign <see cref="targetLight"/> explicitly.
/// - Wire events to call <see cref="Toggle"/>, <see cref="TurnOn"/>, <see cref="TurnOff"/>, or <see cref="Set(bool)"/>.
/// </summary>
public class ToggleLight : MonoBehaviour
{
    [Header("Target Light")]
    [Tooltip("If not set, the script will try to find a Light on this GameObject; optionally in children.")]
    public Light targetLight;

    [Tooltip("If true and no Light on this GameObject, also search the children (include inactive).")]
    public bool searchInChildren = true;

    private void Awake()
    {
        EnsureLightReference();
    }

    private void EnsureLightReference()
    {
        if (targetLight != null) return;
        targetLight = GetComponent<Light>();
        if (targetLight == null && searchInChildren)
        {
            targetLight = GetComponentInChildren<Light>(includeInactive: true);
        }
    }

    /// <summary>
    /// Flip current state of the Light.
    /// </summary>
    public void Toggle()
    {
        if (!EnsureUsable()) return;
        SetInternal(!targetLight.enabled);
    }

    /// <summary>
    /// Turn the Light ON.
    /// </summary>
    public void TurnOn()
    {
        if (!EnsureUsable()) return;
        SetInternal(true);
    }

    /// <summary>
    /// Turn the Light OFF.
    /// </summary>
    public void TurnOff()
    {
        if (!EnsureUsable()) return;
        SetInternal(false);
    }

    /// <summary>
    /// Explicitly set the Light to ON (true) or OFF (false).
    /// </summary>
    public void Set(bool on)
    {
        if (!EnsureUsable()) return;
        SetInternal(on);
    }

    private bool EnsureUsable()
    {
        if (targetLight == null)
        {
            EnsureLightReference();
        }
        return targetLight != null;
    }

    private void SetInternal(bool on)
    {
        targetLight.enabled = on;
    }
}
