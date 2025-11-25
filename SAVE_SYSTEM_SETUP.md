# Save System Setup Guide

This guide explains how to set up and use the game save/load system.

## Overview

The save system allows you to save and load game state including:
- Player location
- Portal locations and which portal was used
- Flashlight state (battery, on/off, grabbed)
- Battery pack state
- Keypad PIN and unlock state
- Enemy (Kidnapper) locations and states (up to 3 enemies)
- Radio position

## Setup Instructions

### 1. Core Scripts Setup

The following scripts have been created in `Assets/Scripts/Core/`:
- `SaveData.cs` - Data structure for save data
- `SaveSystem.cs` - Handles save/load operations
- `GameManager.cs` - Main game manager with save button integration
- `Portal.cs` - Portal script for scene transitions
- `PortalManager.cs` - Manages portal state
- `KeypadController.cs` - Keypad system (if you don't have one)
- `SaveButtonHandler.cs` - Helper for save button setup

### 2. Setting Up Portals in MainScene

1. **Create 3 Portal GameObjects** in MainScene:
   - Create empty GameObjects named "Portal_0", "Portal_1", "Portal_2"
   - Position them where you want portals to appear
   - Add a Collider (SphereCollider recommended) with `Is Trigger = true`
   - Add the `Portal` script to each
   - Set `portalIndex` to 0, 1, and 2 respectively
   - Set `targetSceneName` to "GameManagerScene"

2. **Create PortalManager**:
   - Create an empty GameObject named "PortalManager"
   - Add the `PortalManager` script
   - Assign the 3 portal GameObjects to the `portals` array (or let it auto-find)

### 3. Setting Up Save Button in GameManagerScene

**Option A: Using SaveButtonHandler (Recommended)**
1. Find your "SaveFileButton" or "SaveFilew" GameObject in GameManagerScene
2. Add the `SaveButtonHandler` script to it
3. The button will automatically connect to GameManager

**Option B: Manual Setup**
1. Find your "SaveFileButton" GameObject
2. In the Button component, add a new OnClick event
3. Drag the GameManager GameObject to the object field
4. Select `GameManager > SaveGame()` from the dropdown

### 4. Setting Up GameManager

1. **In MainScene:**
   - Create an empty GameObject named "GameManager"
   - Add the `GameManager` script
   - Set `mainSceneName` = "MainScene"
   - Set `gameManagerSceneName` = "GameManagerScene"

2. **In GameManagerScene:**
   - Create an empty GameObject named "GameManager" (or use the same one if using DontDestroyOnLoad)
   - Add the `GameManager` script
   - Assign the SaveFileButton to `saveFileButton` field (or let it auto-find)

### 5. Setting Up SaveSystem

The SaveSystem will be created automatically by GameManager, but you can also:
1. Create an empty GameObject named "SaveSystem"
2. Add the `SaveSystem` script
3. It will persist across scenes automatically

### 6. Keypad Setup (if needed)

If you don't have a keypad system yet:
1. Create a GameObject for your keypad
2. Add the `KeypadController` script
3. Set the `correctPin` value
4. Connect your keypad UI buttons to call `AddDigit()` method

If you already have a keypad:
- Make sure it implements:
  - `GetCurrentPin()` method returning string
  - `IsUnlocked()` method returning bool
  - `SetPin(string pin)` method
  - `Unlock()` method

### 7. Radio Setup

Make sure your Radio GameObject:
- Is named "Radio" OR
- Has the tag "Radio"

The save system will automatically find and save/load its position.

## How It Works

### Saving Game

1. Player enters a portal in MainScene → goes to GameManagerScene
2. Player clicks "SaveFileButton" in GameManagerScene
3. `GameManager.SaveGame()` is called
4. `SaveSystem` collects all game data
5. Data is saved to JSON file in `Application.persistentDataPath`

### Loading Game

1. When returning to MainScene from GameManagerScene
2. `GameManager` automatically loads save data if it exists
3. `SaveSystem.ApplySaveData()` restores all game state
4. Player is teleported back to the portal they entered from

### Portal System

- When player enters Portal 0, 1, or 2, the portal index is saved
- When returning to MainScene, player spawns at the same portal they entered from
- This ensures seamless transitions between scenes

## Save File Location

Save files are stored at:
- **Windows:** `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\gamesave.json`
- **Mac:** `~/Library/Application Support/<CompanyName>/<ProductName>/gamesave.json`
- **Linux:** `~/.config/unity3d/<CompanyName>/<ProductName>/gamesave.json`

## Testing

1. **Test Save:**
   - Enter MainScene
   - Move player, pick up battery, turn on flashlight
   - Enter a portal → go to GameManagerScene
   - Click Save button
   - Check console for "Game saved successfully!"

2. **Test Load:**
   - After saving, return to MainScene
   - Game state should be restored
   - Player should be at the portal they entered from

3. **Test Portal Tracking:**
   - Enter Portal 0 → Save → Return → Should spawn at Portal 0
   - Enter Portal 1 → Save → Return → Should spawn at Portal 1
   - Enter Portal 2 → Save → Return → Should spawn at Portal 2

## Troubleshooting

### Save button doesn't work
- Check that `SaveButtonHandler` is attached to the button
- Or manually connect button to `GameManager.SaveGame()`
- Check console for errors

### Player doesn't return to correct portal
- Make sure PortalManager exists in MainScene
- Verify portals have correct `portalIndex` (0, 1, 2)
- Check that portals are assigned to PortalManager

### Save data not loading
- Check console for errors
- Verify save file exists at the path shown in console
- Make sure GameManager exists in MainScene

### Enemy positions not saving
- Make sure all KidnapperAI scripts are active
- Check that enemies have the `KidnapperAI` component
- Verify no more than 3 enemies (system supports up to 3)

## Customization

### Adding More Save Data

To add more data to save:
1. Add fields to `SaveData.cs`
2. Add collection logic in `SaveSystem.CollectSaveData()`
3. Add application logic in `SaveSystem.ApplySaveData()`

### Changing Save File Location

Modify `SaveSystem.SaveFilePath` property to change where saves are stored.

## Notes

- Save files are in JSON format (human-readable)
- Save system supports up to 3 enemies
- Portal system requires exactly 3 portals
- All saveable objects must exist in the scene when saving/loading

