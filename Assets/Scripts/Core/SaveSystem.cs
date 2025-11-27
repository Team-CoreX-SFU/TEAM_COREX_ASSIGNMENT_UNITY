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
        if (portalManager != null && CurrentSaveData.lastUsedPortalIndex >= 0)
        {
            portalManager.SetLastUsedPortal(CurrentSaveData.lastUsedPortalIndex);
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
}

