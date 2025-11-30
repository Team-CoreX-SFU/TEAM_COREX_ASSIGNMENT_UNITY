using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Trigger zone that detects when a screwdriver is used to interact with power supply components.
/// Attach this to a GameObject with a Collider (set as Trigger) near the interaction point.
/// </summary>
public class ScrewdriverInteractionZone : MonoBehaviour
{
    [Header("Interaction Type")]
    [Tooltip("What type of interaction this zone handles")]
    public InteractionType interactionType = InteractionType.Cover;
    
    public enum InteractionType
    {
        Cover,      // Opens the power supply cover
        Fuse,       // Removes the fuse
        Switch      // Toggles the switch (optional, doesn't require screwdriver)
    }
    
    [Header("Target Components")]
    [Tooltip("The PowerSupplyCoverController to interact with (for Cover type)")]
    public PowerSupplyCoverController coverController;
    
    [Tooltip("The PowerSupplyFuseController to interact with (for Fuse type)")]
    public PowerSupplyFuseController fuseController;
    
    [Tooltip("The PowerSupplySwitchController to interact with (for Switch type)")]
    public PowerSupplySwitchController switchController;
    
    [Header("Screwdriver Detection")]
    [Tooltip("Tag to identify the screwdriver object")]
    public string screwdriverTag = "Screwdriver";
    
    [Tooltip("Layer mask for screwdriver detection (optional)")]
    public LayerMask screwdriverLayer = -1;
    
    [Header("Interaction Settings")]
    [Tooltip("Require screwdriver to be held for interaction")]
    public bool requireScrewdriver = true;
    
    [Tooltip("Auto-trigger on enter (if false, requires button press)")]
    public bool autoTrigger = true;
    
    [Header("Input Settings (if autoTrigger is false)")]
    [Tooltip("Button to press for interaction")]
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip interactionSound;
    
    private bool screwdriverInZone = false;
    private GameObject currentScrewdriver = null;
    private bool hasInteracted = false;
    
    void OnTriggerEnter(Collider other)
    {
        // Check if screwdriver entered the zone
        if (IsScrewdriver(other.gameObject))
        {
            screwdriverInZone = true;
            currentScrewdriver = other.gameObject;
            
            // Check if screwdriver is being held
            XRGrabInteractable grabInteractable = other.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null && grabInteractable.isSelected)
            {
                if (autoTrigger)
                {
                    TryInteract();
                }
            }
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (IsScrewdriver(other.gameObject))
        {
            XRGrabInteractable grabInteractable = other.GetComponent<XRGrabInteractable>();
            
            // If auto-trigger is off, check for button press
            if (!autoTrigger && grabInteractable != null && grabInteractable.isSelected)
            {
                if (Input.GetKeyDown(interactionKey))
                {
                    TryInteract();
                }
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (IsScrewdriver(other.gameObject))
        {
            screwdriverInZone = false;
            currentScrewdriver = null;
        }
    }
    
    private bool IsScrewdriver(GameObject obj)
    {
        // Check by tag
        if (!string.IsNullOrEmpty(screwdriverTag) && obj.CompareTag(screwdriverTag))
        {
            return true;
        }
        
        // Check by component
        if (obj.GetComponent<ScrewdriverPickup>() != null)
        {
            return true;
        }
        
        // Check by layer
        if (screwdriverLayer != -1 && ((1 << obj.layer) & screwdriverLayer) != 0)
        {
            return true;
        }
        
        return false;
    }
    
    private void TryInteract()
    {
        if (hasInteracted) return;
        
        // Check if screwdriver is required and present
        if (requireScrewdriver && !screwdriverInZone)
        {
            return;
        }
        
        // Check if screwdriver is being held
        if (requireScrewdriver && currentScrewdriver != null)
        {
            XRGrabInteractable grabInteractable = currentScrewdriver.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null || !grabInteractable.isSelected)
            {
                return;
            }
        }
        
        // Perform interaction based on type
        switch (interactionType)
        {
            case InteractionType.Cover:
                if (coverController != null && !coverController.IsOpen())
                {
                    coverController.OpenCover();
                    hasInteracted = true;
                    PlayInteractionSound();
                }
                break;
                
            case InteractionType.Fuse:
                if (fuseController != null && !fuseController.IsRemoved())
                {
                    fuseController.RemoveFuse();
                    hasInteracted = true;
                    PlayInteractionSound();
                }
                break;
                
            case InteractionType.Switch:
                if (switchController != null)
                {
                    switchController.ToggleSwitch();
                    PlayInteractionSound();
                }
                break;
        }
    }
    
    private void PlayInteractionSound()
    {
        if (audioSource != null && interactionSound != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
    }
    
    /// <summary>
    /// Reset the interaction state (useful if you want to allow multiple interactions).
    /// </summary>
    public void ResetInteraction()
    {
        hasInteracted = false;
    }
}

