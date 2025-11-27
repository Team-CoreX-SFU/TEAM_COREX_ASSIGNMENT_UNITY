# Portal System Setup Guide

## Overview

The portal system now supports bidirectional travel:
- **MainScene**: Has 3 portals (index 0, 1, 2) → All lead to GameManagerScene
- **GameManagerScene**: Has 1 portal → Returns to MainScene at the portal player entered from
- **Save System**: Remembers which portal was used, defaults to portal 0 if no save file exists

## Setup Instructions

### MainScene Setup (3 Portals)

1. **Create 3 Portal GameObjects:**
   - Create 3 cubes (or any GameObjects) named: "Portal_0", "Portal_1", "Portal_2"
   - Position them where you want portals to appear

2. **For each portal (0, 1, 2):**
   - Add **Box Collider** component
   - Check **Is Trigger** ✅
   - Add **Portal** script
   - Set **Portal Index**:
     - Portal_0 → `0`
     - Portal_1 → `1`
     - Portal_2 → `2`
   - **Target Scene Name** will auto-detect (leave empty or set to "GameManagerScene")
   - **Main Scene Name** = "MainScene"
   - **Game Manager Scene Name** = "GameManagerScene"

3. **Create PortalManager:**
   - Create empty GameObject named "PortalManager"
   - Add **PortalManager** script
   - Assign the 3 portals to **Main Scene Portals** array (or let it auto-find)
   - Set **Default Portal Index** = `0` (used when no save file exists)

### GameManagerScene Setup (1 Return Portal)

1. **Create 1 Portal GameObject:**
   - Create a cube (or any GameObject) named "ReturnPortal"
   - Position it where you want the return portal

2. **Setup the return portal:**
   - Add **Box Collider** component
   - Check **Is Trigger** ✅
   - Add **Portal** script
   - Set **Portal Index** = `-1` (or any value, doesn't matter for return portal)
   - **Target Scene Name** will auto-detect (leave empty or set to "MainScene")
   - **Main Scene Name** = "MainScene"
   - **Game Manager Scene Name** = "GameManagerScene"

## How It Works

### Going to GameManagerScene:
1. Player enters Portal 0, 1, or 2 in MainScene
2. PortalManager saves which portal index was used
3. Scene switches to GameManagerScene
4. Portal index is saved to save file when player clicks Save button

### Returning to MainScene:
1. Player enters the return portal in GameManagerScene
2. Scene switches to MainScene
3. GameManager loads save file (if exists) to get portal index
4. If no save file exists, uses default portal index (0)
5. Player spawns at the portal they entered from (or portal 0 if first time)

## Save File Integration

The portal index is automatically saved when:
- Player clicks "SaveFileButton" in GameManagerScene
- SaveSystem collects portal data from PortalManager

The portal index is automatically loaded when:
- Player returns to MainScene from GameManagerScene
- GameManager calls `LoadGameOnReturn()`
- PortalManager restores the saved portal index

## Default Behavior

- **First time playing** (no save file): Player returns to Portal 0
- **After saving**: Player returns to the portal they entered from
- **If save file corrupted**: Falls back to Portal 0

## Testing

1. **Test Portal 0:**
   - Enter Portal 0 in MainScene → Should go to GameManagerScene
   - Save game
   - Return via return portal → Should spawn at Portal 0

2. **Test Portal 1:**
   - Enter Portal 1 in MainScene → Should go to GameManagerScene
   - Save game
   - Return via return portal → Should spawn at Portal 1

3. **Test Portal 2:**
   - Enter Portal 2 in MainScene → Should go to GameManagerScene
   - Save game
   - Return via return portal → Should spawn at Portal 2

4. **Test Default (No Save):**
   - Delete save file (or first time playing)
   - Enter any portal → Go to GameManagerScene
   - Return via return portal → Should spawn at Portal 0 (default)

## Troubleshooting

### Player doesn't return to correct portal:
- ✅ Check: PortalManager exists in MainScene
- ✅ Check: All 3 portals are assigned to PortalManager
- ✅ Check: Portal indices are 0, 1, 2 (not other numbers)
- ✅ Check: Save file was created (check console for "Game saved successfully")
- ✅ Check: Default Portal Index is set to 0 in PortalManager

### Portal doesn't work:
- ✅ Check: Box Collider has Is Trigger enabled
- ✅ Check: Portal script is attached
- ✅ Check: Player has "Player" tag
- ✅ Check: Scene names are correct ("MainScene", "GameManagerScene")

### Save file not working:
- ✅ Check: SaveSystem exists in scene
- ✅ Check: GameManager exists in scene
- ✅ Check: Save button is connected to GameManager.SaveGame()
- ✅ Check console for save/load messages

## Summary

- **MainScene**: 3 portals (index 0, 1, 2) → All go to GameManagerScene
- **GameManagerScene**: 1 return portal → Returns to MainScene
- **Return location**: Saved portal index (or default 0 if no save)
- **Save system**: Automatically saves/loads portal index

That's it! Your bidirectional portal system is ready! 🎉

