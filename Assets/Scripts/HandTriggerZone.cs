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
    private bool isCutting = false;  // Flag to prevent multiple activations

    private void Start()
    {
        // Wait a frame for scene to fully load, then check save data and auto-cut if needed
        StartCoroutine(CheckSaveDataAndAutoCut());
    }

    private IEnumerator CheckSaveDataAndAutoCut()
    {
        // Wait for scene objects to fully initialize
        yield return null;
        yield return null;
        yield return new WaitForSeconds(0.2f); // Extra wait for SaveSystem to auto-load

        // Wait for SaveSystem to be ready and have loaded save data
        int maxWaitFrames = 30; // Wait up to 30 frames (0.5 seconds at 60fps)
        int waited = 0;
        while (SaveSystem.Instance == null && waited < maxWaitFrames)
        {
            yield return null;
            waited++;
        }

        // If the rope was already cut in a previous session, immediately apply the cut state
        // as soon as this scene loads, without waiting for the trigger zone or playing audio.
        if (SaveSystem.Instance != null &&
            SaveSystem.Instance.CurrentSaveData != null &&
            SaveSystem.Instance.CurrentSaveData.ropeCut)
        {
            Debug.Log("[HandTriggerZone] ropeCut=true in save data on scene load, performing instant cut (no sound).");

            // Make sure we have a reference to the rope object before cutting
            if (ropeHandcuff == null)
            {
                ropeHandcuff = FindRopeHandcuffInScene();
            }

            // Set flag to prevent activation (rope already cut)
            isCutting = true;
            PerformCutInstantly(false);
            // Flag stays true since rope is already cut - no more activations possible
        }
        else
        {
            Debug.Log($"[HandTriggerZone] SaveSystem check: Instance={SaveSystem.Instance != null}, " +
                      $"CurrentSaveData={SaveSystem.Instance?.CurrentSaveData != null}, " +
                      $"ropeCut={SaveSystem.Instance?.CurrentSaveData?.ropeCut ?? false}");
        }
    }

    /// <summary>
    /// Recursively search the entire scene for RopeHandcuff object (usually under Left Controller).
    /// </summary>
    private GameObject FindRopeHandcuffInScene()
    {
        // Method 1: Search by name in entire scene
        GameObject found = GameObject.Find("RopeHandcuff");
        if (found != null)
        {
            Debug.Log($"[HandTriggerZone] Found RopeHandcuff by name: {found.name} at path {GetFullPath(found.transform)}");
            return found;
        }

        // Method 2: Search in player hierarchy recursively
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Transform ropeTransform = FindChildRecursive(player.transform, "RopeHandcuff");
            if (ropeTransform != null)
            {
                Debug.Log($"[HandTriggerZone] Found RopeHandcuff in player hierarchy: {GetFullPath(ropeTransform)}");
                return ropeTransform.gameObject;
            }
        }

        // Method 3: Search by tag "Rope"
        try
        {
            GameObject[] ropes = GameObject.FindGameObjectsWithTag("Rope");
            if (ropes != null && ropes.Length > 0)
            {
                foreach (var rope in ropes)
                {
                    if (rope != null && rope.name.Contains("RopeHandcuff"))
                    {
                        Debug.Log($"[HandTriggerZone] Found RopeHandcuff by tag 'Rope': {GetFullPath(rope.transform)}");
                        return rope;
                    }
                }
            }
        }
        catch (UnityException)
        {
            // Tag doesn't exist, ignore
        }

        Debug.LogWarning("[HandTriggerZone] Could not find RopeHandcuff object in scene. Make sure it exists and is named 'RopeHandcuff'.");
        return null;
    }

    /// <summary>
    /// Recursively search for a child transform by name.
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent == null) return null;

        // Check direct children first
        foreach (Transform child in parent)
        {
            if (child.name == name || child.name.Contains(name))
            {
                return child;
            }
        }

        // Recursively search in children
        foreach (Transform child in parent)
        {
            Transform found = FindChildRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Get full hierarchy path for debugging.
    /// </summary>
    private string GetFullPath(Transform t)
    {
        if (t == null) return "";
        if (t.parent == null) return t.name;
        return GetFullPath(t.parent) + "/" + t.name;
    }

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
            // Check if handcuff still exists - if not, don't allow activation
            if (ropeHandcuff == null)
            {
                ropeHandcuff = FindRopeHandcuffInScene();
            }
            
            // Only allow activation if handcuff exists and we're not already cutting
            if (ropeHandcuff == null || isCutting)
            {
                return;
            }

            bool pressed = false;

            // Keyboard input for testing
            if (Keyboard.current != null && Keyboard.current[primaryKey].wasPressedThisFrame)
                pressed = true;

            // Gamepad / controller button
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                pressed = true;

            if (pressed)
            {
                // Set flag to prevent multiple activations
                isCutting = true;
                
                // Play sound immediately when button is pressed (not delayed)
                if (audioSource != null && ropeCutClip != null)
                {
                    audioSource.PlayOneShot(ropeCutClip);
                }
                
                // Start coroutine to enable hands and destroy rope after delay
                // Sound already played, so pass false to avoid double playback
                StartCoroutine(EnableHandsAndDestroyRope());
            }
        }
    }

    private IEnumerator EnableHandsAndDestroyRope()
    {
        // Wait for delay before removing rope and enabling hands
        // Note: Sound is already played immediately when button is pressed, not delayed
        yield return new WaitForSeconds(actionDelay);

        // Perform the cut (sound already played, but show notification)
        PerformCutInstantly(false, true);
        
        // Reset flag after cut is complete (handcuff is destroyed, so no more activations possible)
        isCutting = false;
    }

    /// <summary>
    /// Immediately enable hands and destroy the rope handcuff.
    /// Used both by the trigger (with sound) and by the save/load system (without sound).
    /// </summary>
    /// <param name="playSound">If true, plays the rope cut sound (used during normal gameplay only).</param>
    /// <param name="showNotification">If true, shows the "Hands Are Free" notification (default: same as playSound).</param>
    public void PerformCutInstantly(bool playSound, bool? showNotification = null)
    {
        // Optionally play sound
        if (playSound && audioSource != null && ropeCutClip != null)
        {
            audioSource.PlayOneShot(ropeCutClip);
        }

        // Enable hands
        if (handToggle != null)
        {
            handToggle.handsEnabled = true;
            handToggle.ApplyHandState();
            Debug.Log("Hands enabled (instant cut).");
        }

        // Find rope if not already assigned
        if (ropeHandcuff == null)
        {
            ropeHandcuff = FindRopeHandcuffInScene();
        }

        // Destroy rope
        if (ropeHandcuff != null)
        {
            Debug.Log($"[HandTriggerZone] Destroying RopeHandcuff: {GetFullPath(ropeHandcuff.transform)}");
            Destroy(ropeHandcuff);
            Debug.Log("Rope handcuff destroyed (instant cut).");
        }
        else
        {
            Debug.LogWarning("[HandTriggerZone] PerformCutInstantly called but RopeHandcuff object not found!");
        }
        
        // Show notification that hands are free (only during normal gameplay, not when loading from save)
        // If showNotification is not specified, default to playSound value
        bool shouldShowNotification = showNotification ?? playSound;
        if (shouldShowNotification)
        {
            PlayerFollowUIManager.ShowNotification("Hands Are Free.", 5f);
        }
    }
}
