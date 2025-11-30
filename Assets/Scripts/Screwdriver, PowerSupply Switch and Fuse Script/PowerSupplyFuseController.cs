using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the Power Supply Fuse removal.
/// Requires a screwdriver to be used for interaction.
/// </summary>
public class PowerSupplyFuseController : MonoBehaviour
{
    [Header("Fuse Settings")]
    [Tooltip("The GameObject representing the fuse (if null, uses this GameObject)")]
    public GameObject fuseObject;
    
    [Header("Removal Settings")]
    [Tooltip("Delay before fuse disappears after interaction")]
    public float removalDelay = 0.5f;
    
    [Tooltip("Should the fuse be destroyed or just disabled?")]
    public bool destroyOnRemoval = true;
    
    [Header("Animation Settings (Optional)")]
    [Tooltip("Animate fuse removal (e.g., move up and fade out)")]
    public bool animateRemoval = true;
    
    [Tooltip("Distance to move fuse upward when removing")]
    public float removalHeight = 0.2f;
    
    [Tooltip("Duration of removal animation")]
    public float removalAnimationDuration = 0.8f;
    
    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip removalSound;
    
    private bool isRemoved = false;
    private bool isRemoving = false;
    private Vector3 originalPosition;
    private Renderer fuseRenderer;
    private Material fuseMaterial;
    
    void Awake()
    {
        // If no fuse object assigned, use this GameObject
        if (fuseObject == null)
        {
            fuseObject = gameObject;
        }
        
        // Store original position for animation
        originalPosition = fuseObject.transform.localPosition;
        
        // Get renderer for fade effect
        fuseRenderer = fuseObject.GetComponent<Renderer>();
        if (fuseRenderer != null)
        {
            fuseMaterial = fuseRenderer.material;
        }
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
    /// Removes the fuse if screwdriver is present and fuse hasn't been removed.
    /// </summary>
    public void RemoveFuse()
    {
        if (isRemoved || isRemoving) return;
        
        // Check if screwdriver is available (this will be checked by the trigger zone)
        StartCoroutine(RemoveFuseCoroutine());
    }
    
    private IEnumerator RemoveFuseCoroutine()
    {
        isRemoving = true;
        
        // Play sound if available
        if (audioSource != null && removalSound != null)
        {
            audioSource.PlayOneShot(removalSound);
        }
        
        if (animateRemoval)
        {
            // Animate removal: move up and fade out
            float elapsed = 0f;
            Vector3 startPos = fuseObject.transform.localPosition;
            Vector3 endPos = startPos + Vector3.up * removalHeight;
            Color startColor = Color.white;
            
            if (fuseMaterial != null)
            {
                startColor = fuseMaterial.color;
            }
            
            while (elapsed < removalAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / removalAnimationDuration;
                
                // Move upward
                fuseObject.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                
                // Fade out
                if (fuseMaterial != null)
                {
                    Color currentColor = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), t);
                    fuseMaterial.color = currentColor;
                }
                
                yield return null;
            }
        }
        else
        {
            // Simple delay before removal
            yield return new WaitForSeconds(removalDelay);
        }
        
        // Remove the fuse
        isRemoved = true;
        isRemoving = false;
        
        if (destroyOnRemoval)
        {
            Destroy(fuseObject);
        }
        else
        {
            fuseObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Check if fuse has been removed.
    /// </summary>
    public bool IsRemoved()
    {
        return isRemoved;
    }
}

