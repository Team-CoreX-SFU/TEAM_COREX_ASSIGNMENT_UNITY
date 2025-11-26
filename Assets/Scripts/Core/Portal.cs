using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Portal script that transports player between MainScene and GameManagerScene
/// - MainScene: Has 3 portals (index 0, 1, 2) that lead to GameManagerScene
/// - GameManagerScene: Has 1 portal that returns to MainScene at the portal player entered from
/// </summary>
public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Unique index for this portal (0, 1, or 2 for MainScene portals, -1 for GameManagerScene return portal)")]
    public int portalIndex = 0;

    [Tooltip("Scene to load when player enters portal. Leave empty to auto-detect based on current scene.")]
    public string targetSceneName = "";

    [Tooltip("Tag of the player object")]
    public string playerTag = "Player";

    [Header("Scene Names")]
    [Tooltip("Name of the main game scene")]
    public string mainSceneName = "MainScene";

    [Tooltip("Name of the game manager scene")]
    public string gameManagerSceneName = "GameManagerScene";

    [Header("Visual / Spawn Settings")]
    public bool showDebugGizmo = true;

    [Tooltip("Optional spawn point transform where the player should appear when returning to this portal")]
    public Transform spawnPoint;

    private string currentSceneName;
    private float lastPortalUseTime = 0f;
    private const float PORTAL_COOLDOWN = 2f; // Prevent re-triggering for 2 seconds

    void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;

        // Auto-detect target scene if not set
        if (string.IsNullOrEmpty(targetSceneName))
        {
            if (currentSceneName == mainSceneName)
            {
                // In MainScene, go to GameManagerScene
                targetSceneName = gameManagerSceneName;
            }
            else if (currentSceneName == gameManagerSceneName)
            {
                // In GameManagerScene, return to MainScene
                targetSceneName = mainSceneName;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Prevent immediate re-triggering (cooldown after scene load)
            if (Time.time - lastPortalUseTime < PORTAL_COOLDOWN)
            {
                Debug.Log($"[PORTAL] Portal {portalIndex} triggered but cooldown active (preventing immediate re-trigger)");
                return;
            }

            Debug.Log($"[PORTAL] Player entered trigger of portal: {gameObject.name}, Portal Index: {portalIndex}");
            lastPortalUseTime = Time.time;
            EnterPortal();
        }
    }

    /// <summary>
    /// Called when player enters the portal
    /// </summary>
    private void EnterPortal()
    {
        // Debug: Log which portal is being entered
        Debug.Log($"[PORTAL DEBUG] EnterPortal called on GameObject: {gameObject.name}, Portal Index: {portalIndex}, Current Scene: {currentSceneName}");

        // Validate that the target scene exists in build settings
        if (!IsSceneInBuildSettings(targetSceneName))
        {
            Debug.LogError($"Scene '{targetSceneName}' is not in Build Settings! " +
                         "Please add it via File > Build Settings > Add Open Scenes");
            return;
        }

        // Get or create PortalManager
        PortalManager portalManager = PortalManager.Instance;
        if (portalManager == null)
        {
            GameObject managerObj = new GameObject("PortalManager");
            portalManager = managerObj.AddComponent<PortalManager>();
        }

        // If in MainScene, save which portal was used
        if (currentSceneName == mainSceneName)
        {
            // Check if portal index is valid for MainScene portals (0, 1, or 2)
            if (portalIndex >= 0 && portalIndex < 3)
            {
                Debug.Log($"[PORTAL DEBUG] MainScene portal detected! Index: {portalIndex}, GameObject: {gameObject.name}");

                portalManager.SetLastUsedPortal(portalIndex);
                portalManager.UpdateDefaultPortalIndex(portalIndex);
                Debug.Log($"[PORTAL] Portal {portalIndex} in MainScene - going to GameManagerScene (no save file used)");
            }
            else
            {
                Debug.LogWarning($"[PORTAL] Portal index {portalIndex} is not valid for MainScene (must be 0, 1, or 2). GameObject: {gameObject.name}");
            }
        }
        // If in GameManagerScene, return to MainScene (portal index will be loaded from save)
        else if (currentSceneName == gameManagerSceneName)
        {
            Debug.Log("[PORTAL] Returning from GameManagerScene to MainScene");
        }
        else
        {
            Debug.LogWarning($"[PORTAL] Unknown scene: {currentSceneName}. Portal index: {portalIndex}");
        }

        // Load the target scene
        Debug.Log($"[PORTAL] Loading scene: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }

    /// <summary>
    /// Check if a scene is in the build settings
    /// </summary>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
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

