using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Screwdriver pickup script. Tracks when screwdriver is collected.
/// Similar to BatteryPickup - once collected, player has the screwdriver.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class ScrewdriverPickup : MonoBehaviour
{
    [Header("Screwdriver Settings")]
    [Tooltip("Tag to identify this screwdriver")]
    public string screwdriverTag = "Screwdriver";
    
    [Header("Collection Settings")]
    [Tooltip("Should screwdriver disappear after being collected?")]
    public bool disappearOnCollection = true;
    
    [Tooltip("Delay before screwdriver disappears")]
    public float disappearDelay = 0.1f;
    
    [Header("Legacy Assignments (Optional - for backward compatibility)")]
    public PowerSupplyCoverController powerSupplyCover;
    public PowerSupplyFuseController[] fuses;

    private XRGrabInteractable grabInteractable;
    private static bool hasScrewdriver = false; // Static to track across all instances

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to grab events
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        
        // Set tag if not already set
        if (!gameObject.CompareTag(screwdriverTag))
        {
            gameObject.tag = screwdriverTag;
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Mark that player has collected the screwdriver
        hasScrewdriver = true;
        
        // Legacy support: notify components
        if (powerSupplyCover != null)
        {
            powerSupplyCover.HasScrewdriver();
        }

        if (fuses != null)
        {
            foreach (var fuse in fuses)
            {
                if (fuse != null)
                {
                    fuse.HasScrewdriver();
                }
            }
        }
        
        // Disable screwdriver object if it should disappear
        if (disappearOnCollection)
        {
            StartCoroutine(Disappear());
        }
    }
    
    private System.Collections.IEnumerator Disappear()
    {
        // Wait for XR system to finish grab
        yield return new WaitForSeconds(disappearDelay);
        
        // Destroy screwdriver object
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Check if player has collected the screwdriver.
    /// </summary>
    public static bool HasScrewdriver()
    {
        return hasScrewdriver;
    }
    
    /// <summary>
    /// Reset screwdriver collection state (useful for testing or resetting game state).
    /// </summary>
    public static void ResetScrewdriverState()
    {
        hasScrewdriver = false;
    }
}
