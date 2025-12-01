using UnityEngine;

/// <summary>
/// Trigger that REVEALS a specific keypad pin when the player views it.
/// Requirements:
/// - Player must enter this trigger
/// - Flashlight must be ON (UV viewing)
/// Behavior:
/// - Registers the pin with PlayerFollowUIManager (Exit Door keypad helper)
/// - Shows/updates the keypad pins notification (does NOT auto-hide)
/// - Triggers when:
///   - Player enters while flashlight is ON, or
///   - Player is already inside and then switches flashlight ON
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitDoorKeypadPinsTrigger : MonoBehaviour
{
    [Header("Pin Settings")]
    [Tooltip("Digit value of this pin (e.g. 1, 5, 2)")]
    public int pinValue = 1;

    [Header("Flashlight Requirement")]
    [Tooltip("Require flashlight to be ON to reveal this pin")]
    public bool requireFlashlightOn = true;

    private bool playerInside = false;
    private bool pinRevealed = false;
    private FlashlightController cachedFlashlight;

    private void Reset()
    {
        // Ensure collider is set as trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        // If flashlight is already on when entering, reveal immediately
        TryRevealPin();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
    }

    private bool IsFlashlightOn()
    {
        // Cache flashlight reference the first time
        if (cachedFlashlight == null)
        {
            cachedFlashlight = FindObjectOfType<FlashlightController>();
        }

        if (cachedFlashlight == null) return false;
        return cachedFlashlight.isOn;
    }

    private void Update()
    {
        // Handle the case where player is already inside and turns flashlight ON
        if (playerInside && !pinRevealed)
        {
            TryRevealPin();
        }
    }

    private void TryRevealPin()
    {
        if (pinRevealed) return;

        if (requireFlashlightOn && !IsFlashlightOn())
        {
            return;
        }

        // Register this pin and show/update the notification using PlayerFollowUI
        PlayerFollowUIManager.RegisterExitDoorPin(pinValue);
        PlayerFollowUIManager.ShowExitDoorKeypadNotification();
        pinRevealed = true;
    }
}

