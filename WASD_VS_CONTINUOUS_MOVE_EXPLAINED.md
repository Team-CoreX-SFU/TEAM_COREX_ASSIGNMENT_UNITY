# WASD vs Continuous Move Provider - Explained

## Understanding Movement Systems in Unity XR

### WASD Movement (Traditional Keyboard Input)

**What it is:**
- Uses Unity's **old Input Manager** or **new Input System**
- Direct keyboard input (W, A, S, D keys)
- Works in **non-VR mode** or **Editor Play Mode**
- Simple, direct control

**How it works:**
- Press W → Move forward
- Press A → Move left
- Press S → Move backward  
- Press D → Move right
- Speed is usually constant (e.g., 2 units/second)

**In your project:**
- This is what you use when **NOT** in VR mode
- Works in Editor with MOCKHMD when no VR controllers are active
- Uses standard Unity input

---

### Continuous Move Provider (Shift+W) - XR Interaction Toolkit

**What it is:**
- Part of **XR Interaction Toolkit**
- Uses **Input Actions** (new Input System)
- Designed for **VR controllers** (thumbsticks)
- Also works with **keyboard** when configured

**How it works:**
- **Shift + W** = Move forward (keyboard shortcut)
- **VR Controller thumbstick** = Move in direction
- Uses **ActionBasedContinuousMoveProvider** component
- Movement is **continuous** (smooth, not teleport)

**In your project:**
- Located on **"Locomotion System"** GameObject
- Component: **ActionBasedContinuousMoveProvider**
- Move Speed: **2** (in your scene)
- Uses Input Actions: **"Left Hand Move"** and **"Right Hand Move"**

**Key Differences:**
1. **Input Source:**
   - WASD (Traditional): Direct keyboard → Character Controller/Transform
   - WASD (Unified): Keyboard → Input Actions → XR Interaction Toolkit → XR Origin ✅ **Recommended!**
   - Continuous Move: Input Actions → XR Interaction Toolkit → XR Origin

**💡 You can configure WASD to use Input Actions too!** See `HOW_TO_ADD_WASD_TO_CONTINUOUS_MOVE.md` for instructions.

2. **Movement Target:**
   - WASD: Usually moves the player GameObject directly
   - Continuous Move: Moves the **XR Origin** (parent of Camera Offset)

3. **Speed:**
   - WASD: Can be any value, usually set in script
   - Continuous Move: Set in **Move Speed** field (your scene has 2)

4. **Frame of Reference:**
   - WASD: Usually world-space or camera-relative
   - Continuous Move: Can be head-relative or controller-relative

---

## Why Footsteps Aren't Working

**The Problem:**
- Your footstep script tracks **Main Camera position**
- But with **Continuous Move Provider**, the **XR Origin** moves, not the camera
- The camera is a **child** of XR Origin, so it moves with it
- But tracking camera position might not detect movement correctly

**The Solution:**
- Track **XR Origin's position** instead (the parent that actually moves)
- OR track the **movement input** from Continuous Move Provider
- OR use the **Locomotion System's** movement data

---

## How to Fix Footstep Tracking

### Option 1: Track XR Origin Position (Recommended)
- Track the **XR Origin (XR Rig)** transform position
- This is what actually moves with Continuous Move Provider
- Camera is just a child, so it moves with the origin

### Option 2: Track Movement Input
- Get the movement input from **ActionBasedContinuousMoveProvider**
- Use the input magnitude to determine speed
- More accurate but requires accessing the component

### Option 3: Use Character Controller Velocity
- If XR Origin has a CharacterController
- Use `characterController.velocity.magnitude`
- Most accurate for actual movement speed

---

## Your Current Setup

**Locomotion System:**
- Component: **ActionBasedContinuousMoveProvider**
- Move Speed: **2** (units per second)
- Uses Input Actions for movement

**XR Origin (XR Rig):**
- Parent object that moves
- Contains Camera Offset → Main Camera
- This is what actually moves in world space

**Main Camera:**
- Child of Camera Offset
- Child of XR Origin
- Moves WITH the origin, but position relative to origin stays same

---

## Testing Movement

**To test WASD:**
- Disable Continuous Move Provider
- Use standard Unity input
- Move speed depends on your input script

**To test Continuous Move:**
- Enable Continuous Move Provider
- Press **Shift + W** (or use VR controller)
- Move Speed = 2 (from your scene settings)

---

## Summary

| Feature | WASD | Continuous Move Provider |
|---------|------|-------------------------|
| **Input** | Keyboard (W/A/S/D) | Input Actions (Shift+W or VR) |
| **System** | Old/New Input System | XR Interaction Toolkit |
| **Moves** | Player GameObject | XR Origin |
| **Speed** | Script-defined | Move Speed field (2) |
| **Use Case** | Editor/Non-VR | VR/XR |
| **Activation** | Always active | Shift+W or VR controller |

**For Footsteps:**
- Track **XR Origin position** (what actually moves)
- OR track **movement input magnitude** from Continuous Move Provider
- Don't track camera position (it's relative to origin)

