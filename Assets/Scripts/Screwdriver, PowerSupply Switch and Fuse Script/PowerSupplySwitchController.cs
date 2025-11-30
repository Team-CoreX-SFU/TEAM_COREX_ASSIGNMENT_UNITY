using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the Power Supply Switch animation.
/// Switches off by rotating Z axis from -90 to 0 degrees.
/// </summary>
public class PowerSupplySwitchController : MonoBehaviour
{
    [Header("Switch Settings")]
    [Tooltip("The Transform of the switch object to rotate (if null, uses this GameObject's transform)")]
    public Transform switchTransform;
    
    [Header("Animation Settings")]
    [Tooltip("Starting Z rotation angle (ON position)")]
    public float onAngle = -90f;
    
    [Tooltip("Ending Z rotation angle (OFF position)")]
    public float offAngle = 0f;
    
    [Tooltip("Duration of the switch animation in seconds")]
    public float animationDuration = 0.8f;
    
    [Tooltip("Animation curve for smooth switching (optional)")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Interaction Settings")]
    [Tooltip("Can be toggled via XR interaction or trigger zone")]
    public bool canInteract = true;
    
    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip switchOffSound;
    public AudioClip switchOnSound;
    
    [Header("Light Control")]
    [Tooltip("Lights to turn off when switch is turned off")]
    public Light[] lightsToTurnOff;
    
    [Tooltip("GameObjects with Light components to turn off (searches for Light component)")]
    public GameObject[] lightObjectsToTurnOff;
    
    [Tooltip("Search for Light components in children of light objects")]
    public bool searchChildrenForLights = true;
    
    private bool isOn = true; // Switch starts in ON position
    private bool isAnimating = false;
    private Coroutine animationCoroutine;
    
    void Awake()
    {
        // If no switch transform assigned, use this GameObject's transform
        if (switchTransform == null)
        {
            switchTransform = transform;
        }
        
        // Set initial rotation to ON position
        Vector3 currentRotation = switchTransform.localEulerAngles;
        switchTransform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, onAngle);
    }
    
    /// <summary>
    /// Toggles the switch state.
    /// </summary>
    public void ToggleSwitch()
    {
        if (!canInteract || isAnimating) return;
        
        if (isOn)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }
    }
    
    /// <summary>
    /// Turns the switch OFF (Z rotation from -90 to 0).
    /// </summary>
    public void TurnOff()
    {
        if (!isOn || isAnimating) return;
        
        StartCoroutine(AnimateSwitch(offAngle));
        
        // Play sound if available
        if (audioSource != null && switchOffSound != null)
        {
            audioSource.PlayOneShot(switchOffSound);
        }
        
        // Turn off lights
        TurnOffLights();
    }
    
    /// <summary>
    /// Turns the switch ON (Z rotation from 0 to -90).
    /// </summary>
    public void TurnOn()
    {
        if (isOn || isAnimating) return;
        
        StartCoroutine(AnimateSwitch(onAngle));
        
        // Play sound if available
        if (audioSource != null && switchOnSound != null)
        {
            audioSource.PlayOneShot(switchOnSound);
        }
        
        // Turn on lights (if switch can be turned back on)
        TurnOnLights();
    }
    
    private IEnumerator AnimateSwitch(float targetAngle)
    {
        isAnimating = true;
        
        Vector3 startRotation = switchTransform.localEulerAngles;
        float startAngle = startRotation.z;
        float elapsed = 0f;
        
        // Normalize angles to -180 to 180 range for proper interpolation
        if (startAngle > 180f) startAngle -= 360f;
        if (targetAngle > 180f) targetAngle -= 360f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Apply animation curve if available
            if (animationCurve != null && animationCurve.length > 0)
            {
                t = animationCurve.Evaluate(t);
            }
            
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);
            switchTransform.localEulerAngles = new Vector3(startRotation.x, startRotation.y, currentAngle);
            
            yield return null;
        }
        
        // Ensure final rotation is exact
        switchTransform.localEulerAngles = new Vector3(startRotation.x, startRotation.y, targetAngle);
        
        isOn = (targetAngle == onAngle);
        isAnimating = false;
    }
    
    /// <summary>
    /// Check if switch is currently ON.
    /// </summary>
    public bool IsOn()
    {
        return isOn;
    }
    
    /// <summary>
    /// Check if switch is currently OFF.
    /// </summary>
    public bool IsOff()
    {
        return !isOn;
    }
    
    /// <summary>
    /// Turns off all assigned lights.
    /// </summary>
    private void TurnOffLights()
    {
        // Turn off lights from direct Light array
        if (lightsToTurnOff != null)
        {
            foreach (Light light in lightsToTurnOff)
            {
                if (light != null)
                {
                    light.enabled = false;
                }
            }
        }
        
        // Turn off lights from GameObject array
        if (lightObjectsToTurnOff != null)
        {
            foreach (GameObject lightObj in lightObjectsToTurnOff)
            {
                if (lightObj == null) continue;
                
                // Try to get Light component on the GameObject itself
                Light light = lightObj.GetComponent<Light>();
                if (light != null)
                {
                    light.enabled = false;
                }
                
                // If searchChildrenForLights is enabled, search in children
                if (searchChildrenForLights)
                {
                    Light[] childLights = lightObj.GetComponentsInChildren<Light>(includeInactive: true);
                    foreach (Light childLight in childLights)
                    {
                        if (childLight != null)
                        {
                            childLight.enabled = false;
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Turns on all assigned lights (useful if switch can be turned back on).
    /// </summary>
    public void TurnOnLights()
    {
        // Turn on lights from direct Light array
        if (lightsToTurnOff != null)
        {
            foreach (Light light in lightsToTurnOff)
            {
                if (light != null)
                {
                    light.enabled = true;
                }
            }
        }
        
        // Turn on lights from GameObject array
        if (lightObjectsToTurnOff != null)
        {
            foreach (GameObject lightObj in lightObjectsToTurnOff)
            {
                if (lightObj == null) continue;
                
                // Try to get Light component on the GameObject itself
                Light light = lightObj.GetComponent<Light>();
                if (light != null)
                {
                    light.enabled = true;
                }
                
                // If searchChildrenForLights is enabled, search in children
                if (searchChildrenForLights)
                {
                    Light[] childLights = lightObj.GetComponentsInChildren<Light>(includeInactive: true);
                    foreach (Light childLight in childLights)
                    {
                        if (childLight != null)
                        {
                            childLight.enabled = true;
                        }
                    }
                }
            }
        }
    }
}

