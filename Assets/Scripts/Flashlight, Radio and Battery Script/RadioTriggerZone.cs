using UnityEngine;

/// <summary>
/// Allows the player to toggle the radio when inside a trigger volume (no grabbing required).
/// Attach this to a GameObject with a collider marked as "Is Trigger".
/// </summary>
public class RadioTriggerZone : MonoBehaviour
{
    public RadioController radio;
    [Tooltip("Tag assigned to the player's root object (usually XR Origin).")]
    public string playerTag = "Player";
    public bool autoPowerOffOnExit = true;

    private bool playerInside = false;

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerInside = true;
        if (radio != null)
        {
            radio.SetPlayerInRange(true, autoPowerOffOnExit);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!playerInside || !IsPlayer(other))
        {
            return;
        }

        playerInside = false;
        if (radio != null)
        {
            radio.SetPlayerInRange(false, autoPowerOffOnExit);
        }
    }

    bool IsPlayer(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.CompareTag(playerTag))
        {
            return true;
        }

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
}

