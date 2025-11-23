# Multi-Floor NavMesh Setup Guide

Your house has 3 floors (Basement, Ground, First Floor), and the NavMesh needs to cover all walkable areas on each floor.

**⚠️ If your imported FBX floors don't work with NavMesh, see `CREATE_CUSTOM_FLOORS_FOR_NAVMESH.md` for creating simple floor objects.**

## Option 1: Single NavMesh Surface (Easiest - Recommended)

This creates one NavMesh that covers all floors.

### Step 1: Mark All Floor Objects as Navigation Static

1. In Hierarchy, find all floor objects for:
   - Basement floor
   - Ground floor  
   - First floor
2. Select all floor objects (hold Ctrl/Cmd and click each one)
3. In Inspector, check **"Navigation Static"** ✅
4. Click **"Apply"** if prompted

### Step 2: Create NavMesh Surface on Empty GameObject

1. In Hierarchy, right-click → **Create Empty**
2. Name it: **"NavMesh Manager"**
3. Select it
4. In Inspector, click **"Add Component"**
5. Search for: **"Nav Mesh Surface"**
6. Add it

### Step 3: Configure NavMesh Surface

1. In the NavMesh Surface component:
   - **Agent Type**: Humanoid
   - **Collect Objects**: Select **"All"** (this collects all Navigation Static objects)
   - **Include Layers**: Make sure all your floor layers are included
   - **Agent Radius**: 0.5
   - **Agent Height**: 2
   - **Max Slope**: 45
   - **Step Height**: 0.4
   - **Drop Height**: 0 (or higher if you want agents to drop down)
   - **Jump Distance**: 0 (or higher if you want agents to jump)

2. Click **"Bake"** button
3. Wait for Unity to process (may take 30-60 seconds for large scenes)

### Step 4: Verify NavMesh

1. In Scene view, you should see **blue overlay** on all floors
2. If some floors don't have blue:
   - Check that those floor objects are marked as Navigation Static
   - Make sure they're on included layers
   - Try baking again

---

## Option 2: Separate NavMesh Surfaces Per Floor (More Control)

This gives you separate NavMesh areas for each floor, which is better for complex multi-story buildings.

### Step 1: Mark Floor Objects

1. Mark all floor objects as **Navigation Static** (same as Option 1)

### Step 2: Create NavMesh Surface for Basement

1. Create Empty GameObject → Name: **"NavMesh_Basement"**
2. Add **Nav Mesh Surface** component
3. Configure:
   - **Agent Type**: Humanoid
   - **Collect Objects**: **"Volume"**
   - **Size**: Set to cover basement area
     - Position: Center of basement
     - Scale: Size of basement floor
4. Click **"Bake"**

### Step 3: Create NavMesh Surface for Ground Floor

1. Create Empty GameObject → Name: **"NavMesh_Ground"**
2. Add **Nav Mesh Surface** component
3. Configure:
   - **Collect Objects**: **"Volume"**
   - **Size**: Set to cover ground floor area
4. Click **"Bake"**

### Step 4: Create NavMesh Surface for First Floor

1. Create Empty GameObject → Name: **"NavMesh_FirstFloor"**
2. Add **Nav Mesh Surface** component
3. Configure:
   - **Collect Objects**: **"Volume"**
   - **Size**: Set to cover first floor area
4. Click **"Bake"**

### Step 5: Link NavMesh Surfaces (Optional)

If you want agents to move between floors via stairs:

1. Make sure stairs/ramps are marked as Navigation Static
2. Include them in the NavMesh Surface volumes
3. The NavMesh should automatically connect via stairs

---

## Option 3: Use NavMesh Modifiers (For Complex Cases)

If your floors are part of the house FBX and can't be easily separated:

### Step 1: Add NavMesh Modifier to House

1. Select your house GameObject (the main FBX)
2. Add Component → **Nav Mesh Modifier**
3. Configure:
   - **Override Area**: Check ✅
   - **Area Type**: **Walkable**
   - **Affected Objects**: **Children** (to affect all child objects)

### Step 2: Mark Specific Floor Parts

1. If your house has separate floor meshes:
   - Select each floor mesh
   - Check **Navigation Static** ✅
2. If floors are part of one mesh:
   - You might need to use NavMesh Modifier Volume instead
   - Create empty GameObjects to define walkable volumes

---

## Troubleshooting Multi-Floor NavMesh

### Problem: NavMesh Only on One Floor

**Solutions:**
- Make sure all floor objects are marked as Navigation Static
- Check that NavMesh Surface "Collect Objects" is set to "All"
- Verify floor objects are on included layers
- Try increasing NavMesh Surface size/volume

### Problem: NavMesh Not Connecting Between Floors

**Solutions:**
- Make sure stairs/ramps are marked as Navigation Static
- Include stairs in the NavMesh bake
- Check that stairs are within the NavMesh Surface volume
- Increase **Step Height** in NavMesh Surface settings (allows climbing stairs)

### Problem: Agent Can't Reach Certain Areas

**Solutions:**
- Check **Agent Height** - make sure it's 2 (standard human height)
- Check **Agent Radius** - make sure it's 0.5 (not too wide)
- Verify doorways/passages are wide enough (at least 1 unit wide)
- Check **Max Slope** - stairs might need higher value (try 60-90)

### Problem: Blue NavMesh Missing on Some Floors

**Solutions:**
1. **Check Floor Height:**
   - Make sure floors are at correct Y positions
   - Basement might be at Y = -3, Ground at Y = 0, First at Y = 3 (example)

2. **Check NavMesh Surface Position:**
   - NavMesh Surface should be at origin (0,0,0) or cover all floors
   - If using Volume mode, make sure volume covers all floors

3. **Re-bake:**
   - Delete old NavMesh data
   - Re-bake with correct settings

---

## Recommended Settings for Your 3-Floor House

### NavMesh Surface Settings:
- **Agent Type**: Humanoid
- **Collect Objects**: All
- **Agent Radius**: 0.5
- **Agent Height**: 2
- **Max Slope**: 60 (for stairs)
- **Step Height**: 0.4 (for stairs)
- **Drop Height**: 0 (agents won't drop)
- **Jump Distance**: 0 (agents won't jump)

### For Stairs Between Floors:
- Make sure stairs are marked as **Navigation Static**
- **Max Slope** of 60-90 degrees should handle most stairs
- **Step Height** of 0.4 allows climbing standard stairs

---

## Quick Setup Checklist

- [ ] All floor objects marked as Navigation Static
- [ ] NavMesh Surface created and configured
- [ ] NavMesh baked successfully
- [ ] Blue overlay visible on all 3 floors
- [ ] Stairs/ramps included in NavMesh
- [ ] Agent can pathfind on all floors
- [ ] Test kidnapper movement on each floor

---

## Testing Multi-Floor Navigation

1. **Place Kidnapper on Ground Floor:**
   - Set patrol points on ground floor
   - Test patrolling

2. **Place Kidnapper on First Floor:**
   - Set patrol points on first floor
   - Test patrolling

3. **Test Stairs:**
   - Place patrol points on different floors
   - See if kidnapper can navigate stairs
   - If not, check stair settings and Max Slope

4. **Test Chasing:**
   - Player on one floor, kidnapper on another
   - See if kidnapper can chase across floors
   - May need to adjust detection range

---

## Notes

- **FBX Imported Houses:** If your house is one FBX file, you might need to:
  - Mark the entire house as Navigation Static
  - Use NavMesh Modifier to define walkable areas
  - Or separate floor meshes in a 3D editor first

- **Performance:** Large multi-floor NavMeshes can be slow to bake. Be patient!

- **Updates:** If you move floor objects, you need to re-bake the NavMesh

Good luck with your multi-floor escape room! 🏠

