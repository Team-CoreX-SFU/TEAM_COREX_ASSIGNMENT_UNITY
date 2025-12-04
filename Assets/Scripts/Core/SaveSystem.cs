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
    /// Collect all saveable data from the current scene (only rope data)
    /// </summary>
    private void CollectSaveData()
    {
        CurrentSaveData = new SaveData();

        // Collect rope / handcuff data
        CollectRopeData();
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
    /// Apply loaded save data to the current scene (only rope data)
    /// </summary>
    public void ApplySaveData()
    {
        if (CurrentSaveData == null)
        {
            Debug.LogWarning("No save data to apply!");
            return;
        }

        // Apply rope / handcuff data
        ApplyRopeData();
    }

    /// <summary>
    /// Apply loaded save data (only rope data)
    /// </summary>
    public void LoadGameWithoutPlayerPosition()
    {
        if (CurrentSaveData == null)
        {
            Debug.LogWarning("No save data to apply!");
            return;
        }

        // Apply rope / handcuff data
        ApplyRopeData();
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

