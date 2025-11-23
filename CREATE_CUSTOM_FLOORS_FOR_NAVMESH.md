# Creating Custom Floor Objects for NavMesh

When imported FBX floors don't work well with NavMesh, create simple floor objects that the kidnapper can walk on.

**💡 See `HANDLING_OBSTACLES_ON_FLOORS.md` for how to handle furniture and obstacles on your floors.**

## Quick Solution: Create Simple Floor Planes

### Step 1: Create Basement Floor

1. In Hierarchy, right-click → **3D Object → Plane**
2. Name it: **"NavMesh_BasementFloor"**
3. Select it
4. In Inspector, set **Transform**:
   - **Position**: 
     - X: 0 (or center of your basement)
     - Y: [Your basement floor height, e.g., -3]
     - Z: 0 (or center of your basement)
   - **Rotation**: X: 0, Y: 0, Z: 0
   - **Scale**: 
     - X: [Width of basement / 10, e.g., 5 for 50 units wide]
     - Y: 1
     - Z: [Length of basement / 10, e.g., 5 for 50 units long]

5. **Mark as Navigation Static:**
   - In Inspector, check **"Navigation Static"** ✅
   - Or find "Static" dropdown → Check **"Navigation Static"**

6. **Make it invisible (optional but recommended):**
   - In Inspector, find **Mesh Renderer** component
   - Uncheck the checkbox to disable it
   - The floor will still work for NavMesh but won't be visible

### Step 2: Create Ground Floor

1. Right-click → **3D Object → Plane**
2. Name it: **"NavMesh_GroundFloor"**
3. Set **Transform**:
   - **Position**: 
     - X: 0
     - Y: [Your ground floor height, usually 0]
     - Z: 0
   - **Scale**: Match your ground floor size

4. Check **"Navigation Static"** ✅
5. Disable **Mesh Renderer** (make invisible)

### Step 3: Create First Floor

1. Right-click → **3D Object → Plane**
2. Name it: **"NavMesh_FirstFloor"**
3. Set **Transform**:
   - **Position**: 
     - X: 0
     - Y: [Your first floor height, e.g., 3]
     - Z: 0
   - **Scale**: Match your first floor size

4. Check **"Navigation Static"** ✅
5. Disable **Mesh Renderer** (make invisible)

### Step 4: Position Floors Correctly

**To find the correct Y positions:**

1. **Method 1: Use Scene View**
   - Select your house in Hierarchy
   - In Scene view, look at where each floor is
   - Note the Y position of each floor
   - Set your NavMesh floor planes to match

2. **Method 2: Measure in Scene**
   - Select your house
   - In Inspector, check the Transform position
   - Add/subtract based on floor heights
   - Example: If house is at Y=0, basement might be Y=-3, first floor Y=3

3. **Method 3: Place and Adjust**
   - Create floors at estimated positions
   - In Scene view, move them up/down to match actual floors
   - Use the Move tool (W key) and drag the Y axis

### Step 5: Scale Floors to Match House Size

**To find the correct scale:**

1. **Method 1: Measure in Scene View**
   - In Scene view, look at your house
   - Estimate the width and length
   - Divide by 10 (Plane is 10x10 units by default)
   - Example: 50 units wide = Scale X: 5

2. **Method 2: Use Gizmos**
   - Select your house
   - Look at its bounds in Scene view
   - Match your floor planes to those bounds

3. **Method 3: Start Large, Then Adjust**
   - Set Scale to 10, 10, 10 (covers 100x100 units)
   - In Scene view, scale down to match your house
   - Use Scale tool (R key) in Scene view

### Step 6: Create Stairs/Ramps Between Floors (If Needed)

If you want the kidnapper to move between floors:

1. **For Stairs:**
   - Create a **Cube** (3D Object → Cube)
   - Name it: **"NavMesh_Stairs_BasementToGround"**
   - Rotate and scale it to match your stairs
   - Position it connecting basement to ground floor
   - Mark as **Navigation Static** ✅
   - Disable Mesh Renderer

2. **For Ramps:**
   - Create a **Cube**
   - Rotate it to create a slope
   - Position it between floors
   - Mark as **Navigation Static** ✅

### Step 7: Organize in Hierarchy

1. Create an empty GameObject: **"NavMesh_Floors"**
2. Drag all floor planes and stairs into it
3. This keeps your Hierarchy organized

### Step 8: Bake NavMesh

1. Create Empty GameObject → Name: **"NavMesh Manager"**
2. Add Component → **Nav Mesh Surface**
3. Configure:
   - **Collect Objects**: **"All"**
   - **Agent Type**: Humanoid
   - **Agent Radius**: 0.5
   - **Agent Height**: 2
   - **Max Slope**: 60 (for stairs)
   - **Step Height**: 0.4 (for stairs)
4. Click **"Bake"**
5. You should see blue NavMesh on all your floor planes!

---

## Alternative: Use ProBuilder or Probuilder Floors

If you want more control over floor shapes:

1. **Install ProBuilder** (if not already):
   - Window → Package Manager
   - Search "ProBuilder"
   - Install it

2. **Create Custom Floors:**
   - Tools → ProBuilder → ProBuilder Window
   - Create custom shapes matching your floors
   - Mark as Navigation Static

---

## Tips for Better NavMesh

### Floor Positioning:
- Place floors **slightly above** the actual floor (Y + 0.01)
- This prevents the agent from clipping through
- Or place them **exactly at** floor level

### Floor Size:
- Make floors **slightly larger** than your actual floors
- This ensures full coverage
- Example: If floor is 20x20, make NavMesh floor 22x22

### Multiple Rooms:
- You can create **separate planes for each room**
- All marked as Navigation Static
- NavMesh will connect them if they're close enough

### Doorways:
- Make sure doorways are **at least 1 unit wide**
- Agent radius is 0.5, so needs 1 unit clearance
- If doorways are too narrow, agent can't pass

---

## Troubleshooting

### Problem: NavMesh Not Appearing on Floors

**Solutions:**
- Make sure floors are marked as **Navigation Static** ✅
- Check that floors are on included layers
- Verify NavMesh Surface "Collect Objects" is set to "All"
- Make sure floors are within the NavMesh Surface bounds

### Problem: Agent Falls Through Floors

**Solutions:**
- Check floor Y positions match actual floor heights
- Make sure floors are large enough
- Verify NavMesh was baked successfully (blue overlay visible)
- Check that agent is on NavMesh (Console will warn if not)

### Problem: Agent Can't Reach Certain Areas

**Solutions:**
- Check doorways are wide enough (at least 1 unit)
- Verify all rooms have floor planes
- Make sure floors connect (no gaps)
- Check Agent Radius isn't too large

### Problem: Floors Look Wrong/Visible

**Solutions:**
- Disable Mesh Renderer on floor objects
- Or create a transparent material
- Floors only need to exist for NavMesh, not visuals

---

## Quick Checklist

- [ ] Created floor planes for all 3 floors
- [ ] Positioned floors at correct Y heights
- [ ] Scaled floors to match house size
- [ ] Marked all floors as Navigation Static
- [ ] Disabled Mesh Renderer (made invisible)
- [ ] Created stairs/ramps if needed
- [ ] Created NavMesh Surface
- [ ] Baked NavMesh
- [ ] Verified blue overlay on all floors
- [ ] Tested kidnapper movement on each floor

---

## Example Setup

Here's an example for a typical 3-story house:

```
NavMesh_Floors (Empty GameObject)
├── NavMesh_BasementFloor
│   Position: (0, -3, 0)
│   Scale: (5, 1, 5)  // 50x50 units
│   Navigation Static: ✅
│   Mesh Renderer: ❌ (disabled)
│
├── NavMesh_GroundFloor
│   Position: (0, 0, 0)
│   Scale: (5, 1, 5)
│   Navigation Static: ✅
│   Mesh Renderer: ❌
│
├── NavMesh_FirstFloor
│   Position: (0, 3, 0)
│   Scale: (5, 1, 5)
│   Navigation Static: ✅
│   Mesh Renderer: ❌
│
└── NavMesh_Stairs_GroundToFirst
    Position: (0, 1.5, 0)  // Middle of stairs
    Rotation: (0, 0, 30)   // Angled for stairs
    Scale: (1, 0.2, 3)     // Stair dimensions
    Navigation Static: ✅
    Mesh Renderer: ❌
```

---

## Final Steps

1. **Test Each Floor:**
   - Place the kidnapper on each floor
   - Set patrol points on each floor
   - Verify movement works

2. **Test Between Floors:**
   - If you created stairs, test navigation between floors
   - Place patrol points on different floors
   - See if kidnapper can reach them

3. **Optimize:**
   - Remove any unnecessary floor objects
   - Adjust sizes if needed
   - Fine-tune positions

Your custom floors should now work perfectly with NavMesh! 🎮

