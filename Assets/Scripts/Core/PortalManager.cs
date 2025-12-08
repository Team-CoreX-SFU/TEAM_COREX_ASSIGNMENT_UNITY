using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages portal state and tracks which portal the player last used
/// Handles portals in both MainScene (3 portals) and GameManagerScene (1 return portal)
/// </summary>
public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    [Header("Portal Settings")]
    [Tooltip("Array of portal GameObjects in MainScene (should be 3)")]
    public Portal[] mainScenePortals = new Portal[3];

    [Tooltip("Default portal index to use if no save file exists (first time playing). This will be updated from save file if available.")]
    public int defaultPortalIndex = 0;

    private int lastUsedPortalIndex = -1;
    private string currentSceneName;

    public int LastUsedPortalIndex => lastUsedPortalIndex;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Subscribe to scene loaded event to ensure portals are found when scene loads
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        Debug.Log($"[PORTAL MANAGER] Scene loaded: {currentSceneName}");

        // Only find portals if we're in MainScene
        if (currentSceneName == "MainScene")
        {
            Debug.Log("[PORTAL MANAGER] MainScene loaded! Finding and activating portals...");

            // Wait a frame for scene objects to fully initialize, then find and activate portals
            StartCoroutine(FindAndActivatePortalsDelayed());
        }
    }

    void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;

        // Only find portals if we're in MainScene
        if (currentSceneName == "MainScene")
        {
            Debug.Log("[PORTAL MANAGER] Start() in MainScene");

            // Auto-find portals if not assigned
            if (mainScenePortals[0] == null || mainScenePortals[1] == null || mainScenePortals[2] == null)
            {
                Debug.Log("[PORTAL MANAGER] Portals not assigned, searching...");
                FindMainScenePortals();
            }

            // Ensure all portals are active and visible (prevent them from disappearing)
            EnsurePortalsAreActive();

            Debug.Log($"[PORTAL MANAGER] In MainScene. Found portals: [0]={mainScenePortals[0] != null}, [1]={mainScenePortals[1] != null}, [2]={mainScenePortals[2] != null}. Last used index: {lastUsedPortalIndex}, Default: {defaultPortalIndex}");
        }
    }

    /// <summary>
    /// Coroutine to find and activate portals after scene is fully loaded
    /// </summary>
    private System.Collections.IEnumerator FindAndActivatePortalsDelayed()
    {
        // Wait a few frames for scene objects to fully initialize
        yield return null;
        yield return null;
        yield return new WaitForSeconds(0.2f);

        Debug.Log("[PORTAL MANAGER] Finding portals after scene load...");

        // Clear old references
        mainScenePortals[0] = null;
        mainScenePortals[1] = null;
        mainScenePortals[2] = null;

        // Try finding portals multiple times with increasing delays
        for (int attempt = 0; attempt < 3; attempt++)
        {
            Debug.Log($"[PORTAL MANAGER] Portal finding attempt {attempt + 1}/3");

            // Find portals
            FindMainScenePortals();

            // Check if we found all portals
            bool allFound = mainScenePortals[0] != null && mainScenePortals[1] != null && mainScenePortals[2] != null;

            if (allFound)
            {
                Debug.Log("[PORTAL MANAGER] All portals found!");
                break;
            }
            else
            {
                Debug.LogWarning($"[PORTAL MANAGER] Not all portals found yet. Found: [0]={mainScenePortals[0] != null}, [1]={mainScenePortals[1] != null}, [2]={mainScenePortals[2] != null}");
                yield return new WaitForSeconds(0.3f);
            }
        }

        // Ensure portals are active
        EnsurePortalsAreActive();

        // Log results
        Debug.Log($"[PORTAL MANAGER] After scene load - Found portals: [0]={mainScenePortals[0] != null}, [1]={mainScenePortals[1] != null}, [2]={mainScenePortals[2] != null}");

        // Double-check after another delay
        yield return new WaitForSeconds(0.3f);
        EnsurePortalsAreActive();

        // Final check
        yield return new WaitForSeconds(0.5f);
        FindMainScenePortals();
        EnsurePortalsAreActive();
    }

    /// <summary>
    /// Update the default portal index (called when entering a portal)
    /// </summary>
    public void UpdateDefaultPortalIndex(int newDefault)
    {
        if (newDefault >= 0 && newDefault < 3)
        {
            defaultPortalIndex = newDefault;
            Debug.Log($"[PORTAL MANAGER] Default portal index updated to: {defaultPortalIndex}");
        }
    }

    /// <summary>
    /// Find all portals in MainScene
    /// </summary>
    private void FindMainScenePortals()
    {
        Debug.Log("[PORTAL MANAGER] FindMainScenePortals() called. Searching for portals...");

        // Search for all portals, including inactive ones
        Portal[] foundPortals = FindObjectsOfType<Portal>(includeInactive: true);

        Debug.Log($"[PORTAL MANAGER] Found {foundPortals?.Length ?? 0} total Portal components in scene");

        if (foundPortals != null && foundPortals.Length > 0)
        {
            // Log all found portals for debugging
            foreach (Portal portal in foundPortals)
            {
                if (portal != null)
                {
                    Debug.Log($"[PORTAL MANAGER] Found portal: {portal.gameObject.name}, Index: {portal.portalIndex}, Active: {portal.gameObject.activeInHierarchy}");
                }
            }

            // Filter portals that are in MainScene (index 0, 1, or 2)
            System.Collections.Generic.List<Portal> mainPortals = new System.Collections.Generic.List<Portal>();
            foreach (Portal portal in foundPortals)
            {
                if (portal != null && portal.portalIndex >= 0 && portal.portalIndex < 3)
                {
                    mainPortals.Add(portal);
                    Debug.Log($"[PORTAL MANAGER] Added portal {portal.portalIndex} ({portal.gameObject.name}) to mainPortals list");
                }
                else if (portal != null)
                {
                    Debug.Log($"[PORTAL MANAGER] Skipped portal {portal.gameObject.name} with index {portal.portalIndex} (not 0-2)");
                }
            }

            Debug.Log($"[PORTAL MANAGER] Found {mainPortals.Count} MainScene portals (index 0-2)");

            // Sort by portal index
            mainPortals.Sort((a, b) => a.portalIndex.CompareTo(b.portalIndex));

            // Clear the array first
            for (int i = 0; i < mainScenePortals.Length; i++)
            {
                mainScenePortals[i] = null;
            }

            // Assign portals to array by their index
            foreach (Portal portal in mainPortals)
            {
                if (portal != null && portal.portalIndex >= 0 && portal.portalIndex < mainScenePortals.Length)
                {
                    mainScenePortals[portal.portalIndex] = portal;
                    Debug.Log($"[PORTAL MANAGER] Assigned portal {portal.portalIndex} ({portal.gameObject.name}) to mainScenePortals[{portal.portalIndex}]");
                }
            }

            // Verify assignment
            Debug.Log($"[PORTAL MANAGER] Portal assignment complete: [0]={mainScenePortals[0] != null}, [1]={mainScenePortals[1] != null}, [2]={mainScenePortals[2] != null}");
        }
        else
        {
            Debug.LogError("[PORTAL MANAGER] No Portal components found in scene! Make sure portals have Portal component attached.");
        }
    }

    /// <summary>
    /// Ensure all portals in MainScene are active and visible
    /// This prevents portals from disappearing when loading from GameManager
    /// </summary>
    private void EnsurePortalsAreActive()
    {
        // Re-find portals if they're null
        if (mainScenePortals[0] == null || mainScenePortals[1] == null || mainScenePortals[2] == null)
        {
            Debug.Log("[PORTAL MANAGER] Some portals are null, re-finding...");
            FindMainScenePortals();
        }

        int activeCount = 0;
        for (int i = 0; i < mainScenePortals.Length; i++)
        {
            if (mainScenePortals[i] != null)
            {
                // Ensure the portal GameObject and all its parents are active
                if (!mainScenePortals[i].gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"[PORTAL MANAGER] Portal {i} ({mainScenePortals[i].gameObject.name}) was inactive! Activating it now...");
                    mainScenePortals[i].gameObject.SetActive(true);
                }

                // Also ensure the portal itself is enabled
                if (!mainScenePortals[i].enabled)
                {
                    Debug.LogWarning($"[PORTAL MANAGER] Portal {i} component was disabled! Enabling it now...");
                    mainScenePortals[i].enabled = true;
                }

                // Ensure collider is enabled (portals need colliders to work)
                Collider portalCollider = mainScenePortals[i].GetComponent<Collider>();
                if (portalCollider != null && !portalCollider.enabled)
                {
                    Debug.LogWarning($"[PORTAL MANAGER] Portal {i} collider was disabled! Enabling it now...");
                    portalCollider.enabled = true;
                }

                activeCount++;
            }
            else
            {
                Debug.LogWarning($"[PORTAL MANAGER] Portal {i} is null!");
            }
        }

        Debug.Log($"[PORTAL MANAGER] EnsurePortalsAreActive complete. Active portals: {activeCount}/3");
    }

    /// <summary>
    /// Public method to ensure portals are active (can be called from GameManager or other scripts)
    /// </summary>
    public void EnsurePortalsActive()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            EnsurePortalsAreActive();
        }
    }

    /// <summary>
    /// Set which portal was last used (called when leaving MainScene)
    /// </summary>
    public void SetLastUsedPortal(int portalIndex)
    {
        Debug.Log($"[PORTAL MANAGER] SetLastUsedPortal called with index: {portalIndex}");
        if (portalIndex >= 0 && portalIndex < 3)
        {
            lastUsedPortalIndex = portalIndex;
            Debug.Log($"[PORTAL MANAGER] ✓ Portal {portalIndex} was used to enter GameManager scene (saved in memory). LastUsedPortalIndex is now: {lastUsedPortalIndex}");
        }
        else
        {
            Debug.LogWarning($"[PORTAL MANAGER] Invalid portal index: {portalIndex} (must be 0, 1, or 2)");
        }
    }

    /// <summary>
    /// Load the last used portal index from save data, or use default
    /// </summary>
    public void LoadLastUsedPortalIndex(int savedIndex)
    {
        if (savedIndex >= 0 && savedIndex < 3)
        {
            lastUsedPortalIndex = savedIndex;
            // Also update default to match (so future uses use this as default)
            defaultPortalIndex = savedIndex;
            Debug.Log($"[PORTAL MANAGER] Loaded portal index {savedIndex} from save file. Updated default to {defaultPortalIndex}");
        }
        else
        {
            // No valid save, use default (which may have been updated from save file)
            lastUsedPortalIndex = defaultPortalIndex;
            Debug.Log($"[PORTAL MANAGER] No valid save file, using default portal index {defaultPortalIndex}");
        }
    }

    /// <summary>
    /// Get the portal the player should return through in MainScene
    /// </summary>
    public Portal GetReturnPortal()
    {
        // If no portal index set, use default (first time playing)
        int portalIndexToUse = (lastUsedPortalIndex >= 0) ? lastUsedPortalIndex : defaultPortalIndex;

        if (portalIndexToUse >= 0 && portalIndexToUse < mainScenePortals.Length)
        {
            return mainScenePortals[portalIndexToUse];
        }

        // Fallback: try to find portal with index 0
        if (mainScenePortals[0] != null)
        {
            return mainScenePortals[0];
        }

        return null;
    }

    /// <summary>
    /// Teleport player back to the portal they entered from (or default portal)
    /// </summary>
    public void ReturnPlayerToPortal(GameObject player)
    {
        if (player == null)
        {
            Debug.LogWarning("[PORTAL MANAGER] Player GameObject is null!");
            return;
        }

        Debug.Log($"[PORTAL MANAGER] ReturnPlayerToPortal called. Last used index: {lastUsedPortalIndex}, Default: {defaultPortalIndex}");

        // Make sure portals are found
        if (mainScenePortals[0] == null || mainScenePortals[1] == null || mainScenePortals[2] == null)
        {
            Debug.Log("[PORTAL MANAGER] Portals not found, searching...");
            FindMainScenePortals();
        }

        // Ensure portals are active before using them
        EnsurePortalsAreActive();

        Portal returnPortal = GetReturnPortal();
        if (returnPortal != null)
        {
            // Get XR Origin (the actual player object that moves)
            Transform playerTransform = player.transform;
            Vector3 currentPos = playerTransform.position;

            // Base portal position/rotation
            Vector3 portalPosition = returnPortal.transform.position;
            Quaternion portalRotation = returnPortal.transform.rotation;

            // Rotate player so they look to the LEFT relative to the portal's forward
            // (i.e. turn 90 degrees counter-clockwise around Y)
            Quaternion playerRotation = portalRotation * Quaternion.Euler(0f, -90f, 0f);

            // Use a custom spawn point if defined on the Portal, otherwise a default offset in front
            Vector3 spawnPosition;
            if (returnPortal.spawnPoint != null)
            {
                spawnPosition = returnPortal.spawnPoint.position;
            }
            else
            {
                // Default: 1.5 units in front of the portal
                Vector3 offset = returnPortal.transform.forward * 1.5f;
                spawnPosition = portalPosition + offset;
            }

            Debug.Log($"[PORTAL MANAGER] Teleporting player from {currentPos} to {spawnPosition} (portal at {portalPosition})");

            // Disable Character Controller temporarily to allow position change
            UnityEngine.CharacterController charController = player.GetComponent<UnityEngine.CharacterController>();
            bool wasEnabled = false;
            if (charController != null)
            {
                wasEnabled = charController.enabled;
                charController.enabled = false;
                Debug.Log("[PORTAL MANAGER] Disabled Character Controller");
            }

            // Position the player at portal location (with offset)
            // Direct position setting works for both XR and non-XR setups
            playerTransform.position = spawnPosition;
            playerTransform.rotation = playerRotation;

            Debug.Log($"[PORTAL MANAGER] Player position set to: {spawnPosition}, rotation: {playerRotation.eulerAngles}");

            // Re-enable Character Controller
            if (charController != null && wasEnabled)
            {
                charController.enabled = true;
                Debug.Log("[PORTAL MANAGER] Re-enabled Character Controller");
            }

            // Set position again after a tiny delay to ensure it sticks
            StartCoroutine(ReinforcePosition(player, spawnPosition, portalRotation));

            int portalIndexUsed = (lastUsedPortalIndex >= 0) ? lastUsedPortalIndex : defaultPortalIndex;
            Debug.Log($"[PORTAL MANAGER] ✓ Player returned to Portal {portalIndexUsed} at position {portalPosition}. Current position: {playerTransform.position}");

            // Verify position was set correctly - check against spawn position (with offset)
            if (Vector3.Distance(playerTransform.position, spawnPosition) > 0.5f)
            {
                Debug.LogWarning($"[PORTAL MANAGER] Position didn't stick! Expected: {spawnPosition}, Actual: {playerTransform.position}. Trying again...");
                // Try multiple times with delays
                StartCoroutine(ForcePositionAfterDelay(player, spawnPosition, portalRotation, charController, wasEnabled));
            }
            else
            {
                Debug.Log($"[PORTAL MANAGER] ✓ Position set successfully: {playerTransform.position}");
            }
        }
        else
        {
            Debug.LogWarning("[PORTAL MANAGER] Could not find return portal! Trying fallback...");
            Debug.Log($"[PORTAL MANAGER] Last used index: {lastUsedPortalIndex}, Default: {defaultPortalIndex}");
            Debug.Log($"[PORTAL MANAGER] Portals array: [0]={mainScenePortals[0] != null}, [1]={mainScenePortals[1] != null}, [2]={mainScenePortals[2] != null}");

            // Fallback: try to find any portal with index matching default
            Portal[] allPortals = FindObjectsOfType<Portal>();
            Debug.Log($"[PORTAL MANAGER] Found {allPortals.Length} portals in scene");

            foreach (Portal portal in allPortals)
            {
                Debug.Log($"[PORTAL MANAGER] Portal found with index: {portal.portalIndex}");
                if (portal.portalIndex == defaultPortalIndex || portal.portalIndex == 0)
                {
                    Vector3 fallbackPos = portal.transform.position - portal.transform.forward * 1.5f;
                    UnityEngine.CharacterController charController = player.GetComponent<UnityEngine.CharacterController>();
                    bool wasEnabled = false;
                    if (charController != null)
                    {
                        wasEnabled = charController.enabled;
                        charController.enabled = false;
                    }

                    player.transform.position = fallbackPos;
                    player.transform.rotation = portal.transform.rotation;

                    if (charController != null && wasEnabled)
                    {
                        charController.enabled = true;
                    }

                    Debug.Log($"[PORTAL MANAGER] Player returned to Portal {portal.portalIndex} (fallback) at {fallbackPos}");
                    return;
                }
            }

            Debug.LogError("[PORTAL MANAGER] Could not find ANY portal to return player to!");
        }
    }

    private System.Collections.IEnumerator ReinforcePosition(GameObject player, Vector3 targetPosition, Quaternion targetRotation)
    {
        yield return new WaitForSeconds(0.1f);
        player.transform.position = targetPosition;
        player.transform.rotation = targetRotation;

        yield return new WaitForSeconds(0.2f);
        player.transform.position = targetPosition;

        Debug.Log($"[PORTAL MANAGER] Position reinforced. Final: {player.transform.position}, Target: {targetPosition}");
    }

    private System.Collections.IEnumerator ForcePositionAfterDelay(GameObject player, Vector3 targetPosition, Quaternion targetRotation, UnityEngine.CharacterController charController, bool wasEnabled)
    {
        yield return new WaitForSeconds(0.1f);

        if (charController != null)
        {
            charController.enabled = false;
        }
        player.transform.position = targetPosition;
        player.transform.rotation = targetRotation;

        yield return new WaitForSeconds(0.1f);
        player.transform.position = targetPosition;

        if (charController != null && wasEnabled)
        {
            charController.enabled = true;
        }

        Debug.Log($"[PORTAL MANAGER] Force position complete. Final position: {player.transform.position}");
    }
}


