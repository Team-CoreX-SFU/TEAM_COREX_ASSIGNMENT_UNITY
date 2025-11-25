using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable data structure to hold all game state information
/// </summary>
[Serializable]
public class SaveData
{
    // Portal system
    public int lastUsedPortalIndex = -1; // Which portal the player entered from (0, 1, or 2)
    public Vector3[] portalPositions = new Vector3[3]; // Positions of all 3 portals

    // Player data
    public Vector3 playerPosition;
    public Vector3 playerRotation;

    // Flashlight data
    public bool flashlightHasBattery;
    public bool flashlightIsOn;
    public bool flashlightIsGrabbed;
    public Vector3 flashlightPosition;
    public Vector3 flashlightRotation;

    // Battery pack data
    public bool batteryPickedUp;
    public Vector3 batteryPosition;
    public bool batteryActive; // Whether battery still exists in scene

    // Keypad data
    public string keypadPin = "";
    public bool keypadUnlocked;

    // Enemy (Kidnapper) data - supports up to 3 enemies
    public List<EnemySaveData> enemyData = new List<EnemySaveData>();

    // Radio data
    public Vector3 radioPosition;
    public Vector3 radioRotation;
    public bool radioActive;

    // Timestamp
    public string saveTime;
}

/// <summary>
/// Data structure for individual enemy/kidnapper state
/// </summary>
[Serializable]
public class EnemySaveData
{
    public Vector3 position;
    public Vector3 rotation;
    public string state; // "Patrolling", "Chasing", "Attacking", "Searching"
    public int currentPatrolIndex;
    public Vector3 lastKnownPlayerPosition;
    public bool playerDetected;
}

