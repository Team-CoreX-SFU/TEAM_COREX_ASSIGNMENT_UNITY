using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// VR Keypad Button - Handles trigger-based interaction for keypad buttons
/// Attach this to each keypad button GameObject with a Collider set as Trigger
/// Button will be pressed when VR hand/controller enters the trigger collider
/// </summary>
[RequireComponent(typeof(Collider))]
public class VRKeypadButton : MonoBehaviour
{
    [Header("Button Settings")]
    [Tooltip("The number this button represents (0-9)")]
    public int buttonNumber = 0;

    [Header("Keypad Reference")]
    [Tooltip("Reference to the KeyPadScript that manages the keypad")]
    public KeyPadScript keypadScript;

    [Header("VR Hand Detection")]
    [Tooltip("Tags to identify VR hands/controllers (comma-separated, e.g., 'Hand,Controller')")]
    public string[] handTags = new string[] { "Hand", "Controller" };

    [Tooltip("Layer mask for VR hand detection (leave as Everything to detect all)")]
    public LayerMask handLayerMask = -1;

    [Header("Settings")]
    [Tooltip("Cooldown time between button presses (prevents rapid triggering)")]
    public float pressCooldown = 0.3f;

    [Tooltip("Require collider to be set as trigger")]
    public bool requireTrigger = true;

    private Number numberComponent;
    private Collider buttonCollider;
    private float lastPressTime = 0f;
    private bool handInside = false;

    void Start()
    {
        // Get Collider component
        buttonCollider = GetComponent<Collider>();
        if (buttonCollider == null)
        {
            Debug.LogError($"[VRKeypadButton] No Collider found on {gameObject.name}! Please add a Collider component.");
            enabled = false;
            return;
        }

        // Ensure collider is set as trigger
        if (requireTrigger && !buttonCollider.isTrigger)
        {
            Debug.LogWarning($"[VRKeypadButton] Collider on {gameObject.name} is not set as Trigger. Setting it now.");
            buttonCollider.isTrigger = true;
        }

        // Get Number component for button number (check self and children)
        numberComponent = GetComponent<Number>();
        if (numberComponent == null)
        {
            numberComponent = GetComponentInChildren<Number>();
        }
        if (numberComponent != null)
        {
            buttonNumber = numberComponent.number;
        }

        // Auto-find keypad script if not assigned
        if (keypadScript == null)
        {
            keypadScript = FindObjectOfType<KeyPadScript>();
            if (keypadScript == null)
            {
                Debug.LogWarning($"[VRKeypadButton] KeyPadScript not found! Button {buttonNumber} will not function.");
            }
        }
    }

    /// <summary>
    /// Called when a collider enters the trigger zone
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (IsVRHand(other.gameObject))
        {
            handInside = true;
            PressButton();
        }
    }

    /// <summary>
    /// Called when a collider exits the trigger zone
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (IsVRHand(other.gameObject))
        {
            handInside = false;
        }
    }

    /// <summary>
    /// Checks if the GameObject is a VR hand or controller
    /// </summary>
    private bool IsVRHand(GameObject obj)
    {
        if (obj == null) return false;

        // Check layer mask
        if (handLayerMask != -1 && ((1 << obj.layer) & handLayerMask) == 0)
        {
            return false;
        }

        // Check by tag
        foreach (string tag in handTags)
        {
            if (!string.IsNullOrEmpty(tag) && obj.CompareTag(tag))
            {
                return true;
            }
        }

        // Check parent hierarchy for tags
        Transform parent = obj.transform.parent;
        int depth = 0;
        while (parent != null && depth < 5) // Limit depth to avoid infinite loops
        {
            foreach (string tag in handTags)
            {
                if (!string.IsNullOrEmpty(tag) && parent.CompareTag(tag))
                {
                    return true;
                }
            }
            parent = parent.parent;
            depth++;
        }

        // Check for XR interaction components (common in VR hands/controllers)
        if (obj.GetComponent<XRDirectInteractor>() != null ||
            obj.GetComponent<XRRayInteractor>() != null ||
            obj.GetComponent<XRController>() != null ||
            obj.GetComponentInParent<XRDirectInteractor>() != null ||
            obj.GetComponentInParent<XRRayInteractor>() != null ||
            obj.GetComponentInParent<XRController>() != null)
        {
            return true;
        }

        // Check for common VR hand/controller names
        string objName = obj.name.ToLower();
        if (objName.Contains("hand") || objName.Contains("controller") ||
            objName.Contains("interactor") || objName.Contains("ray"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Presses the button (called when VR hand enters trigger)
    /// </summary>
    private void PressButton()
    {
        // Check cooldown
        if (Time.time - lastPressTime < pressCooldown)
        {
            return;
        }

        lastPressTime = Time.time;

        if (keypadScript != null)
        {
            keypadScript.OnButtonPressed(buttonNumber);
            Debug.Log($"[VRKeypadButton] Button {buttonNumber} pressed via trigger.");
        }
        else
        {
            Debug.LogWarning($"[VRKeypadButton] KeypadScript not found! Button {buttonNumber} pressed but cannot register.");
        }
    }
}

