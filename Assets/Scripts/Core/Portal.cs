using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Portal script that transports player between MainScene and GameManagerScene
/// </summary>
public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Unique index for this portal (0, 1, or 2)")]
    public int portalIndex = 0;

    [Tooltip("Scene to load when player enters portal")]
    public string targetSceneName = "GameManagerScene";

    [Tooltip("Tag of the player object")]
    public string playerTag = "Player";

    [Header("Visual Feedback")]
    public bool showDebugGizmo = true;

    private bool playerInRange = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            EnterPortal();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
        }
    }

    /// <summary>
    /// Called when player enters the portal
    /// </summary>
    private void EnterPortal()
    {
        // Store which portal was used
        PortalManager portalManager = FindObjectOfType<PortalManager>();
        if (portalManager == null)
        {
            // Create PortalManager if it doesn't exist
            GameObject managerObj = new GameObject("PortalManager");
            portalManager = managerObj.AddComponent<PortalManager>();
        }

        portalManager.SetLastUsedPortal(portalIndex);

        // Load the target scene
        SceneManager.LoadScene(targetSceneName);
    }

    void OnDrawGizmos()
    {
        if (showDebugGizmo)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        }
    }
}

