using UnityEngine;

/// <summary>
/// Manages portal state and tracks which portal the player last used
/// </summary>
public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    [Header("Portal Settings")]
    [Tooltip("Array of portal GameObjects in MainScene (should be 3)")]
    public Portal[] portals = new Portal[3];

    private int lastUsedPortalIndex = -1;

    public int LastUsedPortalIndex => lastUsedPortalIndex;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Auto-find portals if not assigned
        if (portals[0] == null || portals[1] == null || portals[2] == null)
        {
            FindPortals();
        }
    }

    /// <summary>
    /// Find all portals in the scene
    /// </summary>
    private void FindPortals()
    {
        Portal[] foundPortals = FindObjectsOfType<Portal>();
        if (foundPortals != null && foundPortals.Length >= 3)
        {
            // Sort by portal index
            System.Array.Sort(foundPortals, (a, b) => a.portalIndex.CompareTo(b.portalIndex));

            for (int i = 0; i < Mathf.Min(foundPortals.Length, 3); i++)
            {
                if (i < portals.Length)
                {
                    portals[i] = foundPortals[i];
                }
            }
        }
    }

    /// <summary>
    /// Set which portal was last used
    /// </summary>
    public void SetLastUsedPortal(int portalIndex)
    {
        if (portalIndex >= 0 && portalIndex < 3)
        {
            lastUsedPortalIndex = portalIndex;
            Debug.Log($"Portal {portalIndex} was used to enter GameManager scene");
        }
    }

    /// <summary>
    /// Get the portal the player should return through
    /// </summary>
    public Portal GetReturnPortal()
    {
        if (lastUsedPortalIndex >= 0 && lastUsedPortalIndex < portals.Length)
        {
            return portals[lastUsedPortalIndex];
        }
        return null;
    }

    /// <summary>
    /// Teleport player back to the portal they entered from
    /// </summary>
    public void ReturnPlayerToPortal(GameObject player)
    {
        Portal returnPortal = GetReturnPortal();
        if (returnPortal != null && player != null)
        {
            // Position player at the portal location
            player.transform.position = returnPortal.transform.position;
            player.transform.rotation = returnPortal.transform.rotation;
            Debug.Log($"Player returned to Portal {lastUsedPortalIndex}");
        }
        else
        {
            Debug.LogWarning("Could not return player to portal - portal or player not found");
        }
    }
}

