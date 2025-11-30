using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Trigger zone for power supply interaction.
/// Player enters zone and presses 'E' key multiple times to perform steps one by one:
/// 1. First 'E' press → Cover opens
/// 2. Second 'E' press → Switch turns off AND Fuse is removed (both happen together)
/// Requires screwdriver to be collected first.
/// </summary>
public class PowerSupplyTriggerZone : MonoBehaviour
{
    [Header("Power Supply Components")]
    [Tooltip("The PowerSupplyCoverController to open")]
    public PowerSupplyCoverController coverController;
    
    [Tooltip("The PowerSupplySwitchController to turn off")]
    public PowerSupplySwitchController switchController;
    
    [Tooltip("The PowerSupplyFuseController to remove")]
    public PowerSupplyFuseController fuseController;
    
    [Header("Player Detection")]
    [Tooltip("Tag assigned to the player's root object (usually XR Origin or Player)")]
    public string playerTag = "Player";
    
    [Header("Input Settings")]
    [Tooltip("Key to press for interaction")]
    public Key interactionKey = Key.E;
    
    [Tooltip("Require screwdriver to be collected before interaction")]
    public bool requireScrewdriver = true;
    
    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip interactionSound;
    
    [Header("Settings")]
    [Tooltip("Delay between steps (wait for animations to complete)")]
    public float delayBetweenSteps = 0.5f;
    
    private bool playerInside = false;
    private int currentStep = 0; // 0 = not started, 1 = cover opened, 2 = switch off, 3 = fuse removed
    private bool isProcessing = false; // Prevent multiple rapid presses
    
    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            playerInside = true;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            playerInside = false;
        }
    }
    
    void Update()
    {
        if (playerInside && !isProcessing)
        {
            bool pressed = false;
            
            // Check keyboard input
            if (Keyboard.current != null && Keyboard.current[interactionKey].wasPressedThisFrame)
            {
                pressed = true;
            }
            
            // Check gamepad input (optional)
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                pressed = true;
            }
            
            if (pressed)
            {
                TryInteract();
            }
        }
    }
    
    private void TryInteract()
    {
        // Check if screwdriver is required and collected
        if (requireScrewdriver && !ScrewdriverPickup.HasScrewdriver())
        {
            Debug.Log("PowerSupplyTriggerZone: Screwdriver required but not collected!");
            return;
        }
        
        // Perform actions step by step
        if (currentStep == 0)
        {
            // Step 1: Open Cover
            if (coverController != null && !coverController.IsOpen())
            {
                isProcessing = true;
                coverController.OpenCover();
                currentStep = 1;
                PlayInteractionSound();
                Debug.Log("PowerSupplyTriggerZone: Step 1 - Cover opening...");
                
                // Wait for cover animation to complete, then allow next step
                StartCoroutine(WaitForNextStep(coverController.animationDuration));
            }
        }
        else if (currentStep == 1)
        {
            // Step 2: Turn Off Switch AND Remove Fuse (both at the same time)
            bool actionPerformed = false;
            
            // Turn Off Switch
            if (switchController != null && switchController.IsOn())
            {
                switchController.TurnOff();
                actionPerformed = true;
                Debug.Log("PowerSupplyTriggerZone: Step 2 - Switch turning off...");
            }
            
            // Remove Fuse
            if (fuseController != null && !fuseController.IsRemoved())
            {
                fuseController.RemoveFuse();
                actionPerformed = true;
                Debug.Log("PowerSupplyTriggerZone: Step 2 - Fuse removing...");
            }
            
            if (actionPerformed)
            {
                isProcessing = true;
                currentStep = 2;
                PlayInteractionSound();
                
                // Wait for switch animation to complete (fuse removal happens independently)
                if (switchController != null)
                {
                    StartCoroutine(WaitForNextStep(switchController.animationDuration));
                }
                else
                {
                    // If no switch, just wait a bit for fuse removal
                    StartCoroutine(WaitForNextStep(0.5f));
                }
            }
        }
        else
        {
            Debug.Log("PowerSupplyTriggerZone: All steps completed!");
        }
    }
    
    private System.Collections.IEnumerator WaitForNextStep(float animationDuration)
    {
        // Wait for animation to complete plus a small delay
        yield return new WaitForSeconds(animationDuration + delayBetweenSteps);
        isProcessing = false;
    }
    
    private void PlayInteractionSound()
    {
        if (audioSource != null && interactionSound != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
    }
    
    private bool IsPlayer(Collider other)
    {
        if (other == null)
        {
            return false;
        }
        
        // Check direct tag
        if (other.CompareTag(playerTag))
        {
            return true;
        }
        
        // Check parent hierarchy for player tag
        Transform t = other.transform;
        while (t != null)
        {
            if (t.CompareTag(playerTag))
            {
                return true;
            }
            t = t.parent;
        }
        
        return false;
    }
    
    /// <summary>
    /// Reset interaction state (useful for testing or resetting game state).
    /// </summary>
    public void ResetInteraction()
    {
        currentStep = 0;
        isProcessing = false;
    }
    
    /// <summary>
    /// Manually trigger the interaction (useful for testing or scripted events).
    /// </summary>
    public void TriggerInteraction()
    {
        TryInteract();
    }
}

