# Complete Beginner's Guide: Adding Kidnapper Bots
## Unity 2022.3.35f1 - Step by Step

This guide assumes you're new to Unity and will walk you through every single step.

---

## PART 1: Getting Animations from Mixamo

### Step 1.1: Create Mixamo Account
1. Open your web browser
2. Go to: **https://www.mixamo.com**
3. Click **"Sign In"** in the top right
4. Sign in with your Adobe account (or create a free one)
5. You should now see the Mixamo homepage

### Step 1.2: Download Idle Animation
1. In the search bar at the top, type: **"Idle"**
2. Click on any idle animation (e.g., "Idle", "Standing Idle", "Idle Breathing")
3. You'll see a 3D character preview
4. Click the **"Download"** button (top right)
5. In the download settings:
   - **Format**: Select **"FBX for Unity (.fbx)"**
   - **Skin**: Select **"With Skin"** (if available)
   - **Frame Rate**: Select **"30 fps"**
   - **Keyframe Reduction**: Select **"None"**
6. Click **"Download"** button
7. Save the file somewhere easy to find (like Desktop)
8. Rename it to: **"Idle.fbx"**

### Step 1.3: Download Walking Animation
1. Search for: **"Walking"**
2. Click on "Walking" or "Walking Forward"
3. Click **"Download"**
4. Use the same settings:
   - Format: **FBX for Unity**
   - Skin: **With Skin**
   - Frame Rate: **30 fps**
   - Keyframe Reduction: **None**
5. Click **"Download"**
6. Rename to: **"Walking.fbx"**

### Step 1.4: Download Running Animation
1. Search for: **"Running"**
2. Click on "Running" or "Running Forward"
3. Download with same settings
4. Rename to: **"Running.fbx"**

### Step 1.5: Download Attack Animation
1. Search for: **"Punching"** or **"Kicking"**
2. Choose one you like
3. Download with same settings
4. Rename to: **"Attack.fbx"**

**You should now have 4 files on your computer:**
- Idle.fbx
- Walking.fbx
- Running.fbx
- Attack.fbx

---

## PART 2: Setting Up Folders in Unity

### Step 2.1: Open Your Unity Project
1. Open Unity Hub
2. Click on your project: **"TEAM_COREX_ASSIGNMENT_UNITY"**
3. Wait for Unity to load (this may take a minute)

### Step 2.2: Create Animation Folder
1. In the **Project** window (bottom of screen), navigate to: `Assets`
2. Right-click on `Assets` folder
3. Select: **Create → Folder**
4. Name it: **"Animations"**
5. Right-click on the new `Animations` folder
6. Select: **Create → Folder**
7. Name it: **"Kidnapper"**

**Your folder structure should look like:**
```
Assets/
  └── Animations/
      └── Kidnapper/
```

---

## PART 3: Importing Animations into Unity

### Step 3.1: Import Animation Files
1. In Unity, go to the **Project** window
2. Navigate to: `Assets/Animations/Kidnapper/`
3. Open File Explorer (Windows) or Finder (Mac)
4. Navigate to where you saved your FBX files
5. **Drag and drop** all 4 FBX files into the `Kidnapper` folder in Unity
6. Wait for Unity to import them (you'll see a progress bar)

### Step 3.2: Configure Idle Animation
1. In Unity Project window, click on **"Idle.fbx"**
2. Look at the **Inspector** window (right side)
3. You'll see tabs at the top: **Model**, **Rig**, **Animation**, **Materials**
4. Click the **"Rig"** tab
5. Find **"Animation Type"** dropdown
6. Change it from "None" to **"Humanoid"**
7. Click the **"Apply"** button at the bottom
8. Wait for Unity to process (may take 10-30 seconds)

### Step 3.3: Configure Other Animations
Repeat Step 3.2 for:
- **Walking.fbx** → Rig tab → Humanoid → Apply
- **Running.fbx** → Rig tab → Humanoid → Apply
- **Attack.fbx** → Rig tab → Humanoid → Apply

### Step 3.4: Configure Animation Import Settings
1. Click on **"Idle.fbx"** again
2. Click the **"Animation"** tab in Inspector
3. Make sure **"Import Animation"** is checked ✅
4. **Note about "Bake Animations":**
   - For **Humanoid** animations, Unity automatically bakes them
   - You might not see "Bake Animations" option, or it might be grayed out
   - **This is normal and okay!** Skip this step if you don't see it
5. Scroll down to find **"Root Transform Rotation"**
   - Set the dropdown to: **"Bake Into Pose"** ✅
6. Find **"Root Transform Position (Y)"**
   - Set the dropdown to: **"Bake Into Pose"** ✅
7. Find **"Root Transform Position (XZ)"**
   - Set the dropdown to: **"Bake Into Pose"** ✅
8. Click **"Apply"** at the bottom of the Inspector
9. Repeat steps 1-8 for all other animation files:
   - **Walking.fbx**
   - **Running.fbx**
   - **Attack.fbx**

**Important:** The Root Transform settings are the most important part - they prevent the character from sliding or floating during animations.

---

## PART 4: Finding Your NPC Model

This asset contains **129 prefabs** with complete characters already assembled! You have two options:

### Option A: Use a Complete Character Prefab (RECOMMENDED - Easiest!)

### Step 4.1: Open Prefabs Folder
1. In Unity Project window, navigate to: `Assets/npc_casual_set_00/Prefabs/`
2. You'll see many prefab files (blue cube icons)

### Step 4.2: Find Complete Character Prefabs
Look for prefabs with names like:
- **`npc_csl_00_character_01f_01`** (Female character, type 1, variant 1)
- **`npc_csl_00_character_01f_02`** (Female character, type 1, variant 2)
- **`npc_csl_00_character_01m_01`** (Male character, type 1, variant 1)
- **`npc_csl_00_character_01m_02`** (Male character, type 1, variant 2)
- **`npc_csl_00_character_02f_01`** (Female character, type 2, variant 1)
- **`npc_csl_00_character_02m_01`** (Male character, type 2, variant 1)

**Naming explanation:**
- `01f` = Female body type 1
- `01m` = Male body type 1
- `02f` = Female body type 2
- `02m` = Male body type 2
- The last number (01, 02, 03) = Body variant (athletic, overweight, underweight)

### Step 4.3: Choose a Character
1. **Pick any complete character prefab** (one that starts with `npc_csl_00_character_`)
2. For a kidnapper, a **male character** might work better (e.g., `npc_csl_00_character_01m_01`)
3. **Remember the name** - you'll use this prefab in Part 7

**✅ You're done with Part 4 if using a prefab!** Skip to Part 5.

---

### Option B: Use Complete Character FBX File (Alternative)

If you prefer to use the raw model file instead of a prefab:

### Step 4.4: Open Mesh Folder
1. Navigate to: `Assets/npc_casual_set_00/Mesh/`
2. Look for complete character files:
   - **`npc_csl_00_character_01f.fbx`** (Female, type 1)
   - **`npc_csl_00_character_01m.fbx`** (Male, type 1)
   - **`npc_csl_00_character_02f.fbx`** (Female, type 2)
   - **`npc_csl_00_character_02m.fbx`** (Male, type 2)

### Step 4.5: Choose a Character FBX
1. Pick one of the complete character FBX files
2. **Remember the name** - you'll use this in Part 7

**Note:** The prefabs are easier because they're already set up with materials. The FBX files are the raw models.

---

### Step 4.6: Preview Characters (Optional)
1. Navigate to: `Assets/npc_casual_set_00/Scenes/`
2. Double-click **`npc_casual_set_00.unity`** to open the demo scene
3. This shows all the characters - you can see which one you like
4. **Don't save this scene** - just look and close it
5. Go back to your MainScene

---

## PART 5: Setting Up the Character Model

**Important:** Prefabs don't have a Rig tab - you need to configure the **source FBX file** that the prefab is based on. The good news is the asset is already rigged, so this might already be done!

### Step 5.1: Find the Source FBX File

**If you're using a prefab (e.g., `npc_csl_00_character_01m_01.prefab`):**
1. The prefab is based on an FBX file in the `Mesh/` folder
2. Go to: `Assets/npc_casual_set_00/Mesh/`
3. Find the matching FBX file:
   - If prefab is `npc_csl_00_character_01m_01` → Look for `npc_csl_00_character_01m.fbx`
   - If prefab is `npc_csl_00_character_01f_01` → Look for `npc_csl_00_character_01f.fbx`
   - If prefab is `npc_csl_00_character_02m_01` → Look for `npc_csl_00_character_02m.fbx`
   - If prefab is `npc_csl_00_character_02f_01` → Look for `npc_csl_00_character_02f.fbx`

**If you're using an FBX file directly:**
- You already have the FBX file, so use that!

### Step 5.2: Configure Model Rig
1. In Project window, click on the **FBX file** (not the prefab!)
   - Example: `npc_csl_00_character_01m.fbx`
2. Look at the **Inspector** window (right side)
3. You should see tabs at the top: **Model**, **Rig**, **Animation**, **Materials**
4. Click the **"Rig"** tab
5. Check what **"Animation Type"** is set to:
   - **If it says "Humanoid"** ✅: 
     - Great! It's already configured. Skip to Step 5.3
   - **If it says "None" or "Generic"**:
     - Change the dropdown to **"Humanoid"**
     - Click **"Apply"** button at the bottom
     - Wait for Unity to process (10-30 seconds - you'll see a progress bar)

### Step 5.3: Verify Avatar (Optional but Recommended)
1. Still in the **"Rig"** tab (on the FBX file)
2. Look for **"Avatar Definition"** - it should say **"Create From This Model"**
3. You should see an **"Avatar"** field that might be filled in
4. (Optional) Click the **"Configure..."** button if you want to see the skeleton
   - A new window opens showing the character skeleton
   - Check if bones are mapped (should see green lines connecting bones)
   - If everything looks good, click **"Done"** button
   - If you see red or missing bones, that's okay - Unity will try to auto-map them
5. If there's no "Configure" button or you don't want to check, that's fine too

**Note:** The asset readme says "Rigging: Yes", so the characters should already be rigged! If the Animation Type is already "Humanoid", you're all set! ✅

**Important:** After configuring the FBX file, the prefab will automatically use these settings. You don't need to reconfigure the prefab itself.

---

## PART 6: Creating the Animator Controller

### Step 6.1: Create Animator Controller
1. In Project window, go to: `Assets/Animations/Kidnapper/`
2. Right-click in empty space
3. Select: **Create → Animator Controller**
4. Name it: **"KidnapperAnimatorController"**
5. You should see a new file with an orange icon

### Step 6.2: Open Animator Window
1. Double-click on **"KidnapperAnimatorController"**
2. The **Animator** window should open (this is a special window)
3. If it doesn't open automatically:
   - Go to: **Window → Animation → Animator**

### Step 6.3: Create Idle State
1. In the Animator window, right-click on empty space
2. Select: **Create State → Empty**
3. A new state appears (orange box)
4. Click on it to select it
5. In the **Inspector** (right side), find **"Motion"** field
6. Click the circle icon next to "Motion"
7. Select **"Idle"** from the list
8. The state should now show "Idle" as its name
9. Right-click on the state → **Set as Layer Default State**
   - It should turn orange (default state)

### Step 6.4: Create Walk State
1. Right-click in Animator window → **Create State → Empty**
2. Click on the new state
3. In Inspector, set **Motion** to **"Walking"**
4. Rename it to "Walk" (double-click the state name)

### Step 6.5: Create Run State
1. Right-click → **Create State → Empty**
2. Set **Motion** to **"Running"**
3. Rename to "Run"

### Step 6.6: Create Attack State
1. Right-click → **Create State → Empty**
2. Set **Motion** to **"Attack"**
3. Rename to "Attack"

### Step 6.7: Add Parameters
1. In Animator window, look for **"Parameters"** tab (top left)
2. Click the **"+"** button
3. Select **"Float"**
4. Name it: **"Speed"**
5. Click **"+"** again → **"Trigger"**
6. Name it: **"Attack"**
7. Click **"+"** again → **"Bool"**
8. Name it: **"Idle"**

### Step 6.8: Create Transitions
1. **Idle to Walk:**
   - Click on **"Idle"** state
   - Drag from Idle to **"Walk"** state (creates an arrow)
   - Click on the arrow (transition)
   - In Inspector, find **"Conditions"**
   - Click **"+"** to add condition
   - Set to: **Speed** | **Greater** | **0.1**
   - Uncheck **"Has Exit Time"**

2. **Walk to Idle:**
   - Click on **"Walk"** state
   - Drag to **"Idle"** state
   - Click the arrow
   - Add condition: **Speed** | **Less** | **0.1**
   - Uncheck **"Has Exit Time"**

3. **Walk to Run:**
   - Click on **"Walk"** state
   - Drag to **"Run"** state
   - Add condition: **Speed** | **Greater** | **3**
   - Uncheck **"Has Exit Time"**

4. **Run to Walk:**
   - Click on **"Run"** state
   - Drag to **"Walk"** state
   - Add condition: **Speed** | **Less** | **3**
   - Uncheck **"Has Exit Time"**

5. **Any State to Attack:**
   - Right-click on **"Any State"** (orange box at top)
   - Select **"Make Transition"**
   - Click on **"Attack"** state
   - Click the arrow
   - Add condition: **Attack** (trigger)
   - Uncheck **"Has Exit Time"**

6. **Attack to Idle:**
   - Click on **"Attack"** state
   - Drag to **"Idle"** state
   - Click the arrow
   - Check **"Has Exit Time"**
   - Set **"Exit Time"** to **0.9** (attacks for 90% of animation)
   - Set **"Transition Duration"** to **0.1**

**Your Animator Controller should now have:**
- 4 states: Idle (default), Walk, Run, Attack
- Transitions between them
- 3 parameters: Speed, Attack, Idle

---

## PART 7: Creating the Kidnapper GameObject

### Step 7.1: Create Empty GameObject
1. In the **Hierarchy** window (left side), right-click
2. Select: **Create Empty**
3. Name it: **"Kidnapper"**
4. Make sure it's selected (highlighted)

### Step 7.2: Add Character Model

**If you're using a PREFAB (recommended):**
1. In **Project** window, find your character prefab (e.g., `npc_csl_00_character_01m_01.prefab`)
2. **Drag** the prefab into the **Hierarchy** window
3. Make sure it's a **child** of "Kidnapper" (indented under it)
   - If it's not, drag it onto "Kidnapper" in Hierarchy
4. Select the character prefab (child object)
5. In **Inspector**, set its **Position** to: X=0, Y=0, Z=0
6. Set **Rotation** to: X=0, Y=0, Z=0
7. **Note:** The prefab might have multiple child objects (body, clothes, etc.) - that's normal!

**If you're using an FBX file:**
1. In **Project** window, find your character FBX (e.g., `npc_csl_00_character_01m.fbx`)
2. **Drag** the FBX into the **Hierarchy** window
3. Make sure it's a **child** of "Kidnapper"
4. Select the character model (child object)
5. In **Inspector**, set **Position** to: X=0, Y=0, Z=0
6. Set **Rotation** to: X=0, Y=0, Z=0

### Step 7.3: Add Animator Component

**Important:** Prefabs might have multiple child objects. You need to find the main body/character object.

1. In Hierarchy, expand the character prefab (click the arrow next to it)
2. Look for the main body object - it's usually:
   - The object with the character's name (e.g., `npc_csl_00_character_01m`)
   - Or the object that has a **Skinned Mesh Renderer** component
   - Or the largest/root object of the character
3. **Select that main body object** (not clothing or accessories)
4. In Inspector, check if it already has an **Animator** component:
   - **If YES** ✅: Just configure it (go to step 5)
   - **If NO**: Click **"Add Component"** → Type **"Animator"** → Click it
5. In the Animator component:
   - **Controller**: Drag **"KidnapperAnimatorController"** from Project window into this field
   - **Avatar**: 
     - It might auto-fill ✅ (if so, you're done!)
     - If empty, click the circle icon and select the Avatar from your character model
     - Or drag the Avatar from the character's Rig settings in Project window

**Tip:** If you're not sure which object is the main body, select each child object and look in Inspector for one that has "Skinned Mesh Renderer" - that's usually the main character body.

### Step 7.4: Add NavMesh Agent
1. Select the **"Kidnapper"** GameObject (parent, not child)
2. Click **"Add Component"**
3. Type: **"Nav Mesh Agent"**
4. Click on it
5. In Inspector, configure:
   - **Speed**: 2
   - **Angular Speed**: 120
   - **Acceleration**: 8
   - **Stopping Distance**: 0.5
   - **Height**: 2
   - **Radius**: 0.5

### Step 7.5: Add Capsule Collider
1. Still on **"Kidnapper"** GameObject
2. Click **"Add Component"**
3. Type: **"Capsule Collider"**
4. Configure:
   - **Height**: 2
   - **Radius**: 0.5
   - **Center**: X=0, Y=1, Z=0

### Step 7.6: Add KidnapperAI Script
1. Still on **"Kidnapper"** GameObject (the parent, not the character child)
2. Click **"Add Component"**
3. Type: **"Kidnapper AI"** (or search for "Kidnapper")
4. Click on it
5. In Inspector, configure the KidnapperAI component:
   - **Animator**: 
     - Go to the main character body object (the one you added Animator to in Step 7.3)
     - In Inspector, find the **Animator** component
     - **Drag the Animator component** from that object into this field
     - Or click the circle icon and select the object with the Animator
   - **Patrol Speed**: 2
   - **Chase Speed**: 4
   - **Detection Range**: 10
   - **Attack Range**: 2
   - **Field Of View**: 90
   - **Patrol Wait Time**: 2

---

## PART 8: Creating Patrol Points

### Step 8.1: Create First Patrol Point
1. In Hierarchy, right-click
2. Select: **Create Empty**
3. Name it: **"PatrolPoint1"**
4. In **Scene** view, move it to where you want the kidnapper to patrol
   - Select the object
   - Use the **Move tool** (W key) to position it
5. Position it on the floor/ground

### Step 8.2: Create More Patrol Points
1. Create 2-3 more empty GameObjects
2. Name them: **"PatrolPoint2"**, **"PatrolPoint3"**, etc.
3. Position them around your scene where you want patrol routes

### Step 8.3: Assign Patrol Points to AI
1. Select **"Kidnapper"** GameObject
2. In Inspector, find **"Kidnapper AI"** component
3. Find **"Patrol Points"** array
4. Set **"Size"** to the number of patrol points you created (e.g., 3)
5. Drag each patrol point from Hierarchy into the array slots

---

## PART 9: Setting Up NavMesh

### Step 9.1: Mark Floor as Walkable
1. In Hierarchy, select all floor/ground objects
   - Hold **Ctrl** (Windows) or **Cmd** (Mac) and click multiple objects
   - Or select one, then hold Shift and click others
2. In Inspector, look at the top right area
3. Find the **"Static"** checkbox or dropdown
   - If you see a checkbox: Check it ✅
   - If you see a dropdown: Click it and check **"Navigation Static"** ✅
4. Click **"Apply"** if a dialog appears

**Alternative method if you don't see Static:**
1. Select floor objects
2. In Inspector, look for **"Navigation"** section
3. Check **"Navigation Static"** ✅

### Step 9.2: Open Navigation Window

**Try these methods in order:**

**Method 1: Standard Navigation Window**
1. Go to: **Window → AI → Navigation**
   - If you see this option, use it! ✅

**Method 2: Alternative Menu Path**
1. Try: **Window → Navigation** (without "AI")
2. Or: **Edit → Project Settings → Navigation** (then look for Bake button)

**Method 3: Using AI Navigation Package (Unity 2022.3)**
1. Go to: **Window → AI → Navigation → NavMesh Surface**
2. Or: **Window → AI → Navigation → NavMesh Builder**
3. If you see "NavMesh Surface" component option, that's the new system

**Method 4: If None of the Above Work**
1. Go to: **Window → Package Manager**
2. Search for: **"AI Navigation"**
3. Make sure it's installed (should show "Installed")
4. If not installed, click **"Install"**
5. After installation, try Method 1 again

### Step 9.3: Bake NavMesh

**USE THIS METHOD (NavMesh Surface - Works in Unity 2022.3.35f1):**

1. **Select your floor/ground object** in Hierarchy
   - This is the main floor object where the kidnapper should walk
   - If you have multiple floor pieces, select the main one or create an empty GameObject as a parent

2. **Add NavMesh Surface Component:**
   - In Inspector, click **"Add Component"** button
   - Type: **"Nav Mesh Surface"** (with spaces) or **"NavMeshSurface"** (no spaces)
   - Click on **"Nav Mesh Surface"** when it appears
   - The component should now be added to your floor object

3. **Configure NavMesh Surface:**
   - In the NavMesh Surface component, you'll see:
     - **Agent Type**: Leave as **"Humanoid"** (or default)
     - **Collect Objects**: Usually set to **"All"** or **"Volume"**
     - **Include Layers**: Make sure your floor layer is included
   - **Most settings can stay as default!**

4. **Bake the NavMesh:**
   - Scroll down in the NavMesh Surface component
   - Find the **"Bake"** button (it's a big button, usually at the bottom)
   - Click **"Bake"** button
   - Wait for Unity to process (10-30 seconds - you'll see a progress bar)

5. **Verify NavMesh:**
   - After baking, look at your **Scene** view
   - You should see a **blue overlay** on walkable surfaces
   - This is the NavMesh - the kidnapper can only walk on blue areas
   - If you don't see blue:
     - Make sure **Gizmos** are enabled in Scene view
     - Look for **"Gizmos"** dropdown in Scene view toolbar
     - Make sure **"NavMesh"** is checked ✅

**Alternative: If you can't find "Nav Mesh Surface" component:**
1. Try searching for: **"NavMesh"** (one word)
2. Or: **"AI Navigation"**
3. If still nothing, the package might not be fully installed:
   - Go to **Window → Package Manager**
   - Search for **"AI Navigation"**
   - Make sure it's installed (should show version number)
   - If not installed, click **"Install"**

### Step 9.4: Verify NavMesh
1. In **Scene** view, look for a **blue overlay** on your floor
   - This indicates the NavMesh is baked
2. Make sure your patrol points are positioned on the blue NavMesh areas
3. If there's no blue area:
   - Check that floor objects are marked as Navigation Static
   - Try baking again
   - Make sure the floor is at Y=0 or appropriate height

### Step 9.5: Enable NavMesh Visualization (If Needed)
1. In **Scene** view, look at the top toolbar
2. Find the **"Gizmos"** dropdown (usually shows "Gizmos" text)
3. Click it
4. Make sure **"NavMesh"** is checked ✅
5. This will show the blue NavMesh overlay in Scene view

---

## PART 10: Final Configuration

### Step 10.1: Verify Player Tag
1. In Hierarchy, find your **"XR Origin (XR Rig)"** or player object
2. Select it
3. In Inspector, check the **"Tag"** dropdown (top)
4. Make sure it's set to **"Player"**
5. If not:
   - Click the Tag dropdown
   - Select **"Player"**
   - If "Player" doesn't exist:
     - Click **"Add Tag..."**
     - Click **"+"** button
     - Name it: **"Player"**
     - Click **"Save"**
     - Go back and select "Player" tag

### Step 10.2: Set Up Layers (Optional but Recommended)
1. Go to: **Edit → Project Settings**
2. Click **"Tags and Layers"** in left sidebar
3. Find **"Layers"** section
4. Find an empty **User Layer** (e.g., Layer 8)
5. Name it: **"Enemy"**
6. Close Project Settings
7. Select **"Kidnapper"** GameObject
8. In Inspector, find **"Layer"** dropdown
9. Select **"Enemy"**

### Step 10.3: Configure KidnapperAI Player Detection
1. Select **"Kidnapper"** GameObject
2. In **Kidnapper AI** component
3. Find **"Player Layer"** dropdown
4. Make sure it includes **"Default"** layer (where Player is)
5. Find **"Obstacle Layer"** dropdown
6. Set it to include walls/obstacles (usually "Default")

---

## PART 11: Testing

### Step 11.1: Enter Play Mode
1. Click the **Play** button (top center, triangle icon)
2. Unity enters Play Mode

### Step 11.2: Observe Kidnapper Behavior
1. The kidnapper should:
   - Start moving between patrol points
   - Walk slowly (patrol speed)
   - When you get close, detect you
   - Chase you when detected
   - Attack when very close

### Step 11.3: Test Detection
1. Move your player close to the kidnapper
2. The kidnapper should detect and chase you
3. If not:
   - Check Detection Range is large enough
   - Make sure Player has "Player" tag
   - Check that you're in front of the kidnapper (field of view)

### Step 11.4: Exit Play Mode
1. Click **Play** button again to stop
2. Any changes made in Play Mode will be reset

---

## PART 12: Troubleshooting

### Problem: Can't Find Navigation Window
**Solutions:**
- Try: **Window → Navigation** (without "AI")
- Try: **Window → AI → Navigation → NavMesh Surface**
- Check Package Manager: **Window → Package Manager** → Search "AI Navigation" → Make sure it's installed
- Alternative: Add **NavMesh Surface** component directly to floor object (Add Component → NavMesh Surface → Bake)

### Problem: Animations Not Looping
**Solutions:**
1. **In Animation Import Settings:**
   - Select your animation file (e.g., `Walking.fbx`) in Project window
   - Click **"Animation"** tab in Inspector
   - Find **"Loop Time"** checkbox
   - Check **"Loop Time"** ✅
   - Click **"Apply"**
   - Repeat for all animations (Idle, Walking, Running)

2. **In Animator Controller:**
   - Open your `KidnapperAnimatorController`
   - Select each animation state (Idle, Walk, Run)
   - In Inspector, find **"Motion"** field
   - Click the animation clip name
   - In the animation clip settings, make sure **"Loop"** is checked ✅
   - Repeat for all states

### Problem: Kidnapper Not Moving Between Patrol Points
**Solutions:**
- Check that patrol points are assigned in KidnapperAI component (array should have 2+ points)
- Verify patrol points are on the NavMesh (blue area)
- Check Console for errors (Window → General → Console)
- Make sure NavMesh Agent component is enabled
- Try increasing **Patrol Wait Time** to 0.1 (very short wait)
- Check that patrol points are not at the same position
- In Scene view, select Kidnapper and check if you see blue lines connecting patrol points (Gizmos)

### Problem: Kidnapper Not Chasing Player
**Solutions:**
- Verify Player has **"Player"** tag (select Player object → Inspector → Tag dropdown)
- Check **Detection Range** is large enough (try 15 or 20)
- Make sure you're in front of kidnapper (not behind - field of view matters)
- Check **Field Of View** is wide enough (try 120)
- Make sure there are no walls/obstacles blocking line of sight
- Check Console for debug messages about player detection
- Verify Player object is active in Hierarchy
- Try moving very close to kidnapper to test detection

### Problem: Kidnapper Not Moving
**Solutions:**
- Check NavMesh is baked (blue overlay in Scene view)
- Verify patrol points are on NavMesh
- Make sure NavMesh Agent component is enabled (checkbox checked)
- Check that patrol points are assigned in KidnapperAI component
- Enable NavMesh visualization: Scene view → Gizmos dropdown → Check "NavMesh"
- Check Console for errors

### Problem: Animations Not Playing
**Solutions:**
- Verify Animator Controller is assigned to Animator component
- Check animation parameter names match (Speed, Attack, Idle)
- Make sure Avatar is assigned in Animator component
- Verify animations are set to Humanoid in Rig settings

### Problem: Player Not Detected
**Solutions:**
- Verify Player has "Player" tag
- Check Detection Range is large enough (try increasing to 15)
- Make sure you're in front of kidnapper (not behind)
- Check Field of View is wide enough (try 120)
- Verify Player Layer is set correctly in KidnapperAI

### Problem: Kidnapper Falls Through Floor
**Solutions:**
- Add Rigidbody component to Kidnapper
- Set Rigidbody to **"Is Kinematic"** ✅
- Or ensure NavMesh Agent is properly configured

### Problem: Character Model Looks Wrong
**Solutions:**
- Check model import settings (Rig tab → Humanoid)
- Verify materials/textures are assigned
- Make sure model scale is correct (1, 1, 1)

---

## PART 13: Fine-Tuning

### Make Kidnapper More Aggressive:
1. Select Kidnapper
2. In KidnapperAI component:
   - **Detection Range**: 10 → **15**
   - **Field Of View**: 90 → **120**
   - **Chase Speed**: 4 → **6**

### Make Kidnapper Less Aggressive:
1. Select Kidnapper
2. In KidnapperAI component:
   - **Detection Range**: 10 → **5**
   - **Field Of View**: 90 → **60**
   - **Patrol Speed**: 2 → **1.5**

### Adjust Animation Speed:
1. Select animation file (e.g., Walking.fbx)
2. Click **"Animation"** tab
3. Find **"Sample Rate"** or adjust animation speed in Animator Controller

---

## Summary Checklist

Before testing, make sure you have:
- [ ] Downloaded 4 animations from Mixamo (Idle, Walking, Running, Attack)
- [ ] Imported animations and set to Humanoid
- [ ] Created Animator Controller with 4 states
- [ ] Set up transitions and parameters
- [ ] Created Kidnapper GameObject with:
  - [ ] Character model as child
  - [ ] Animator component (on child)
  - [ ] NavMesh Agent (on parent)
  - [ ] Capsule Collider (on parent)
  - [ ] KidnapperAI script (on parent)
- [ ] Created 2-3 patrol points
- [ ] Assigned patrol points to KidnapperAI
- [ ] Marked floor as Navigation Static
- [ ] Baked NavMesh
- [ ] Verified Player has "Player" tag
- [ ] Tested in Play Mode

---

## Need More Help?

- Check the **KIDNAPPER_QUICK_START.md** for quick reference
- See **KIDNAPPER_BOT_SETUP_GUIDE.md** for detailed explanations
- Unity Documentation: https://docs.unity3d.com/2022.3/Documentation/Manual/
- Mixamo Help: https://helpx.adobe.com/mixamo/using/mixamo.html

Good luck with your escape room game! 🎮

