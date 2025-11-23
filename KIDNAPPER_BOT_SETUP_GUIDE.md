# Kidnapper Bot Setup Guide

This guide will walk you through setting up kidnapper bots in your escape room game using the NPC Casual Set 00 model.

## Step 1: Download Animations from Mixamo

Since the NPC model doesn't include animations, you'll need to download them from **Mixamo** (free):

1. Go to [https://www.mixamo.com](https://www.mixamo.com)
2. Sign up for a free Adobe account if you don't have one
3. Search for and download these animations:
   - **Idle**: "Idle" or "Standing Idle"
   - **Walking**: "Walking" or "Walking Forward"
   - **Running**: "Running" or "Running Forward"
   - **Attack**: "Punching" or "Kicking" (choose based on your preference)
   - **Searching**: "Looking Around" or "Idle Looking Around"

4. **Important Settings when downloading:**
   - Format: **FBX for Unity**
   - Skin: **With Skin** (if available)
   - Frame Rate: **30 fps**
   - Keyframe Reduction: **None** (for better quality)

## Step 2: Import Animations into Unity

1. Create a new folder: `Assets/Animations/Kidnapper/`
2. Drag and drop all downloaded FBX animation files into this folder
3. Unity will automatically import them

## Step 3: Configure Animation Import Settings

For each animation file:

1. Select the animation file in the Project window
2. In the Inspector, go to the **Animation** tab
3. Check **Import Animation**
4. Set **Bake Animations** to ON
5. Set **Root Transform Rotation** to "Bake Into Pose"
6. Set **Root Transform Position (Y)** to "Bake Into Pose"
7. Set **Root Transform Position (XZ)** to "Bake Into Pose"
8. Click **Apply**

## Step 4: Set Up the NPC Character Model

1. **Locate the NPC Model:**
   - Navigate to `Assets/npc_casual_set_00/`
   - Find the main character prefab or FBX file
   - If you see multiple parts, look for a complete character or assemble one

2. **Import Settings for the Model:**
   - Select the character model
   - In the Inspector, go to the **Rig** tab
   - Set **Animation Type** to **Humanoid**
   - Click **Apply**
   - Unity will try to map the bones automatically

3. **Verify the Rig:**
   - Go to the **Rig** tab
   - Click **Configure...** to open the Avatar Configuration
   - Check if all bones are mapped correctly (should be green)
   - If some are red, you may need to manually map them
   - Click **Done**

## Step 5: Create an Animator Controller

1. Right-click in `Assets/Animations/Kidnapper/`
2. Create → **Animator Controller**
3. Name it `KidnapperAnimatorController`
4. Double-click to open it in the Animator window

5. **Add Animation States:**
   - Right-click in the Animator window → Create State → Empty
   - Name them:
     - `Idle` (set as default - orange)
     - `Walk`
     - `Run`
     - `Attack`
     - `Search`

6. **Assign Animations to States:**
   - Select each state
   - In the Inspector, drag the corresponding animation from your project into the **Motion** field

7. **Create Transitions:**
   - Click on a state and drag to another state to create a transition
   - Set up transitions:
     - **Idle → Walk**: Condition: `Speed > 0.1`
     - **Walk → Idle**: Condition: `Speed < 0.1`
     - **Walk → Run**: Condition: `Speed > 3`
     - **Run → Walk**: Condition: `Speed < 3`
     - **Any State → Attack**: Condition: `Attack` (trigger)
     - **Attack → Idle**: Exit Time: 0.9 (after attack animation finishes)

8. **Add Parameters:**
   - In the Animator window, click **Parameters** tab
   - Add:
     - `Speed` (Float)
     - `Attack` (Trigger)
     - `Idle` (Bool)

## Step 6: Create the Kidnapper Prefab

1. **Create Empty GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Name it `Kidnapper`

2. **Add the Character Model:**
   - Drag your NPC model as a child of the Kidnapper GameObject
   - Position it at (0, 0, 0) relative to parent

3. **Add Required Components:**
   - Select the Kidnapper GameObject
   - Add Component → **Nav Mesh Agent**
   - Add Component → **KidnapperAI** (script)
   - Add Component → **Capsule Collider** (for physics)
     - Set Height: 2
     - Set Radius: 0.5
     - Set Center: (0, 1, 0)

4. **Add Animator:**
   - Select the character model child object
   - Add Component → **Animator**
   - Drag your `KidnapperAnimatorController` into the **Controller** field
   - Set **Avatar** to the Avatar from your character model

5. **Configure KidnapperAI Script:**
   - Select the Kidnapper GameObject
   - In the KidnapperAI component:
     - Drag the Animator component into the **Animator** field
     - Set **Patrol Speed**: 2
     - Set **Chase Speed**: 4
     - Set **Detection Range**: 10
     - Set **Attack Range**: 2
     - Set **Field Of View**: 90

6. **Set Up Patrol Points:**
   - Create empty GameObjects where you want the kidnapper to patrol
   - Name them "PatrolPoint1", "PatrolPoint2", etc.
   - Position them around your scene
   - Select the Kidnapper
   - In KidnapperAI, set **Patrol Points** array size to match number of points
   - Drag each patrol point into the array

7. **Set Up Layers:**
   - Go to Edit → Project Settings → Tags and Layers
   - Create a new layer called "Enemy" (if not exists)
   - Select the Kidnapper GameObject
   - Set its Layer to "Enemy"
   - In KidnapperAI, set **Player Layer** to include the "Default" layer (where Player is)

## Step 7: Set Up NavMesh

1. **Mark Walkable Surfaces:**
   - Select all floor/ground objects in your scene
   - In Inspector, check **Navigation Static**

2. **Bake NavMesh:**
   - Go to Window → AI → Navigation
   - Click the **Bake** tab
   - Click **Bake** button
   - Unity will generate a blue NavMesh overlay

3. **Verify NavMesh:**
   - The kidnapper should be able to pathfind on the blue areas
   - Adjust **Agent Radius** and **Agent Height** if needed

## Step 8: Test the Kidnapper

1. **Enter Play Mode**
2. The kidnapper should:
   - Start patrolling between patrol points
   - Detect the player when in range and line of sight
   - Chase the player when detected
   - Attack when close enough
   - Search for the player if they escape

## Step 9: Fine-Tuning

### Adjust Detection:
- Increase **Detection Range** for more aggressive AI
- Adjust **Field Of View** to change detection angle
- Modify **Obstacle Layer** to include walls/objects that block line of sight

### Adjust Movement:
- Change **Patrol Speed** and **Chase Speed** to match your game feel
- Adjust **Patrol Wait Time** to change how long they wait at each point

### Adjust Animations:
- Make sure animation parameter names match in the Animator Controller
- Adjust transition conditions for smoother animation blending

## Troubleshooting

### Animations Not Playing:
- Check that the Animator Controller is assigned
- Verify animation parameter names match the script
- Ensure the Avatar is properly configured

### NavMesh Agent Not Moving:
- Verify NavMesh is baked
- Check that patrol points are on the NavMesh
- Ensure the agent's height and radius fit through doorways

### Player Not Detected:
- Verify Player has the "Player" tag
- Check that Player Layer is set correctly in KidnapperAI
- Ensure there are no obstacles blocking line of sight
- Adjust Detection Range and Field of View

### Character Model Issues:
- If the model doesn't have a proper rig, you may need to use a different model
- Try using Unity's built-in character models for testing first

## Additional Features (Optional)

You can extend the KidnapperAI script to add:
- Multiple kidnappers working together
- Sound effects (footsteps, grunts, etc.)
- Visual indicators (detection cone, alert icons)
- Different AI behaviors (stationary guard, roaming guard)
- Integration with game over system when player is caught

## References

- [Mixamo](https://www.mixamo.com) - Free character animations
- [Unity NavMesh Documentation](https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html)
- [Unity Animator Controller](https://docs.unity3d.com/Manual/class-AnimatorController.html)


