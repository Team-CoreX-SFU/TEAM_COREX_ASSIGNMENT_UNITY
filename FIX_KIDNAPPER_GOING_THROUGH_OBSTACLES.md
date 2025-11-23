# Fix: Kidnapper Going Through Obstacles

**This is NOT normal!** The kidnapper should walk **around** obstacles, not through them.

## Why This Happens

The kidnapper goes through obstacles because:
1. Obstacles are **not marked as Navigation Static** ❌
2. NavMesh was baked **before** obstacles were marked
3. Obstacles are on a layer that's **excluded** from NavMesh
4. Obstacles don't have **colliders** (NavMesh needs colliders to detect them)

---

## Quick Fix

### Step 1: Mark All Obstacles as Navigation Static

1. **Select all obstacles** in your scene:
   - Furniture (tables, chairs, sofas, etc.)
   - Walls (interior walls)
   - Large objects (cabinets, shelves, etc.)
   - Hold **Ctrl** (Windows) or **Cmd** (Mac) and click each one

2. **Mark as Navigation Static:**
   - In Inspector, find **"Static"** dropdown (top right)
   - Check **"Navigation Static"** ✅
   - Click **"Apply"** if prompted

   **OR**

   - In Inspector, look for **"Navigation"** section
   - Check **"Navigation Static"** ✅

### Step 2: Verify Obstacles Have Colliders

1. Select an obstacle
2. In Inspector, check if it has:
   - **Box Collider**
   - **Mesh Collider**
   - **Capsule Collider**
   - Or any other Collider component

3. **If no collider:**
   - Click **"Add Component"**
   - Add **"Box Collider"** (or appropriate collider)
   - Adjust size to match the obstacle

### Step 3: Re-bake NavMesh

**This is CRITICAL!** NavMesh must be re-baked after marking obstacles.

1. Select your **NavMesh Manager** (or object with NavMesh Surface)
2. In Inspector, find **Nav Mesh Surface** component
3. Click **"Bake"** button
4. Wait for Unity to process (30-60 seconds)

### Step 4: Verify NavMesh Has Holes

1. In **Scene** view, look at the blue NavMesh overlay
2. You should see **holes/gaps** where obstacles are
3. NavMesh should go **around** obstacles, not through them
4. If you still see blue NavMesh going through obstacles:
   - Check that obstacles are marked as Navigation Static
   - Check that obstacles have colliders
   - Re-bake again

---

## Detailed Steps

### Method 1: Select Obstacles Individually

1. In Hierarchy, find your house/room objects
2. Expand to see all child objects
3. Select each obstacle one by one
4. Mark as Navigation Static
5. Re-bake NavMesh

### Method 2: Select Multiple Obstacles

1. Hold **Ctrl** (Windows) or **Cmd** (Mac)
2. Click each obstacle in Hierarchy
3. All selected objects will be highlighted
4. In Inspector, check **"Navigation Static"** ✅
5. Click **"Apply"** if prompted
6. Re-bake NavMesh

### Method 3: Use Layers (For Many Obstacles)

1. **Create Obstacle Layer:**
   - Edit → Project Settings → Tags and Layers
   - Find empty **User Layer** (e.g., Layer 8)
   - Name it: **"Obstacles"**

2. **Assign Obstacles to Layer:**
   - Select all obstacles
   - In Inspector, set **Layer** to **"Obstacles"**

3. **Include Layer in NavMesh:**
   - Select NavMesh Manager
   - In NavMesh Surface, find **"Include Layers"**
   - Make sure **"Obstacles"** layer is included ✅

4. **Mark and Bake:**
   - Select all obstacles
   - Mark as Navigation Static
   - Re-bake NavMesh

---

## Common Issues

### Issue 1: Obstacles Don't Have Colliders

**Problem:** NavMesh can't detect obstacles without colliders.

**Solution:**
1. Select obstacle
2. Add Component → **Box Collider** (or appropriate type)
3. Adjust size to match obstacle
4. Re-bake NavMesh

### Issue 2: NavMesh Baked Before Obstacles Marked

**Problem:** NavMesh was created before obstacles were marked, so it doesn't know about them.

**Solution:**
1. Mark all obstacles as Navigation Static
2. **Re-bake NavMesh** (this is essential!)
3. NavMesh will now have holes where obstacles are

### Issue 3: Obstacles Are Too Small

**Problem:** Very small obstacles might not create visible holes in NavMesh.

**Solution:**
1. Check obstacle size (should be at least 0.5 units)
2. Or combine small obstacles into groups
3. Or use NavMesh Modifier to explicitly block areas

### Issue 4: Obstacles Are Part of FBX

**Problem:** If obstacles are part of a large FBX import, you might not be able to select them separately.

**Solution:**
1. **Option A:** In your 3D software, separate obstacles before exporting
2. **Option B:** Use NavMesh Modifier Volume to block areas
3. **Option C:** Create invisible blocking objects (Cubes) where obstacles are

---

## Testing

### Visual Test:

1. **Bake NavMesh**
2. In **Scene** view, look at blue NavMesh overlay
3. **Check:**
   - ✅ Blue NavMesh goes **around** obstacles (has holes)
   - ❌ Blue NavMesh goes **through** obstacles (no holes) = Problem!

### Movement Test:

1. **Place patrol points:**
   - One on each side of an obstacle
   - Press Play
   - Watch kidnapper navigate

2. **Expected behavior:**
   - ✅ Kidnapper walks **around** obstacle
   - ❌ Kidnapper walks **through** obstacle = Problem!

---

## Quick Checklist

- [ ] All obstacles selected
- [ ] All obstacles marked as **Navigation Static** ✅
- [ ] All obstacles have **Colliders** ✅
- [ ] NavMesh **re-baked** after marking obstacles
- [ ] Blue NavMesh shows **holes** around obstacles
- [ ] Kidnapper walks **around** obstacles (tested)

---

## Example Setup

```
House (FBX)
├── Table (Navigation Static ✅, Box Collider ✅)
├── Chair (Navigation Static ✅, Box Collider ✅)
├── Sofa (Navigation Static ✅, Box Collider ✅)
└── Cabinet (Navigation Static ✅, Box Collider ✅)

NavMesh Manager
└── Nav Mesh Surface (Baked ✅)
```

**Result:** NavMesh has holes where obstacles are, kidnapper walks around them.

---

## If Still Not Working

### Try This:

1. **Delete old NavMesh:**
   - Select NavMesh Manager
   - In NavMesh Surface, there might be a "Clear" or delete option
   - Or just re-bake (it will overwrite)

2. **Check NavMesh Settings:**
   - Agent Radius: 0.5
   - Agent Height: 2
   - Make sure these match your obstacles

3. **Use NavMesh Modifier:**
   - Select obstacle
   - Add Component → **Nav Mesh Modifier**
   - Set **Area Type** to **"Not Walkable"**
   - Re-bake

4. **Create Blocking Objects:**
   - Create invisible Cubes where obstacles are
   - Mark as Navigation Static
   - Disable Mesh Renderer (make invisible)
   - Re-bake

---

## Summary

**The Fix:**
1. Mark obstacles as **Navigation Static** ✅
2. Make sure obstacles have **Colliders** ✅
3. **Re-bake NavMesh** ✅
4. Verify blue NavMesh has **holes** around obstacles ✅

**Remember:** NavMesh must be **re-baked** every time you add or change obstacles!

Your kidnapper should now walk around obstacles instead of through them! 🎮

