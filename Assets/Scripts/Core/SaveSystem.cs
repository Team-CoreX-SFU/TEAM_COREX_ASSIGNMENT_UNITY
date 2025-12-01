using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Singleton system for saving and loading game data
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private const string SAVE_FILE_NAME = "gamesave.json";
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    public SaveData CurrentSaveData { get; private set; }

    /// <summary>
    /// Get the save file path (for debugging)
    /// </summary>
    public string GetSaveFilePath() => SaveFilePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentSaveData = new SaveData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Auto-load save file if it exists, so other scripts can check ropeCut etc. immediately
        if (SaveFileExists())
        {
            LoadGame();
            Debug.Log("[SAVE SYSTEM] Auto-loaded save file on Start().");
        }
    }

    /// <summary>
    /// Save the current game state
    /// </summary>
    public bool SaveGame()
    {
        try
        {
            // Collect all saveable data from the scene
            CollectSaveData();

            // Serialize to JSON
            string json = JsonUtility.ToJson(CurrentSaveData, true);

            // Write to file
            File.WriteAllText(SaveFilePath, json);

            Debug.Log($"Game saved successfully to: {SaveFilePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Save ONLY the portal index (quick save when entering portal, no save button needed)
    /// </summary>
    public bool SavePortalIndexOnly(int portalIndex)
    {
        try
        {
            Debug.Log($"[SAVE SYSTEM] SavePortalIndexOnly called with index: {portalIndex}");
            Debug.Log($"[SAVE SYSTEM] Save file path: {SaveFilePath}");
            Debug.Log($"[SAVE SYSTEM] File exists before save: {File.Exists(SaveFilePath)}");

            // Load existing save if it exists, or create new one
            if (File.Exists(SaveFilePath))
            {
                string existingJson = File.ReadAllText(SaveFilePath);
                CurrentSaveData = JsonUtility.FromJson<SaveData>(existingJson);
                Debug.Log($"[SAVE SYSTEM] Loaded existing save. Previous portal index: {CurrentSaveData.lastUsedPortalIndex}");
            }
            else
            {
                CurrentSaveData = new SaveData();
                Debug.Log("[SAVE SYSTEM] Creating new save data");
            }

            // Update just the portal index
            int oldIndex = CurrentSaveData.lastUsedPortalIndex;
            CurrentSaveData.lastUsedPortalIndex = portalIndex;
            CurrentSaveData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Save to file
            string json = JsonUtility.ToJson(CurrentSaveData, true);
            File.WriteAllText(SaveFilePath, json);

            // Verify it was written
            if (File.Exists(SaveFilePath))
            {
                string verifyJson = File.ReadAllText(SaveFilePath);
                SaveData verifyData = JsonUtility.FromJson<SaveData>(verifyJson);
                Debug.Log($"[SAVE SYSTEM] ✓ Portal index saved: {oldIndex} → {portalIndex}. Verified in file: {verifyData.lastUsedPortalIndex}");
                Debug.Log($"[SAVE SYSTEM] Save file location: {SaveFilePath}");
                return true;
            }
            else
            {
                Debug.LogError($"[SAVE SYSTEM] ✗ File was not created! Path: {SaveFilePath}");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SAVE SYSTEM] Failed to save portal index: {e.Message}\nStack: {e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Load the saved game state
    /// </summary>
    public bool LoadGame()
    {
        try
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning("No save file found!");
                return false;
            }

            // Read from file
            string json = File.ReadAllText(SaveFilePath);

            // Deserialize
            CurrentSaveData = JsonUtility.FromJson<SaveData>(json);

            Debug.Log($"Game loaded successfully from: {SaveFilePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if a save file exists
    /// </summary>
    public bool SaveFileExists()
    {
        return File.Exists(SaveFilePath);
    }

    /// <summary>
    /// Delete the save file
    /// </summary>
    public bool DeleteSave()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                CurrentSaveData = new SaveData();
                Debug.Log("Save file deleted successfully");
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Collect all saveable data from the current scene
    /// </summary>
    private void CollectSaveData()
    {
        CurrentSaveData = new SaveData();
        CurrentSaveData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Collect player data
        CollectPlayerData();

        // Collect portal data
        CollectPortalData();

        // Collect flashlight data
        CollectFlashlightData();

        // Collect battery data
        CollectBatteryData();

        // Collect keypad data
        CollectKeypadData();

        // Collect enemy data
        CollectEnemyData();

        // Collect radio data
        CollectRadioData();

        // Collect rope / handcuff data
        CollectRopeData();
    }

    private void CollectPlayerData()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Transform playerTransform = player.transform;
            // Try to find camera offset or main camera
            Transform cameraOffset = playerTransform.Find("Camera Offset");
            if (cameraOffset != null)
            {
                Transform mainCamera = cameraOffset.Find("Main Camera");
                if (mainCamera != null)
                {
                    CurrentSaveData.playerPosition = mainCamera.position;
                    CurrentSaveData.playerRotation = mainCamera.eulerAngles;
                }
                else
                {
                    CurrentSaveData.playerPosition = cameraOffset.position;
                    CurrentSaveData.playerRotation = cameraOffset.eulerAngles;
                }
            }
            else
            {
                CurrentSaveData.playerPosition = playerTransform.position;
                CurrentSaveData.playerRotation = playerTransform.eulerAngles;
            }
        }
    }

    private void CollectPortalData()
    {
        Portal[] portals = FindObjectsOfType<Portal>();
        if (portals != null && portals.Length > 0)
        {
            // Sort portals by index to ensure consistent ordering
            System.Array.Sort(portals, (a, b) => a.portalIndex.CompareTo(b.portalIndex));

            for (int i = 0; i < Mathf.Min(portals.Length, 3); i++)
            {
                if (portals[i] != null)
                {
                    CurrentSaveData.portalPositions[i] = portals[i].transform.position;
                }
            }

            // Get the last used portal index from PortalManager if it exists
            PortalManager portalManager = FindObjectOfType<PortalManager>();
            if (portalManager != null)
            {
                CurrentSaveData.lastUsedPortalIndex = portalManager.LastUsedPortalIndex;
            }
        }
    }

    private void CollectFlashlightData()
    {
        FlashlightController flashlight = FindObjectOfType<FlashlightController>();
        if (flashlight != null)
        {
            CurrentSaveData.flashlightHasBattery = flashlight.hasBattery;
            CurrentSaveData.flashlightIsOn = flashlight.isOn;
            CurrentSaveData.flashlightIsGrabbed = flashlight.isGrabbed;
            CurrentSaveData.flashlightPosition = flashlight.transform.position;
            CurrentSaveData.flashlightRotation = flashlight.transform.eulerAngles;
        }
    }

    private void CollectBatteryData()
    {
        BatteryPickup[] batteries = FindObjectsOfType<BatteryPickup>();
        if (batteries != null && batteries.Length > 0)
        {
            // Save the first active battery found
            BatteryPickup activeBattery = System.Array.Find(batteries, b => b.gameObject.activeSelf);
            if (activeBattery != null)
            {
                CurrentSaveData.batteryPickedUp = false;
                CurrentSaveData.batteryActive = true;
                CurrentSaveData.batteryPosition = activeBattery.transform.position;
            }
            else
            {
                // All batteries picked up
                CurrentSaveData.batteryPickedUp = true;
                CurrentSaveData.batteryActive = false;
            }
        }
        else
        {
            CurrentSaveData.batteryPickedUp = true;
            CurrentSaveData.batteryActive = false;
        }
    }

    private void CollectKeypadData()
    {
        // Look for keypad component - you'll need to create this or adjust based on your keypad implementation
        KeypadController keypad = FindObjectOfType<KeypadController>();
        if (keypad != null)
        {
            CurrentSaveData.keypadPin = keypad.GetCurrentPin();
            CurrentSaveData.keypadUnlocked = keypad.IsUnlocked();
        }
    }

    private void CollectEnemyData()
    {
        CurrentSaveData.enemyData.Clear();

        KidnapperAI[] enemies = FindObjectsOfType<KidnapperAI>();
        if (enemies != null)
        {
            foreach (KidnapperAI enemy in enemies)
            {
                if (enemy != null)
                {
                    EnemySaveData enemySave = new EnemySaveData
                    {
                        position = enemy.transform.position,
                        rotation = enemy.transform.eulerAngles,
                        state = enemy.currentState.ToString(),
                        currentPatrolIndex = enemy.GetCurrentPatrolIndex(),
                        lastKnownPlayerPosition = enemy.GetLastKnownPlayerPosition(),
                        playerDetected = enemy.IsPlayerDetected()
                    };
                    CurrentSaveData.enemyData.Add(enemySave);
                }
            }
        }
    }

    private void CollectRadioData()
    {
        GameObject radio = GameObject.Find("Radio");
        if (radio == null)
        {
            // Try to find by tag or component
            radio = GameObject.FindGameObjectWithTag("Radio");
        }

        if (radio != null)
        {
            CurrentSaveData.radioPosition = radio.transform.position;
            CurrentSaveData.radioRotation = radio.transform.eulerAngles;
            CurrentSaveData.radioActive = radio.activeSelf;
        }
    }

    /// <summary>
    /// Collects the cut/not-cut state of the rope/handcuff.
    /// Assumes any rope objects are tagged with "Rope".
    /// If no active rope objects are found, we treat it as already cut.
    /// </summary>
    private void CollectRopeData()
    {
        // Try to find any active GameObject(s) tagged as Rope.
        GameObject[] ropes = GameObject.FindGameObjectsWithTag("Rope");
        bool anyActiveRope = false;

        if (ropes != null && ropes.Length > 0)
        {
            foreach (var rope in ropes)
            {
                if (rope != null && rope.activeInHierarchy)
                {
                    anyActiveRope = true;
                    break;
                }
            }
        }

        // If there is at least one active rope, it has not been cut.
        // If there are no active ropes, we consider the rope cut.
        CurrentSaveData.ropeCut = !anyActiveRope;
    }

    /// <summary>
    /// Apply loaded save data to the current scene
    /// </summary>
    public void ApplySaveData()
    {
        if (CurrentSaveData == null)
        {
            Debug.LogWarning("No save data to apply!");
            return;
        }

        // Apply player data
        ApplyPlayerData();

        // Apply portal data (restore last used portal)
        ApplyPortalData();

        // Apply flashlight data
        ApplyFlashlightData();

        // Apply battery data
        ApplyBatteryData();

        // Apply keypad data
        ApplyKeypadData();

        // Apply enemy data
        ApplyEnemyData();

        // Apply radio data
        ApplyRadioData();

        // Apply rope / handcuff data
        ApplyRopeData();
    }

    /// <summary>
    /// Apply loaded save data WITHOUT player position or portal data
    /// (used when returning from portal; portal index is kept in memory only)
    /// </summary>
    public void LoadGameWithoutPlayerPosition()
    {
        if (CurrentSaveData == null)
        {
            Debug.LogWarning("No save data to apply!");
            return;
        }

        // Skip player position AND portal index - portal system uses in-memory state only

        // Apply flashlight data
        ApplyFlashlightData();

        // Apply battery data
        ApplyBatteryData();

        // Apply keypad data
        ApplyKeypadData();

        // Apply enemy data
        ApplyEnemyData();

        // Apply radio data
        ApplyRadioData();

        // Apply rope / handcuff data
        ApplyRopeData();
    }

    private void ApplyPlayerData()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Transform playerTransform = player.transform;
            Transform cameraOffset = playerTransform.Find("Camera Offset");

            if (cameraOffset != null)
            {
                Transform mainCamera = cameraOffset.Find("Main Camera");
                if (mainCamera != null)
                {
                    mainCamera.position = CurrentSaveData.playerPosition;
                    mainCamera.rotation = Quaternion.Euler(CurrentSaveData.playerRotation);
                }
                else
                {
                    cameraOffset.position = CurrentSaveData.playerPosition;
                    cameraOffset.rotation = Quaternion.Euler(CurrentSaveData.playerRotation);
                }
            }
            else
            {
                playerTransform.position = CurrentSaveData.playerPosition;
                playerTransform.rotation = Quaternion.Euler(CurrentSaveData.playerRotation);
            }
        }
    }

    private void ApplyPortalData()
    {
        PortalManager portalManager = FindObjectOfType<PortalManager>();
        if (portalManager != null)
        {
            // Load the saved portal index (or use default if no save)
            if (CurrentSaveData.lastUsedPortalIndex >= 0 && CurrentSaveData.lastUsedPortalIndex < 3)
            {
                portalManager.LoadLastUsedPortalIndex(CurrentSaveData.lastUsedPortalIndex);
            }
            else
            {
                // No valid save, use default portal index (0)
                portalManager.LoadLastUsedPortalIndex(-1); // Will trigger default
            }
        }
    }

    private void ApplyFlashlightData()
    {
        FlashlightController flashlight = FindObjectOfType<FlashlightController>();
        if (flashlight != null)
        {
            flashlight.hasBattery = CurrentSaveData.flashlightHasBattery;
            flashlight.isOn = CurrentSaveData.flashlightIsOn;
            flashlight.isGrabbed = CurrentSaveData.flashlightIsGrabbed;
            flashlight.transform.position = CurrentSaveData.flashlightPosition;
            flashlight.transform.rotation = Quaternion.Euler(CurrentSaveData.flashlightRotation);

            // Update light state
            if (flashlight.flashlightLight != null)
            {
                flashlight.flashlightLight.enabled = CurrentSaveData.flashlightIsOn;
            }
        }
    }

    private void ApplyBatteryData()
    {
        // If battery was picked up, don't restore it
        if (CurrentSaveData.batteryPickedUp || !CurrentSaveData.batteryActive)
        {
            BatteryPickup[] batteries = FindObjectsOfType<BatteryPickup>();
            foreach (var battery in batteries)
            {
                if (battery != null)
                {
                    Destroy(battery.gameObject);
                }
            }
        }
        else
        {
            // Restore battery position if it exists
            BatteryPickup battery = FindObjectOfType<BatteryPickup>();
            if (battery != null)
            {
                battery.transform.position = CurrentSaveData.batteryPosition;
            }
        }
    }

    private void ApplyKeypadData()
    {
        KeypadController keypad = FindObjectOfType<KeypadController>();
        if (keypad != null)
        {
            keypad.SetPin(CurrentSaveData.keypadPin);
            if (CurrentSaveData.keypadUnlocked)
            {
                keypad.Unlock();
            }
        }
    }

    private void ApplyEnemyData()
    {
        KidnapperAI[] enemies = FindObjectsOfType<KidnapperAI>();
        if (enemies != null && CurrentSaveData.enemyData != null)
        {
            for (int i = 0; i < Mathf.Min(enemies.Length, CurrentSaveData.enemyData.Count); i++)
            {
                if (enemies[i] != null && i < CurrentSaveData.enemyData.Count)
                {
                    EnemySaveData enemySave = CurrentSaveData.enemyData[i];
                    enemies[i].transform.position = enemySave.position;
                    enemies[i].transform.rotation = Quaternion.Euler(enemySave.rotation);

                    // Restore AI state
                    if (System.Enum.TryParse<KidnapperAI.AIState>(enemySave.state, out KidnapperAI.AIState state))
                    {
                        enemies[i].currentState = state;
                    }

                    enemies[i].SetCurrentPatrolIndex(enemySave.currentPatrolIndex);
                    enemies[i].SetLastKnownPlayerPosition(enemySave.lastKnownPlayerPosition);
                    enemies[i].SetPlayerDetected(enemySave.playerDetected);
                }
            }
        }
    }

    private void ApplyRadioData()
    {
        GameObject radio = GameObject.Find("Radio");
        if (radio == null)
        {
            radio = GameObject.FindGameObjectWithTag("Radio");
        }

        if (radio != null)
        {
            radio.transform.position = CurrentSaveData.radioPosition;
            radio.transform.rotation = Quaternion.Euler(CurrentSaveData.radioRotation);
            radio.SetActive(CurrentSaveData.radioActive);
        }
    }

    /// <summary>
    /// Apply rope/handcuff cut state.
    /// If the rope has been cut previously, immediately:
    /// - Enable hands (via any HandMovementToggleInspector in the scene)
    /// - Destroy any rope/handcuff objects, regardless of where the player is
    /// This runs every time the scene loads and ropeCut is true.
    /// </summary>
    private void ApplyRopeData()
    {
        if (!CurrentSaveData.ropeCut)
        {
            // Rope has not been cut yet; leave it as placed in the scene.
            return;
        }

        Debug.Log("[SAVE SYSTEM] ropeCut=true, applying global rope cut state on scene load.");

        // 1) Enable hands everywhere (no sound, no delay), independent of trigger zones
        var handToggles = FindObjectsOfType<HandMovementToggleInspector>();
        foreach (var toggle in handToggles)
        {
            if (toggle == null) continue;
            toggle.handsEnabled = true;
            toggle.ApplyHandState();
        }

        // 2) Destroy any rope/handcuff objects we know about, regardless of where they are

        // By tag "Rope"
        GameObject[] ropesByTag = null;
        try
        {
            ropesByTag = GameObject.FindGameObjectsWithTag("Rope");
        }
        catch (UnityException)
        {
            // Tag might not exist; ignore.
        }

        if (ropesByTag != null)
        {
            foreach (var rope in ropesByTag)
            {
                if (rope == null) continue;
                Destroy(rope);
            }
        }

        // By name "RopeHandcuff" (in case tag is not used)
        GameObject ropeByName = GameObject.Find("RopeHandcuff");
        if (ropeByName != null)
        {
            Destroy(ropeByName);
        }

        Debug.Log("[SAVE SYSTEM] Global rope cut applied: hands enabled and rope objects removed.");
    }
}

