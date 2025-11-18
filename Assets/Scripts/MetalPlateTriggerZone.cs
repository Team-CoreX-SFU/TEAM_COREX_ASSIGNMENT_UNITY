using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandTriggerZone : MonoBehaviour
{
    [Header("Hand Manipulation Toggle")]
    public HandMovementToggleInspector handToggle;

    [Header("Rope Handcuff Object to Remove")]
    public GameObject ropeHandcuff;

    [Header("Audio")]
    public AudioSource audioSource;   // Assign AudioSource here
    public AudioClip ropeCutClip;     // Assign AudioClip here

    [Header("Primary button for activation")]
    public Key primaryKey = Key.Space;

    [Header("Settings")]
    public float actionDelay = 1f;    // Delay in seconds before enabling hands and destroying rope

    private bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    private void Update()
    {
        if (playerInside)
        {
            bool pressed = false;

            // Keyboard input for testing
            if (Keyboard.current != null && Keyboard.current[primaryKey].wasPressedThisFrame)
                pressed = true;

            // Gamepad / controller button
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                pressed = true;

            if (pressed)
            {
                // Play sound immediately
                if (audioSource != null && ropeCutClip != null)
                    audioSource.PlayOneShot(ropeCutClip);

                // Start coroutine to enable hands and destroy rope after delay
                StartCoroutine(EnableHandsAndDestroyRope());
            }
        }
    }

    private IEnumerator EnableHandsAndDestroyRope()
    {
        // Wait for delay
        yield return new WaitForSeconds(actionDelay);

        // Enable hands
        if (handToggle != null)
        {
            handToggle.handsEnabled = true;
            handToggle.ApplyHandState();
            Debug.Log("Hands enabled after delay.");
        }

        // Destroy rope
        if (ropeHandcuff != null)
        {
            Destroy(ropeHandcuff);
            Debug.Log("Rope handcuff destroyed after delay.");
        }
    }
}