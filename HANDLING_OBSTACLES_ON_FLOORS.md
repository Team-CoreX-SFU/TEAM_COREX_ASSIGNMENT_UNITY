# Handling Obstacles on Floors with NavMesh

When you have furniture, walls, or other objects on your floors, NavMesh needs to know about them so the kidnapper can navigate around them.

## How NavMesh Handles Obstacles

NavMesh automatically **excludes** areas covered by objects marked as **Navigation Static**. This creates "holes" in the NavMesh where obstacles are, so agents walk around them.

---

## Method 1: Mark Obstacles as Navigation Static (Recommended)

This tells NavMesh to avoid these objects when baking.

### Step 1: Identify Obstacles

Objects that should block navigation:
- Furniture (tables, chairs, sofas, etc.)
- Walls (interior walls, not outer walls)
- Large objects (cabinets, shelves, etc.)
- Pillars or columns

### Step 2: Mark Obstacles

1. Select all obstacle objects (hold Ctrl/Cmd and click each one)
2. In Inspector, check **"Navigation Static"** ✅
3. Click **"Apply"** if prompted

**Important:** 
- Mark obstacles as Navigation Static **BEFORE** baking NavMesh
- If you add obstacles later, you need to re-bake NavMesh

### Step 3: Re-bake NavMesh

1. Select your NavMesh Manager
2. Click **"Bake"** button again
3. NavMesh will now have holes where obstacles are
4. The kidnapper will walk around obstacles automatically

---

## Method 2: Use NavMesh Modifier (For Complex Cases)

If you want more control over which objects block navigation:

### Step 1: Add NavMesh Modifier to Obstacle

1. Select an obstacle object
2. Add Component → **Nav Mesh Modifier**
3. Configure:
   - **Override Area**: Check ✅
   - **Area Type**: **Not Walkable**
   - **Affected Objects**: **Children** (if obstacle has child objects)

### Step 2: Re-bake NavMesh

This tells NavMesh to exclude this specific area.

---

## Method 3: NavMesh Obstacle (For Moving Objects)

If you have objects that might move or be removed during gameplay:

### Step 1: Add NavMesh Obstacle Component

1. Select the moving object
2. Add Component → **Nav Mesh Obstacle**
3. Configure:
   - **Shape**: **Box** or **Capsule** (match object shape)
   - **Carve**: Check ✅ (creates hole in NavMesh)
   - **Move Threshold**: 0.1 (how much it needs to move before updating)
   - **Time To Stationary**: 0.1 (how long stationary before updating)

### Step 2: Adjust Size

- **Center**: Position of obstacle
- **Size**: Match the obstacle's size

**Note:** NavMesh Obstacle works at runtime, so you don't need to re-bake.

---

## Method 4: Adjust Agent Settings for Better Navigation

If the kidnapper is getting stuck or bumping into things:

### In NavMesh Surface Settings:

1. Select NavMesh Manager
2. In NavMesh Surface component:
   - **Agent Radius**: 0.5 (standard)
     - Increase to 0.6-0.7 if agent keeps bumping into walls
     - Decrease to 0.4 if agent can't fit through doorways
   - **Agent Height**: 2 (standard)
   - **Max Slope**: 60 (for stairs)
   - **Step Height**: 0.4 (for stairs)

### In NavMesh Agent Component (on Kidnapper):

1. Select Kidnapper GameObject
2. In NavMesh Agent component:
   - **Obstacle Avoidance**: **High Quality Obstacle Avoidance**
   - **Radius**: Match NavMesh Surface (0.5)
   - **Height**: Match NavMesh Surface (2)

---

## Common Obstacle Scenarios

### Scenario 1: Furniture on Floors

**Solution:**
- Mark all furniture as **Navigation Static** ✅
- Re-bake NavMesh
- Kidnapper will walk around furniture

### Scenario 2: Tables with Legs (Gaps Underneath)

**Solution:**
- Mark table top as Navigation Static
- NavMesh will go under the table if there's space
- Or create a NavMesh Modifier Volume to block the area

### Scenario 3: Chairs That Can Be Moved

**Solution:**
- Use **NavMesh Obstacle** component (Method 3)
- Set **Carve** to true
- Agent will avoid it, and it updates if moved

### Scenario 4: Doorways That Are Too Narrow

**Solution:**
- Check doorway width (should be at least 1 unit for radius 0.5 agent)
- If too narrow:
  - Widen doorway in your model, OR
  - Decrease Agent Radius to 0.4, OR
  - Mark doorway walls as NOT Navigation Static (let agent walk through)

### Scenario 5: Stairs Between Floors

**Solution:**
- Mark stairs as **Navigation Static** ✅
- Set **Max Slope** to 60-90 degrees in NavMesh Surface
- Set **Step Height** to 0.4 or higher
- Re-bake NavMesh
- Agent should be able to walk up/down stairs

---

## Testing Obstacle Avoidance

### Step 1: Visual Check

1. Bake NavMesh
2. In Scene view, look at the blue NavMesh overlay
3. You should see **holes** where obstacles are
4. NavMesh should go **around** obstacles, not through them

### Step 2: Test Navigation

1. Place patrol points on opposite sides of an obstacle
2. Press Play
3. Watch kidnapper navigate
4. Should walk **around** obstacle, not through it

### Step 3: Check Problem Areas

1. Look for areas where kidnapper gets stuck
2. Check if obstacles are properly marked
3. Verify NavMesh has paths around obstacles
4. Adjust Agent Radius if needed

---

## Troubleshooting

### Problem: Kidnapper Walks Through Obstacles

**Solutions:**
- Make sure obstacles are marked as **Navigation Static** ✅
- Re-bake NavMesh after marking obstacles
- Check that obstacles are on included layers
- Verify NavMesh Surface "Collect Objects" includes obstacles

### Problem: Kidnapper Gets Stuck on Obstacles

**Solutions:**
- Increase **Agent Radius** slightly (0.6-0.7)
- Check **Obstacle Avoidance** is set to High Quality
- Make sure there's enough space around obstacles (at least 1 unit clearance)
- Verify NavMesh has paths around obstacles

### Problem: NavMesh Doesn't Show Holes for Obstacles

**Solutions:**
- Make sure obstacles are marked as Navigation Static
- Re-bake NavMesh
- Check obstacle size (very small objects might not create visible holes)
- Verify obstacles are within NavMesh Surface bounds

### Problem: Too Many Obstacles = No Walkable Space

**Solutions:**
- Remove some obstacles from Navigation Static (make them non-blocking)
- Increase floor size to provide more walkable area
- Use NavMesh Modifier to mark only important obstacles
- Adjust Agent Radius to fit through tighter spaces

### Problem: Dynamic Obstacles Not Working

**Solutions:**
- Use **NavMesh Obstacle** component instead of Navigation Static
- Make sure **Carve** is enabled
- Check **Move Threshold** and **Time To Stationary** settings
- Verify obstacle has Collider component

---

## Best Practices

### 1. Mark Obstacles Before Baking
- Always mark obstacles as Navigation Static **before** first bake
- Saves time and ensures correct NavMesh

### 2. Organize Obstacles
- Group obstacles in Hierarchy (e.g., "Furniture", "Walls", "Obstacles")
- Makes it easier to select and mark them all at once

### 3. Test Navigation Paths
- Place test patrol points around obstacles
- Verify kidnapper can reach all areas
- Adjust if needed

### 4. Balance Realism vs. Gameplay
- Too many obstacles = frustrating navigation
- Too few = unrealistic
- Find a balance that works for your game

### 5. Use Layers for Organization
- Put obstacles on a separate layer (e.g., "Obstacles")
- Makes it easier to select and manage them

---

## Quick Checklist

- [ ] Identified all obstacles on floors
- [ ] Marked obstacles as Navigation Static
- [ ] Re-baked NavMesh
- [ ] Verified blue NavMesh shows holes around obstacles
- [ ] Tested kidnapper navigation around obstacles
- [ ] Adjusted Agent Radius if needed
- [ ] Tested doorways and tight spaces
- [ ] Verified stairs work (if applicable)
- [ ] Checked for stuck areas
- [ ] Optimized obstacle placement

---

## Example Setup

```
House (FBX Import)
├── Basement
│   ├── Floor (Navigation Static) ✅
│   ├── Table (Navigation Static) ✅
│   ├── Chairs (Navigation Static) ✅
│   └── Walls (Navigation Static) ✅
│
├── Ground Floor
│   ├── Floor (Navigation Static) ✅
│   ├── Sofa (Navigation Static) ✅
│   ├── Cabinet (Navigation Static) ✅
│   └── Interior Walls (Navigation Static) ✅
│
└── First Floor
    ├── Floor (Navigation Static) ✅
    ├── Bed (Navigation Static) ✅
    └── Desk (Navigation Static) ✅

NavMesh_Floors (Custom Floor Planes)
├── NavMesh_BasementFloor (Navigation Static) ✅
├── NavMesh_GroundFloor (Navigation Static) ✅
└── NavMesh_FirstFloor (Navigation Static) ✅
```

**Note:** Both the imported floors AND your custom floor planes can be marked as Navigation Static. NavMesh will use whichever works better.

---

## Summary

**Simple Rule:**
- **Navigation Static** = NavMesh avoids this area (creates holes)
- **Not Navigation Static** = NavMesh can cover this area (walkable)

**For obstacles:** Mark as Navigation Static ✅  
**For floors:** Mark as Navigation Static ✅  
**For moving objects:** Use NavMesh Obstacle component

Your kidnapper will now intelligently navigate around all obstacles! 🎮


