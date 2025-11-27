# Build Settings Setup Guide

## Problem
If you see this error:
```
Scene 'GameManagerScene' couldn't be loaded because it has not been added to the build settings
```

This means your scenes need to be added to Unity's Build Settings.

## Solution: Add Scenes to Build Settings

### Step-by-Step Instructions:

1. **Open Build Settings:**
   - Go to **File** → **Build Settings...**
   - Or press `Ctrl+Shift+B` (Windows/Linux) or `Cmd+Shift+B` (Mac)

2. **Add MainScene:**
   - In Unity, open `MainScene` (double-click it in Project window)
   - In Build Settings window, click **"Add Open Scenes"** button
   - MainScene should now appear in the "Scenes In Build" list

3. **Add GameManagerScene:**
   - In Unity, open `GameManagerScene` (double-click it in Project window)
   - In Build Settings window, click **"Add Open Scenes"** button
   - GameManagerScene should now appear in the "Scenes In Build" list

4. **Verify Order:**
   - Make sure both scenes are in the list
   - The order doesn't matter for portal system, but typically:
     - Index 0: MainScene (main game scene)
     - Index 1: GameManagerScene (menu/save scene)

5. **Save:**
   - Close Build Settings window
   - Unity automatically saves the build settings

## Alternative Method (Drag & Drop):

1. Open **Build Settings** (File → Build Settings)
2. Drag scenes from Project window directly into "Scenes In Build" list:
   - Drag `Assets/Scenes/MainScene.unity`
   - Drag `Assets/Scenes/GameManagerScene.unity`

## Verify Setup:

After adding scenes, you should see:
```
Scenes In Build:
[0] MainScene
[1] GameManagerScene
```

## Testing:

1. Play the game
2. Enter a portal in MainScene
3. Should successfully load GameManagerScene (no error)
4. Enter return portal in GameManagerScene
5. Should successfully return to MainScene

## Important Notes:

- ✅ Scenes must be in Build Settings for `SceneManager.LoadScene()` to work
- ✅ The portal script now checks if scenes are in build settings and shows a helpful error
- ✅ You only need to do this once - Unity remembers the build settings
- ✅ If you add new scenes later, remember to add them to Build Settings too

## Troubleshooting:

**Still getting error after adding scenes?**
- ✅ Make sure scene names match exactly: "MainScene" and "GameManagerScene" (case-sensitive)
- ✅ Check that scenes are actually in the list (not just open)
- ✅ Try closing and reopening Build Settings
- ✅ Restart Unity if needed

**Portal script shows error message?**
- The updated Portal script now checks if scenes are in build settings
- It will show a helpful error message if a scene is missing
- Follow the steps above to add the missing scene

That's it! Your scenes should now work with the portal system! 🎉

