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
        if (playOnce && hasPlayed)
            return;

        if (!IsPlayer(other))
            return;

        if (audioSource != null && victoryMusic != null)
        {
            audioSource.PlayOneShot(victoryMusic);
            hasPlayed = true;
        }
        else
        {
            Debug.LogWarning("[GroundVictoryMusicTrigger] Missing AudioSource or VictoryMusic clip.");
        }
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


