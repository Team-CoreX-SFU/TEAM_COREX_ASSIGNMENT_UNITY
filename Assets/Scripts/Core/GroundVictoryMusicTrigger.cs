using UnityEngine;

/// <summary>
/// Plays a victory music clip once when the player touches this ground trigger.
/// Attach this to a GameObject with a Collider set as "Is Trigger".
/// </summary>
[RequireComponent(typeof(Collider))]
public class GroundVictoryMusicTrigger : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Tag assigned to the player's root object (usually XR Origin / Player)")]
    public string playerTag = "Player";

    [Header("Audio")]
    [Tooltip("Audio source used to play the victory music")]
    public AudioSource audioSource;

    [Tooltip("Victory music clip to play when the player touches this ground")]
    public AudioClip victoryMusic;

    [Tooltip("If true, this trigger will only play once and then disable itself")]
    public bool playOnce = true;

    private bool hasPlayed = false;

    private void Awake()
    {
        // Ensure collider is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[GroundVictoryMusicTrigger] OnTriggerEnter called on '{gameObject.name}' by '{other.gameObject.name}' (Tag: {other.tag})");

        if (playOnce && hasPlayed)
        {
            Debug.Log($"[GroundVictoryMusicTrigger] Already played once, ignoring trigger");
            return;
        }

        if (!IsPlayer(other))
        {
            Debug.Log($"[GroundVictoryMusicTrigger] Collider '{other.gameObject.name}' is not the player (tag: {other.tag}, expected: {playerTag})");
            return;
        }

        Debug.Log($"[GroundVictoryMusicTrigger] Player detected! Checking audio setup...");

        if (audioSource == null)
        {
            Debug.LogError($"[GroundVictoryMusicTrigger] AudioSource is NULL! Please assign an AudioSource component.");
            return;
        }

        if (victoryMusic == null)
        {
            Debug.LogError($"[GroundVictoryMusicTrigger] VictoryMusic clip is NULL! Please assign an AudioClip.");
            return;
        }

        // All checks passed - play music
        Debug.Log($"[GroundVictoryMusicTrigger] Playing victory music '{victoryMusic.name}' on '{gameObject.name}'");
        audioSource.PlayOneShot(victoryMusic);
        hasPlayed = true;

        // Close all doors that were opened by keypads
        // This works for ANY victory ground trigger - dynamic, not hardcoded
        Debug.Log($"[GroundVictoryMusicTrigger] Ground breaking music triggered on '{gameObject.name}' - closing all keypad doors");
        KeyPadScript.TriggerCloseAllDoors();

        // Hide the "time left to run from kidnapper" timer UI
        Debug.Log($"[GroundVictoryMusicTrigger] Hiding kidnapper timer UI");
        KidnapperAI.HideKidnapperTimer();
    }

    /// <summary>
    /// Check if the collider belongs to the player (by tag or parent tag).
    /// </summary>
    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag(playerTag))
            return true;

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


