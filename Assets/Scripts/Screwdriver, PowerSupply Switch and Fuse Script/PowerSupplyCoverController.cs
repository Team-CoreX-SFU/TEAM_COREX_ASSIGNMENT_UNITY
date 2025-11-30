using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the Power Supply Cover opening animation.
/// Requires a screwdriver to be used for interaction.
/// Opens with Y rotation from -180 to -70 degrees.
/// </summary>
public class PowerSupplyCoverController : MonoBehaviour
{
    [Header("Cover Settings")]
    [Tooltip("The Transform of the cover object to rotate (if null, uses this GameObject's transform)")]
    public Transform coverTransform;
    
    [Header("Animation Settings")]
    [Tooltip("Starting Y rotation angle (closed position)")]
    public float closedAngle = -180f;
    
    [Tooltip("Ending Y rotation angle (open position)")]
    public float openAngle = -70f;
    
    [Tooltip("Duration of the opening animation in seconds")]
    public float animationDuration = 1.5f;
    
    [Tooltip("Animation curve for smooth opening (optional)")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Interaction Settings")]
    [Tooltip("Tag to identify the screwdriver object")]
    public string screwdriverTag = "Screwdriver";
    
    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip openSound;
    
    private bool isOpen = false;
    private bool isAnimating = false;
    private Coroutine animationCoroutine;
    
    void Awake()
    {
        // If no cover transform assigned, use this GameObject's transform
        if (coverTransform == null)
        {
            coverTransform = transform;
        }
        
        // Set initial rotation to closed position
        Vector3 currentRotation = coverTransform.localEulerAngles;
        coverTransform.localEulerAngles = new Vector3(currentRotation.x, closedAngle, currentRotation.z);
    }
    
    /// <summary>
    /// Called by ScrewdriverPickup when screwdriver is available.
    /// This method can be triggered via trigger zone interaction.
    /// </summary>
    public void HasScrewdriver()
    {
        // This method is kept for compatibility but actual interaction happens via trigger zone
    }
    
    /// <summary>
    /// Opens the cover if screwdriver is present and cover is closed.
    /// </summary>
    public void OpenCover()
    {
        if (isOpen || isAnimating) return;
        
        // Check if screwdriver is available (this will be checked by the trigger zone)
        StartCoroutine(AnimateCover(openAngle));
    }
    
    /// <summary>
    /// Closes the cover (if needed for future functionality).
    /// </summary>
    public void CloseCover()
    {
        if (!isOpen || isAnimating) return;
        
        StartCoroutine(AnimateCover(closedAngle));
    }
    
    private IEnumerator AnimateCover(float targetAngle)
    {
        isAnimating = true;
        
        // Play sound if available
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        
        Vector3 startRotation = coverTransform.localEulerAngles;
        float startAngle = startRotation.y;
        float elapsed = 0f;
        
        // Store the raw target angle for final positioning
        float finalTargetAngle = targetAngle;
        
        // Normalize both angles to 0-360 range for calculation
        float normalizedStart = startAngle;
        while (normalizedStart < 0) normalizedStart += 360f;
        while (normalizedStart >= 360f) normalizedStart -= 360f;
        
        float normalizedTarget = targetAngle;
        while (normalizedTarget < 0) normalizedTarget += 360f;
        while (normalizedTarget >= 360f) normalizedTarget -= 360f;
        
        // Calculate angle difference
        // If target > start, go forward directly (e.g., 180 to 300 = +120)
        // If target < start, we need to decide: forward wrap or backward
        float angleDifference;
        
        if (targetAngle > startAngle)
        {
            // User wants forward direction (e.g., 180 to 300)
            // Calculate forward distance
            if (normalizedTarget >= normalizedStart)
            {
                angleDifference = normalizedTarget - normalizedStart; // Direct forward
            }
            else
            {
                // Wrap forward (shouldn't happen if target > start, but handle it)
                angleDifference = (360f - normalizedStart) + normalizedTarget;
            }
        }
        else if (targetAngle < startAngle)
        {
            // User wants backward direction
            if (normalizedTarget <= normalizedStart)
            {
                angleDifference = normalizedTarget - normalizedStart; // Direct backward (negative)
            }
            else
            {
                // Wrap backward
                angleDifference = normalizedTarget - (normalizedStart + 360f);
            }
        }
        else
        {
            angleDifference = 0f;
        }
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Apply animation curve if available
            if (animationCurve != null && animationCurve.length > 0)
            {
                t = animationCurve.Evaluate(t);
            }
            
            // Calculate current angle by adding the interpolated difference
            float currentAngle = normalizedStart + (angleDifference * t);
            
            // Normalize to 0-360 range
            while (currentAngle < 0) currentAngle += 360f;
            while (currentAngle >= 360f) currentAngle -= 360f;
            
            coverTransform.localEulerAngles = new Vector3(startRotation.x, currentAngle, startRotation.z);
            
            yield return null;
        }
        
        // Ensure final rotation matches the target exactly
        float finalAngle = finalTargetAngle;
        while (finalAngle < 0) finalAngle += 360f;
        while (finalAngle >= 360f) finalAngle -= 360f;
        coverTransform.localEulerAngles = new Vector3(startRotation.x, finalAngle, startRotation.z);
        
        isOpen = true;
        isAnimating = false;
    }
    
    /// <summary>
    /// Check if cover is currently open.
    /// </summary>
    public bool IsOpen()
    {
        return isOpen;
    }
}

