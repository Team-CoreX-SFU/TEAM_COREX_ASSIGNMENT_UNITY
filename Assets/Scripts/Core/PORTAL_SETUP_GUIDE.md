# Portal Setup Guide

## Problem Fixed
Portals were disappearing when loading MainScene from GameManagerScene. This has been fixed with automatic portal detection and activation.

## How It Works Now

1. **PortalManager** automatically finds and activates portals when MainScene loads
2. Portals are checked multiple times to ensure they stay active
3. Scene loaded events ensure portals are found even when loading from GameManagerScene

## Setup Steps

### Step 1: Verify Portal GameObjects in MainScene

1. Open **MainScene** in Unity
2. Find all 3 portal GameObjects in the scene hierarchy
3. Each portal should have:
   - A **Portal** component
   - A **Collider** component (set as Trigger)
   - Portal index set to **0**, **1**, or **2** (one for each portal)

### Step 2: Check Portal Component Settings

For each portal in MainScene:

1. Select the portal GameObject
2. In the Inspector, find the **Portal** component
3. Verify:
   - **Portal Index**: Must be 0, 1, or 2 (unique for each portal)
   - **Player Tag**: Should be "Player"
   - **Main Scene Name**: Should be "MainScene"
   - **Game Manager Scene Name**: Should be "GameManagerScene"
   - **Target Scene Name**: Can be empty (auto-detected) or "GameManagerScene"

### Step 3: Verify Portal Colliders

1. Each portal must have a **Collider** component
2. The collider must be set as **Is Trigger = true**
3. The collider should cover the portal area where the player enters

### Step 4: Check PortalManager (Optional)

1. In MainScene, look for a GameObject with **PortalManager** component
2. If it exists, you can manually assign portals in the Inspector:
   - **Main Scene Portals**: Array of 3 Portal references
   - **Default Portal Index**: Usually 0
3. If not assigned, PortalManager will auto-find portals

### Step 5: Verify GameManager Setup

1. In **GameManagerScene**, find the **GameManager** GameObject
2. Verify it has:
   - **GameManager** component
   - **SaveSystem** reference (if using save system)
   - **Main Scene Name**: Should be "MainScene"
   - **Game Manager Scene Name**: Should be "GameManagerScene"

### Step 6: Test the Setup

1. **Test from MainScene**:
   - Start the game from MainScene
   - Walk into any portal
   - Should teleport to GameManagerScene
   - Walk into the return portal in GameManagerScene
   - Should return to MainScene at the same portal you entered from
   - All 3 portals should still be visible

2. **Test from GameManagerScene**:
   - Start the game from GameManagerScene
   - Click button to go to MainScene (or use portal)
   - All 3 portals should be visible in MainScene
   - You should be able to use any portal to go back to GameManagerScene

## Troubleshooting

### Portals Still Disappear

1. **Check Console Logs**:
   - Look for `[PORTAL MANAGER]` messages
   - Should see "Finding portals after scene load..."
   - Should see "Found portals: [0]=True, [1]=True, [2]=True"

2. **Verify Portal Names/Tags**:
   - Portals should be in the scene hierarchy
   - They should not be children of objects that get disabled
   - Portal GameObjects should be active in the scene

3. **Check Portal Indexes**:
   - Each portal must have a unique index: 0, 1, or 2
   - No two portals should have the same index
   - Portals with index -1 or >2 won't be found

4. **Manual Portal Assignment**:
   - If auto-finding fails, manually assign portals:
     - Create/Find PortalManager GameObject in MainScene
     - Drag each portal into the Main Scene Portals array
     - Index 0 → Array[0], Index 1 → Array[1], Index 2 → Array[2]

### Portals Not Working (Can't Enter)

1. **Check Colliders**:
   - Portal must have a Collider
   - Collider must be set as Trigger
   - Collider should be large enough for player to enter

2. **Check Player Tag**:
   - Player GameObject must have tag "Player"
   - Portal's Player Tag setting must match

3. **Check Portal Cooldown**:
   - There's a 2-second cooldown after using a portal
   - Wait 2 seconds before trying again

### Portals Found But Inactive

The system should automatically activate portals, but if they're still inactive:

1. Check if portal GameObjects are disabled in hierarchy
2. Check if parent objects are disabled
3. Check Console for activation warnings

## Code Changes Made

1. **PortalManager.cs**:
   - Added `OnSceneLoaded` event handler
   - Added `FindAndActivatePortalsDelayed` coroutine
   - Enhanced `EnsurePortalsAreActive` to check colliders
   - Added multiple activation checks with delays

2. **GameManager.cs**:
   - Added `EnsurePortalsActiveDelayed` coroutine
   - Calls portal activation after scene loads

## Important Notes

- Portals are automatically found and activated when MainScene loads
- The system checks multiple times to ensure portals stay active
- PortalManager persists across scenes (DontDestroyOnLoad)
- Portals should always be visible in MainScene, regardless of how you got there

