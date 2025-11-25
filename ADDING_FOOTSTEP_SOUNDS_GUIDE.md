# Adding Footstep Sounds to XR Player - Step by Step Guide

This guide will help you add walking and running sound effects to your XR Origin (XR Rig) player.

---

## Part 1: Getting Footstep Sound Files

### Option A: Download Free Sounds

1. Go to **freesound.org** or **zapsplat.com** (free with account)
2. Search for:
   - "footstep" or "footsteps"
   - "walking on wood/concrete/grass" (match your floor type)
   - "running footsteps"
3. Download multiple variations (3-5 different sounds)
4. Save them in a folder on your computer

### Option B: Use Unity Asset Store

1. Open Unity Asset Store
2. Search for "footstep sounds" or "footstep pack"
3. Download free packs
4. Import into your project

### Recommended Sound Types:
- **Walking:** Slow, regular footsteps
- **Running:** Faster, more intense footsteps
- **Multiple variations:** 3-5 different sounds for variety

---

## Part 2: Import Sounds into Unity

### Step 1: Create Sounds Folder

1. In Unity Project window, navigate to `Assets/`
2. Right-click → **Create → Folder**
3. Name it: **"Sounds"**
4. Right-click on `Sounds` → **Create → Folder**
5. Name it: **"Footsteps"**

### Step 2: Import Sound Files

1. Open File Explorer (Windows) or Finder (Mac)
2. Navigate to where you saved your footstep sounds
3. **Drag and drop** all sound files into `Assets/Sounds/Footsteps/` folder
4. Wait for Unity to import them

### Step 3: Configure Sound Import Settings

For each sound file:

1. Click on the sound file in Project window
2. In Inspector, you'll see import settings:
   - **Load Type**: **Decompress On Load** (for instant playback)
   - **Compression Format**: **PCM** (best quality) or **Vorbis** (smaller file)
   - **Sample Rate Setting**: **Preserve Sample Rate**
   - **3D Sound**: Uncheck (unless you want 3D spatial audio)
3. Click **"Apply"**

**Note:** For footstep sounds, **Decompress On Load** is recommended for instant playback without delay.

---

## Part 3: Add Footstep Script to Player

### Step 1: Find XR Origin (XR Rig)

1. In Hierarchy, find **"XR Origin (XR Rig)"** or your player object
2. Select it

### Step 2: Add PlayerFootstepSounds Script

1. In Inspector, click **"Add Component"**
2. Type: **"Player Footstep Sounds"**
3. Click on it when it appears
4. The component should now be added
5. (Optional) If you want footsteps to react to head movement (e.g. for room-scale), enable **Use XR Head Position** in the component.  
   - By default we track the XR Origin (rig) position so rotating in place won't trigger footsteps.  
   - Only toggle this on if you specifically want head-based tracking.

### Step 3: Configure Audio Sources

**Option A: Let Script Create Audio Sources (Easiest)**

1. Leave **Walking Audio Source** and **Running Audio Source** empty (null)
2. The script will create them automatically

**Option B: Create Audio Sources Manually (More Control)**

1. Select XR Origin
2. Click **"Add Component"** → **"Audio Source"**
3. Name it: **"Walking Audio Source"**
4. Configure:
   - **Play On Awake**: Uncheck ❌
   - **Loop**: Uncheck ❌
   - **Volume**: 0.5
   - **Spatial Blend**: 0 (2D sound) or 1 (3D sound)
5. Repeat for running (add another Audio Source)

### Step 4: Assign Sound Clips

1. In **Player Footstep Sounds** component:
2. Find **"Walking Sounds"** array
   - Set **Size** to number of walking sounds you have (e.g., 3)
   - Drag each walking sound from Project window into array slots
3. Find **"Running Sounds"** array
   - Set **Size** to number of running sounds you have (e.g., 3)
   - Drag each running sound from array slots

### Step 5: Configure Settings

In **Player Footstep Sounds** component:

- **Walking Speed Threshold**: 0.1 (minimum speed to play walking sound)
- **Running Speed Threshold**: 2.5 (speed to switch to running sounds)
- **Footstep Interval**: 0.5 (time between footsteps when walking)
- **Running Footstep Interval**: 0.3 (time between footsteps when running)
- **Walking Volume**: 0.5 (adjust as needed)
- **Running Volume**: 0.7 (adjust as needed)
- **Use XR Head Position**: Check ✅ (uses head movement for detection)
- **Use Controller Velocity**: Uncheck ❌ (unless you want controller-based)

---

## Part 4: Test Footstep Sounds

### Step 1: Enter Play Mode

1. Click **Play** button
2. Move around in VR or using your movement system
3. You should hear footstep sounds!

### Step 2: Adjust Settings

If sounds are:
- **Too frequent:** Increase **Footstep Interval** values
- **Too quiet:** Increase **Volume** values
- **Not playing:** Check **Walking Speed Threshold** (might be too high)
- **Always running:** Check **Running Speed Threshold** (might be too low)

---

## Part 5: Fine-Tuning

### Adjust Speed Thresholds

1. **Walking Speed Threshold**: 
   - Lower = plays at slower movement
   - Higher = only plays at faster movement
   - Default: 0.1

2. **Running Speed Threshold**:
   - Lower = switches to running sounds sooner
   - Higher = stays in walking longer
   - Default: 2.5

### Adjust Timing

1. **Footstep Interval** (Walking):
   - Lower = faster footsteps
   - Higher = slower footsteps
   - Default: 0.5 seconds

2. **Running Footstep Interval**:
   - Lower = faster running footsteps
   - Higher = slower running footsteps
   - Default: 0.3 seconds

### Adjust Volume

1. **Walking Volume**: 0.0 to 1.0
   - 0.5 = moderate volume
   - 1.0 = full volume

2. **Running Volume**: Usually slightly louder than walking
   - 0.7 = good default

---

## Part 6: Advanced Options

### Option 1: 3D Spatial Audio

If you want footsteps to sound like they come from the player's position:

1. Select Audio Source components
2. Set **Spatial Blend** to **1** (fully 3D)
3. Adjust **Min Distance** and **Max Distance**
4. Set **Rolloff Mode** to **Logarithmic**

### Option 2: Different Sounds for Different Surfaces

You can extend the script to:
- Detect what surface player is walking on
- Play different sounds for wood, concrete, grass, etc.
- Use raycasting to check floor material

### Option 3: Controller-Based Detection

If head movement doesn't work well:

1. In **Player Footstep Sounds** component
2. Check **"Use Controller Velocity"**
3. The script will use controller movement instead

---

## Troubleshooting

### Problem: No Sounds Playing

**Solutions:**
- Check that sound files are assigned to arrays
- Verify **Walking Speed Threshold** is low enough (try 0.05)
- Make sure Audio Sources are not muted
- Check Console for errors
- Verify you're actually moving (check speed in script)

### Problem: Sounds Too Frequent

**Solutions:**
- Increase **Footstep Interval** (try 0.7 or 0.8)
- Increase **Running Footstep Interval** (try 0.4 or 0.5)

### Problem: Sounds Too Quiet

**Solutions:**
- Increase **Walking Volume** and **Running Volume**
- Check Audio Source volume in Inspector
- Check system volume
- Verify sound files aren't too quiet

### Problem: Always Playing Running Sounds

**Solutions:**
- Increase **Running Speed Threshold** (try 3.0 or 4.0)
- Check your movement speed (might be too fast)

### Problem: Sounds Not Syncing with Movement

**Solutions:**
- Adjust **Footstep Interval** to match your movement speed
- Try different **Speed Threshold** values
- Check if **Use XR Head Position** is enabled

### Problem: Sounds Play When Not Moving

**Solutions:**
- Increase **Walking Speed Threshold** (try 0.2)
- Check if head/player is drifting (VR tracking issue)

---

## Quick Setup Checklist

- [ ] Downloaded/imported footstep sound files
- [ ] Created `Assets/Sounds/Footsteps/` folder
- [ ] Imported sounds and configured import settings
- [ ] Added **Player Footstep Sounds** script to XR Origin
- [ ] Assigned walking sounds to **Walking Sounds** array
- [ ] Assigned running sounds to **Running Sounds** array
- [ ] Configured speed thresholds and intervals
- [ ] Tested in Play Mode
- [ ] Adjusted volume and timing as needed

---

## Example Configuration

```
XR Origin (XR Rig)
└── Player Footstep Sounds Component
    ├── Walking Audio Source: (Auto-created)
    ├── Running Audio Source: (Auto-created)
    ├── Walking Sounds: [footstep1.wav, footstep2.wav, footstep3.wav]
    ├── Running Sounds: [run1.wav, run2.wav, run3.wav]
    ├── Walking Speed Threshold: 0.1
    ├── Running Speed Threshold: 2.5
    ├── Footstep Interval: 0.5
    ├── Running Footstep Interval: 0.3
    ├── Walking Volume: 0.5
    ├── Running Volume: 0.7
    └── Use XR Head Position: ✅
```

---

## Tips

1. **Multiple Sound Variations**: Use 3-5 different footstep sounds for variety
2. **Match Floor Type**: Use sounds that match your floor material (wood, concrete, etc.)
3. **Test in VR**: If possible, test in actual VR for best experience
4. **Adjust for Your Movement**: Different locomotion systems have different speeds
5. **Balance Volume**: Make sure footsteps don't overpower other sounds

Your player should now have realistic footstep sounds! 🎮👣

---

## Kidnapper NPC Footsteps

Need footsteps for the kidnappers too? Use the `KidnapperFootstepSounds` script.

1. **Add the Component**
   - Select each Kidnapper prefab/instance (object with `KidnapperAI` + `NavMeshAgent`).
   - Click `Add Component` → search **Kidnapper Footstep Sounds**.

2. **Assign Audio Clips**
   - Drag NPC step clips into **Walking Clips**.
   - Optional: add heavier / faster clips to **Running Clips** (used while chasing).

3. **Audio Source**
   - Script auto-creates a 3D AudioSource if none is provided (spatialized, min/max distance pre-set).
   - Configure `Min Distance`, `Max Distance`, and `Rolloff Mode` right on the component so footsteps fade quicker.
   - Optional: add a child AudioSource near the feet and assign it to `Footstep Source`.

4. **Speed + Timing**
   - `Walking Speed Threshold` / `Running Speed Threshold` default from `KidnapperAI` patrol/chase speeds.
   - Adjust `Walking Interval` / `Running Interval` to match the animation cadence.
   - `Speed Smoothing` prevents the sounds from stopping when the NavMesh Agent briefly slows down.

5. **Animator Sync**
   - Script reads the Animator `Speed` parameter (same one `KidnapperAI` sets) so footsteps match the blend tree even if the agent velocity is small.

6. **Distance & Audibility**
   - Set `Max Audible Distance` to control how far the player can hear the NPC (set to 0 to disable the check).
   - The script automatically skips playback if the listener (player) is farther than that distance.

7. **Testing Checklist**
   - ✅ Kidnapper moves on NavMesh (agent velocity > 0).
   - ✅ Walking clips assigned.
   - ✅ Console shows no warnings.
   - ✅ You can hear spatialized footsteps while patrolling and chasing.

Now both the player **and** the kidnappers have synced footstep audio! 🎮👣

