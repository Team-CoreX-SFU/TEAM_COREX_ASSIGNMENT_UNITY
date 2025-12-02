using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Trigger zone that calls GameManager.SaveGame() when the player is inside
/// the zone and presses an interaction key / gamepad button.
///
/// This is similar in behaviour to PowerSupplyTriggerZone, but only performs
/// a single action: saving the game.
/// </summary>
public class SaveTriggerZone : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Tag assigned to the player's root object (usually XR Origin or Player)")]
    public string playerTag = "Player";

    [Header("Input Settings")]
    [Tooltip("Key to press for interaction (keyboard)")]
    public Key interactionKey = Key.E;

    [Tooltip("Also allow gamepad south button (A/X) to trigger the save")]
    public bool allowGamepadButton = true;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip saveSound;

    private bool playerInside = false;
    private GameManager gameManager;

    private void Awake()
    {
        // Cache GameManager reference
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("SaveTriggerZone: No GameManager found in scene. One will be created at runtime.");
            GameObject managerObj = new GameObject("GameManager");
            gameManager = managerObj.AddComponent<GameManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            playerInside = false;
        }
    }

    private void Update()
    {
        if (!playerInside || gameManager == null)
            return;

        bool pressed = false;

        // Keyboard
        if (Keyboard.current != null && Keyboard.current[interactionKey].wasPressedThisFrame)
        {
            pressed = true;
        }

        // Gamepad
        if (!pressed && allowGamepadButton && Gamepad.current != null &&
            Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            pressed = true;
        }

        if (pressed)
        {
            TriggerSave();
        }
    }

    private void TriggerSave()
    {
        if (gameManager == null)
        {
            Debug.LogError("SaveTriggerZone: GameManager reference is missing; cannot save.");
            return;
        }

        bool success = false;
        gameManager.SaveGame();
        success = true; // SaveGame() already logs its own success/failure.

        if (success)
        {
            PlaySaveSound();
        }
    }

    private void PlaySaveSound()
    {
        if (audioSource != null && saveSound != null)
        {
            audioSource.PlayOneShot(saveSound);
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        // Direct tag
        if (other.CompareTag(playerTag))
            return true;

        // Parent chain
        Transform t = other.transform;
        while (t != null)
        {
            if (t.CompareTag(playerTag))
                return true;
            t = t.parent;
        }

        return false;
    }
}


