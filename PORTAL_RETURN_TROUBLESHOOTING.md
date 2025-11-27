# Portal Return Troubleshooting Guide

## Problem: Player Always Spawns at Original Position

If the player always spawns at the starting position instead of the portal, follow this checklist:

## ✅ Setup Checklist

### 1. Portal Setup in MainScene

**For each of the 3 portals (Portal_0, Portal_1, Portal_2):**

- [ ] Portal GameObject exists
- [ ] Has **Box Collider** component
- [ ] Box Collider has **Is Trigger** ✅ checked
- [ ] Has **Portal** script attached
- [ ] Portal Index is set correctly:
  - Portal_0 → `0`
  - Portal_1 → `1`
  - Portal_2 → `2`
- [ ] Portal script has:
  - Main Scene Name = `"MainScene"`
  - Game Manager Scene Name = `"GameManagerScene"`
  - Target Scene Name = `"GameManagerScene"` (or leave empty for auto-detect)

### 2. PortalManager Setup in MainScene

- [ ] PortalManager GameObject exists
- [ ] Has **PortalManager** script
- [ ] **Main Scene Portals** array has all 3 portals assigned:
  - [0] = Portal_0
  - [1] = Portal_1
  - [2] = Portal_2
- [ ] **Default Portal Index** = `0`
- [ ] PortalManager uses **DontDestroyOnLoad** (should be automatic)

### 3. Return Portal in GameManagerScene

- [ ] Return Portal GameObject exists
- [ ] Has **Box Collider** component
- [ ] Box Collider has **Is Trigger** ✅ checked
- [ ] Has **Portal** script attached
- [ ] Portal Index = `-1` (or any value, doesn't matter)
- [ ] Target Scene Name = `"MainScene"` (or leave empty for auto-detect)

### 4. Save System Setup

- [ ] SaveSystem GameObject exists (or will be auto-created)
- [ ] SaveSystem uses **DontDestroyOnLoad** (automatic)
- [ ] Save file is being created (check console for "Game saved successfully")

### 5. GameManager Setup

- [ ] GameManager GameObject exists in MainScene
- [ ] Has **GameManager** script
- [ ] Main Scene Name = `"MainScene"`
- [ ] Game Manager Scene Name = `"GameManagerScene"`
- [ ] GameManager uses **DontDestroyOnLoad** (automatic)

### 6. Player Setup

- [ ] XR Origin has tag **"Player"** (exactly, case-sensitive)
- [ ] XR Origin exists in both scenes

## 🔍 Debug Steps

### Step 1: Check Console Logs

When you enter a portal, you should see:
```
Portal 1 in MainScene - saving index and going to GameManagerScene
Portal index 1 saved to file immediately
```

When you return, you should see:
```
MainScene loaded - checking if returning from GameManagerScene
Loaded portal index 1 from save file
Player returned to Portal 1 at position (x, y, z)
```

### Step 2: Verify Save File

1. Check if save file exists:
   - Windows: `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\gamesave.json`
   - Mac: `~/Library/Application Support/<CompanyName>/<ProductName>/gamesave.json`
   - Linux: `~/.config/unity3d/<CompanyName>/<ProductName>/gamesave.json`

2. Open the save file and check:
   ```json
   {
     "lastUsedPortalIndex": 1,  // Should match the portal you entered
     ...
   }
   ```

### Step 3: Test Each Portal

1. **Test Portal 0:**
   - Enter Portal 0 → Check console for "Portal 0"
   - Save game in GameManagerScene
   - Return → Should spawn at Portal 0

2. **Test Portal 1:**
   - Enter Portal 1 → Check console for "Portal 1"
   - Save game in GameManagerScene
   - Return → Should spawn at Portal 1

3. **Test Portal 2:**
   - Enter Portal 2 → Check console for "Portal 2"
   - Save game in GameManagerScene
   - Return → Should spawn at Portal 2

## 🐛 Common Issues

### Issue 1: Portal Index Not Saved

**Symptoms:** Console shows portal index but save file doesn't have it

**Fix:**
- Make sure SaveSystem exists before entering portal
- Check that portal script can find SaveSystem
- Verify save file path is writable

### Issue 2: Portals Not Found

**Symptoms:** Console shows "Could not find return portal"

**Fix:**
- Make sure all 3 portals are assigned to PortalManager
- Check portal indices are 0, 1, 2 (not other numbers)
- Verify portals are active in scene

### Issue 3: Player Position Overridden

**Symptoms:** Player moves to portal then immediately back to start

**Fix:**
- Check if any other script is resetting player position
- Make sure no spawn point scripts are running
- Verify XR Origin isn't being reset by another system

### Issue 4: Timing Issues

**Symptoms:** Player spawns at start, then teleports to portal

**Fix:**
- The system now waits longer for scene to load
- If still happening, increase delay in `ReturnPlayerToPortalDelayed()`

## 📋 File Structure (No Special Requirements)

**No special file/folder structure needed!** The scripts work from any location:

```
Assets/
├── Scripts/
│   └── Core/
│       ├── Portal.cs
│       ├── PortalManager.cs
│       ├── GameManager.cs
│       └── SaveSystem.cs
```

Just make sure:
- Scripts are in a folder Unity can compile
- Scripts are attached to GameObjects
- Scene names match exactly: "MainScene" and "GameManagerScene"

## ✅ Verification Test

**Complete Test Flow:**

1. Start in MainScene
2. Move to Portal 1
3. Enter Portal 1 → Should see: "Portal 1... saving index"
4. Arrive in GameManagerScene
5. Click Save button → Should see: "Game saved successfully"
6. Enter return portal → Should see: "Returning from GameManagerScene"
7. Arrive in MainScene → Should see: "Player returned to Portal 1"
8. **Verify:** Player is at Portal 1 position, NOT starting position

## 🆘 Still Not Working?

If it's still not working after checking everything:

1. **Check Console:** Look for error messages or warnings
2. **Check Save File:** Verify portal index is actually saved
3. **Check Portal Positions:** Make sure portals are where you think they are
4. **Test with Debug Logs:** All scripts now have detailed logging

**Share these details if asking for help:**
- Console log output
- Save file contents (lastUsedPortalIndex value)
- Portal setup (indices, positions)
- Any error messages

The system should work now with the improvements! 🎉

