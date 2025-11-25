# How to Add WASD to Continuous Move Provider

Yes! You can absolutely set up WASD to work through the same Input Actions system as Continuous Move Provider. This unifies your input and makes footstep tracking work consistently.

---

## Why Do This?

**Benefits:**
- ✅ **Unified Input System** - Both WASD and VR controllers use the same path
- ✅ **Consistent Footstep Tracking** - Same movement detection for both (see `WHY_CONSISTENT_FOOTSTEP_TRACKING.md` for details)
- ✅ **Easier to Maintain** - One input system instead of two
- ✅ **Better for Testing** - WASD works the same way as VR movement

**What "Consistent Footstep Tracking" means:**
- Your footstep script tracks **XR Origin position** to detect movement
- If WASD moves XR Origin (via Continuous Move Provider) → Footsteps work ✅
- If WASD moves something else (direct input) → Footsteps might NOT work ❌
- By using the same system, footsteps work reliably for BOTH input methods!

**How it works:**
- WASD keys → Input Actions → Continuous Move Provider → XR Origin
- Same path as VR controllers, just different input source

---

## Step-by-Step: Add WASD to Input Actions

### Step 1: Open Input Actions File

1. In Unity Project window, navigate to:
   `Assets/Samples/XR Interaction Toolkit/2.5.4/Starter Assets/`
2. Find **"XRI Default Input Actions.inputactions"**
3. **Double-click** it to open in the Input Actions window
   - A new window should open showing all Input Action Maps

### Step 2: Find "XRI LeftHand Locomotion" Map

1. In the left panel, look for **"XRI LeftHand Locomotion"** action map
   - ⚠️ **Important:** It's NOT "XRI LeftHand" - it's "XRI LeftHand **Locomotion**"
   - You should see it in the list: "XRI LeftHand Locomotion"
   - Scroll down in the left panel if you don't see it immediately
2. Click on **"XRI LeftHand Locomotion"** to expand it
3. You'll see several actions:
   - Teleport Select
   - Teleport Mode Activate
   - Teleport Mode Cancel
   - Teleport Direction
   - **Turn**
   - **Move** ← This is the one you want!
   - Grab Move
   - Snap Turn
4. Click on **"Move"** to select it (it's a Vector2 action)

### Step 3: Add Keyboard Binding

**Where to find the "+" button:**

**Option 1: Use the "+" next to "Move" action (Easiest)**
1. In the **middle panel** (Actions), look at the **"Move"** action
2. You should see a **"+"** button to the **right** of the "Move" action name
3. Click that **"+"** button
4. A new binding will appear below "Primary2DAxis [LeftHand XR Controller]"

**Option 2: Use the "+" in Binding Properties**
1. In the **right panel** (Binding Properties), at the very top
2. You should see a **"+"** button in the **Binding Properties** section
3. Click that **"+"** button
4. A new binding will appear

**After clicking "+":**
5. A new binding will appear (it might say "No Binding" or be empty)
6. Click on the new binding to select it
7. In the **"Path"** field (in the right panel), click the **dropdown arrow** or **"T"** button
8. Press **W** on your keyboard, or type: `<Keyboard>/w`
9. It should show: **"<Keyboard>/w"**

### Step 4: Add 2D Vector Composite for Full WASD

**Method 1: Right-Click to Add Composite (Recommended)**

1. With **"Move"** action selected, **right-click** on the **"Move"** action in the middle panel
2. A context menu appears - select **"Add Composite Binding"** or **"Add Binding"** → **"2D Vector Composite"**
3. This creates 4 sub-bindings: Up, Down, Left, Right
4. For each one:
   - Click **"Up"** → In Path field (right panel), click dropdown → Press **W** or type `<Keyboard>/w`
   - Click **"Down"** → In Path field, click dropdown → Press **S** or type `<Keyboard>/s`
   - Click **"Left"** → In Path field, click dropdown → Press **A** or type `<Keyboard>/a`
   - Click **"Right"** → In Path field, click dropdown → Press **D** or type `<Keyboard>/d`

**Method 2: Add Individual Bindings (If Composite doesn't work)**

If you can't find "2D Vector Composite", you can add individual bindings:

1. Click **"+"** next to "Move" action (adds a new binding)
2. Click the new binding
3. In Path field, click dropdown → Press **W** or type `<Keyboard>/w`
4. Repeat for S, A, D keys (add 3 more bindings)

**Method 3: Change Binding Type After Adding**

1. Click **"+"** to add a new binding
2. Click on the new binding
3. In the **right panel**, look for a **"Binding"** or **"Type"** dropdown (might be near the top)
4. Change it from "Path" to **"Composite"** or **"2D Vector"**
5. This should convert it to a composite with Up/Down/Left/Right

**Important:** 
- Make sure the binding group is set to **"Continuous Move"** (check the checkbox in "Use in control scheme" section)
- Or leave it unchecked to work with all control schemes

### Step 5: Repeat for Right Hand (Optional)

1. Find **"XRI RightHand Locomotion"** action map (NOT "XRI RightHand")
2. Find **"Move"** action
3. Add the same WASD bindings (2D Vector Composite)
4. This allows keyboard to work for both hands

### Step 6: Save

1. Click **"Save Asset"** button (top of Input Actions window)
2. Or press **Ctrl+S** (Windows) / **Cmd+S** (Mac)
3. Unity will recompile the Input System

---

## Alternative: Quick Method Using Unity Editor

If the Input Actions window is confusing, you can also:

### Method 1: Edit JSON Directly (Advanced)

1. Right-click **"XRI Default Input Actions.inputactions"**
2. Select **"Open With"** → **Text Editor**
3. Find the "Left Hand Move" action
4. Add keyboard bindings in JSON format

### Method 2: Use Input System Package Window

1. Window → **Analysis → Input Debugger**
2. This shows all input actions and their bindings
3. You can test bindings here

---

## What the Bindings Look Like

After adding WASD, your Input Actions should have:

```
Left Hand Move Action:
├── <XRController>{LeftHand}/primary2DAxis (VR Controller)
├── <Keyboard>/w (W key - Up)
├── <Keyboard>/s (S key - Down)
├── <Keyboard>/a (A key - Left)
└── <Keyboard>/d (D key - Right)
```

---

## Testing with MOCKHMD

**Important for MOCKHMD users:**
- MOCKHMD is Unity's mock VR headset for testing
- It simulates VR but uses keyboard/mouse
- Adding WASD bindings makes testing much easier!

### Step 1: Enter Play Mode

1. Press **Play**
2. With MOCKHMD active, you should now be able to move with **WASD** keys
3. No need for Shift+W anymore!
4. Movement should work the same as VR controller thumbstick

### Step 2: Verify Movement

1. Press **W** → Should move forward
2. Press **A** → Should move left
3. Press **S** → Should move backward
4. Press **D** → Should move right
5. Movement should be smooth (continuous)

### Step 3: Test Footsteps

1. Move with WASD
2. Footstep sounds should play
3. Check Console for speed values
4. Should work the same as Shift+W movement

---

## Troubleshooting

### Problem: WASD Doesn't Work After Adding Bindings

**Solutions:**
- Make sure you **saved** the Input Actions file
- Check that **Continuous Move Provider** is enabled
- Verify bindings are correct (W, A, S, D keys)
- Restart Unity if changes don't apply

### Problem: Both WASD and VR Controller Work (Conflict)

**Solutions:**
- This is actually fine! Both can work simultaneously
- If you want to disable VR controller when keyboard is used, you'd need custom logic
- Usually, both working is the desired behavior

### Problem: Movement is Too Fast/Slow

**Solutions:**
- Adjust **Move Speed** in Continuous Move Provider (currently 2)
- Lower = slower movement
- Higher = faster movement

### Problem: WASD Works But Footsteps Don't

**Solutions:**
- Check that footstep script is tracking **XR Origin position**
- Verify **Walking Speed Threshold** is low enough (0.1)
- Check Console for speed values
- Make sure sound files are assigned

---

## Current vs. New Setup

### Before (Separate Systems):
```
WASD → Direct Input → Player Movement (separate system)
Shift+W → Input Actions → Continuous Move Provider → XR Origin
```

### After (Unified System):
```
WASD → Input Actions → Continuous Move Provider → XR Origin
VR Controller → Input Actions → Continuous Move Provider → XR Origin
```

**Both use the same path now!** ✅

---

## Advanced: Custom Input Actions

If you want to create your own Input Actions file:

1. Right-click in Project → **Create → Input Actions**
2. Name it: **"My Custom Input Actions"**
3. Add actions:
   - **Move** (Vector2)
   - **Turn** (Vector2)
   - **Jump** (Button)
4. Bind WASD to Move action
5. Assign to Continuous Move Provider

---

## Visual Example: What It Looks Like

After adding WASD, your Input Actions window should show:

```
XRI LeftHand Locomotion
└── Move (Vector2)
    ├── <XRController>{LeftHand}/{Primary2DAxis} [Continuous Move group]
    └── 2D Vector Composite
        ├── Up: <Keyboard>/w
        ├── Down: <Keyboard>/s
        ├── Left: <Keyboard>/a
        └── Right: <Keyboard>/d
```

**Note:** The action is in "XRI LeftHand **Locomotion**", not "XRI LeftHand"!

## Summary

**To add WASD to Continuous Move Provider:**

1. ✅ Open **XRI Default Input Actions.inputactions**
2. ✅ Find **"XRI LeftHand Locomotion"** → **"Move"** action (NOT "XRI LeftHand"!)
3. ✅ Add **2D Vector Composite** binding
4. ✅ Bind **W, A, S, D** keys to Up, Down, Left, Right
5. ✅ Set binding group to **"Continuous Move"** (or leave empty)
6. ✅ Save the file
7. ✅ Test in Play Mode with MOCKHMD

**Result:**
- WASD now works through Input Actions ✅
- Same system as VR controllers ✅
- Consistent footstep tracking ✅
- Unified input management ✅
- Works perfectly with MOCKHMD ✅

**Before:** WASD → Separate system | Shift+W → Input Actions  
**After:** WASD → Input Actions | VR Controller → Input Actions  
**Both use the same path now!** 🎮

