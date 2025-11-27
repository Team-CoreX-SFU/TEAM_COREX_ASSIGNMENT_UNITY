# XR Origin Setup for GameManagerScene

## Quick Answer: Yes, You Need It!

You need to copy the XR Origin setup to GameManagerScene so VR controls work there.

## Option 1: Copy/Paste Setup (Recommended)

### Step-by-Step:

1. **In MainScene:**
   - Find "XR Origin (XR Rig)" in Hierarchy
   - Right-click → **Copy** (or Ctrl+C)

2. **Switch to GameManagerScene:**
   - Open GameManagerScene
   - In Hierarchy, right-click → **Paste** (or Ctrl+V)
   - You should see "XR Origin (XR Rig)" appear

3. **Also Copy XR Interaction Manager:**
   - In MainScene, find "XR Interaction Manager"
   - Copy it
   - Paste it in GameManagerScene

4. **Position the XR Origin:**
   - Select "XR Origin (XR Rig)" in GameManagerScene
   - Position it where you want player to spawn
   - Adjust rotation if needed

5. **Verify Setup:**
   - Make sure XR Origin has tag "Player" (should be set already)
   - Check that all components are intact

### What Gets Copied:

- ✅ XR Origin (XR Rig) - Main VR player object
- ✅ Camera Offset → Main Camera
- ✅ Character Controller
- ✅ Locomotion System components
- ✅ Input Action components
- ✅ XR Interaction Manager (separate object)

## Option 2: Use Prefab (Better for Reusability)

If you want to reuse the setup:

1. **Create Prefab:**
   - In MainScene, select "XR Origin (XR Rig)"
   - Drag it to Project window to create a prefab
   - Name it "XR Origin Setup"

2. **Use in GameManagerScene:**
   - Open GameManagerScene
   - Drag the prefab from Project to Hierarchy
   - Position it where needed

3. **Do the same for XR Interaction Manager:**
   - Create prefab of "XR Interaction Manager"
   - Add it to GameManagerScene

## What You Need to Copy:

### Essential Objects:
1. **XR Origin (XR Rig)**
   - Contains: Camera, Locomotion, Character Controller
   - Tag: "Player" (important for portals!)

2. **XR Interaction Manager**
   - Handles XR interactions
   - Usually at root level

### Optional (if you have them):
- Any custom locomotion scripts
- Input Action assets (usually shared, but verify)
- Hand movement toggle scripts

## Important Notes:

- ✅ **Tag must be "Player"** - Portals check for this tag!
- ✅ **Position matters** - Where player spawns in GameManagerScene
- ✅ **XR Interaction Manager** - Needed for grabbing/interactions
- ✅ **Input Actions** - Should work automatically if same asset

## Testing:

1. Play GameManagerScene
2. You should be able to:
   - Move around (locomotion)
   - Look around (head tracking)
   - Interact with objects (if any)
   - Click the Save button

## Troubleshooting:

**Player can't move in GameManagerScene:**
- ✅ Check: XR Origin is in scene
- ✅ Check: Locomotion components are enabled
- ✅ Check: Input Actions are assigned

**Portal doesn't detect player:**
- ✅ Check: XR Origin has tag "Player"
- ✅ Check: Tag is exactly "Player" (case-sensitive)

**Interactions don't work:**
- ✅ Check: XR Interaction Manager exists in scene
- ✅ Check: Same Input Action asset is assigned

## Quick Checklist:

- [ ] XR Origin (XR Rig) copied to GameManagerScene
- [ ] XR Interaction Manager copied to GameManagerScene
- [ ] XR Origin has tag "Player"
- [ ] XR Origin positioned where you want player to spawn
- [ ] Tested movement in GameManagerScene
- [ ] Tested portal return to MainScene

That's it! Your VR setup should work in both scenes now! 🎉

