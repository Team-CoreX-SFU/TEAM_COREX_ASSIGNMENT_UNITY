# Power Supply Interaction System - Setup Guide

This guide will walk you through setting up the power supply interaction system with keyboard-based controls (similar to Radio system).

## Overview

The system works like this:
1. **Player collects screwdriver** (picks it up - it disappears like a battery)
2. **Player walks to trigger zone** near PowerSupplyCase
3. **Player presses 'E' key multiple times** - Each press does the next step:
   - **First 'E' press** → PowerSupplyCover opens (Y rotation: -180° to -70°)
   - **Second 'E' press** → PowerSupplySwitch turns off (Z rotation: -90° to 0°)
   - **Third 'E' press** → PowerSupplyFuse is removed
   
Each step waits for the previous animation to complete before allowing the next step.

## Components

1. **ScrewdriverPickup** - Tracks when screwdriver is collected (disappears on pickup)
2. **PowerSupplyCoverController** - Opens the cover with animation
3. **PowerSupplySwitchController** - Switches power on/off
4. **PowerSupplyFuseController** - Removes the fuse
5. **PowerSupplyTriggerZone** - Single trigger zone that handles all interactions with 'E' key

---

## Step 1: Setup Screwdriver Object

### 1.1 Prepare Screwdriver GameObject
1. Select your **Screwdriver** GameObject in the scene
2. Ensure it has:
   - A **Collider** (for physics interaction)
   - An **XRGrabInteractable** component (for XR grabbing)
   - A **Rigidbody** (if needed for physics)

### 1.2 Add ScrewdriverPickup Script
1. Add the **ScrewdriverPickup** component to the Screwdriver GameObject
2. Configure the settings:
   - **Screwdriver Tag**: "Screwdriver" (or your preferred tag)
   - **Disappear On Collection**: `true` (screwdriver disappears when picked up, like battery)
   - **Disappear Delay**: `0.1` seconds

### 1.3 Configure XRGrabInteractable
- Ensure the XRGrabInteractable is properly configured for your XR setup
- When player grabs the screwdriver, it will disappear and the system will remember it was collected

---

## Step 2: Setup Power Supply Cover

### 2.1 Prepare Cover GameObject
1. Select the **PowerSupplyCover** GameObject (the object that opens)
2. This should be a child of the power supply case or a separate object

### 2.2 Add PowerSupplyCoverController
1. Add the **PowerSupplyCoverController** component to the Cover GameObject
2. Configure the settings:
   - **Cover Transform**: If the script is on the cover itself, leave null. Otherwise, assign the cover's Transform
   - **Closed Angle**: `-180` (starting position - closed)
   - **Open Angle**: `-70` (ending position - open)
   - **Animation Duration**: `1.5` seconds (adjust as needed)
   - **Animation Curve**: Optional - leave default for smooth easing

### 2.3 Set Initial Rotation
- The script will automatically set the cover to closed position (-180°) on Awake
- Make sure your cover's initial rotation in the scene matches this

### 2.4 (Optional) Add Audio
- Add an **AudioSource** component to the Cover GameObject
- Assign it to the **Audio Source** field in PowerSupplyCoverController
- Assign an **Audio Clip** to the **Open Sound** field

---

## Step 3: Setup Power Supply Switch

### 3.1 Prepare Switch GameObject
1. Select the **PowerSupplySwitch** GameObject (the switch/lever object)
2. This should be a separate object that rotates

### 3.2 Add PowerSupplySwitchController
1. Add the **PowerSupplySwitchController** component to the Switch GameObject
2. Configure the settings:
   - **Switch Transform**: If the script is on the switch itself, leave null
   - **On Angle**: `-90` (ON position)
   - **Off Angle**: `0` (OFF position)
   - **Animation Duration**: `0.8` seconds (adjust as needed)
   - **Can Interact**: `true` (allows interaction)

### 3.3 Set Initial Rotation
- The script will automatically set the switch to ON position (-90°) on Awake
- Make sure your switch's initial rotation matches this

### 3.4 (Optional) Add Audio
- Add an **AudioSource** component
- Assign **Switch Off Sound** and **Switch On Sound** clips

### 3.5 Setup Light Control
1. In the **PowerSupplySwitchController** component, find the **Light Control** section
2. **Option A - Direct Light Assignment**:
   - Expand **Lights To Turn Off** array
   - Set the array size to the number of lights you want to control
   - Drag Light components directly into the array slots
3. **Option B - GameObject Assignment**:
   - Expand **Light Objects To Turn Off** array
   - Set the array size to the number of GameObjects with lights
   - Drag GameObjects that have Light components into the array
   - Enable **Search Children For Lights** if lights are child objects
4. When the switch is turned off, all assigned lights will automatically turn off
5. If the switch can be turned back on, lights will turn back on automatically

---

## Step 4: Setup Power Supply Fuse

### 4.1 Prepare Fuse GameObject
1. Select the **PowerSupplyFuse** GameObject (inside the power supply case)
2. This is the object that will be removed

### 4.2 Add PowerSupplyFuseController
1. Add the **PowerSupplyFuseController** component to the Fuse GameObject
2. Configure the settings:
   - **Fuse Object**: Leave null if script is on the fuse itself
   - **Removal Delay**: `0.5` seconds
   - **Destroy On Removal**: `true` (fuse disappears) or `false` (just disables)
   - **Animate Removal**: `true` (moves up and fades out)
   - **Removal Height**: `0.2` units (how far up it moves)
   - **Removal Animation Duration**: `0.8` seconds

### 4.3 (Optional) Add Audio
- Add an **AudioSource** component
- Assign a **Removal Sound** clip

---

## Step 5: Setup Power Supply Trigger Zone (IMPORTANT)

This is the main interaction zone that handles everything with a single 'E' key press.

### 5.1 Create Trigger Zone GameObject
1. Create an empty GameObject named **"PowerSupplyTriggerZone"**
2. Position it near the PowerSupplyCase (where player should stand to interact)
3. Add a **Collider** component:
   - Set **Is Trigger** = `true`
   - Adjust size to cover the interaction area (e.g., Box Collider with size 1 x 1 x 1)
   - Make sure it's large enough for player to easily enter

### 5.2 Add PowerSupplyTriggerZone Script
1. Add the **PowerSupplyTriggerZone** component to the trigger zone GameObject
2. Configure the settings:
   - **Cover Controller**: Drag the PowerSupplyCover GameObject here
   - **Switch Controller**: Drag the PowerSupplySwitch GameObject here
   - **Fuse Controller**: Drag the PowerSupplyFuse GameObject here
   - **Player Tag**: "Player" (or your player's tag)
   - **Interaction Key**: `E` (Key.E)
   - **Require Screwdriver**: `true` (player must collect screwdriver first)
   - **Delay Between Steps**: `0.5` seconds (wait time after each animation completes)

### 5.3 (Optional) Add Audio
- Add an **AudioSource** component to the trigger zone
- Assign an **Interaction Sound** clip

---

## Step 6: Testing the System

### Testing Order:
1. **Collect Screwdriver**: 
   - Pick up the screwdriver - it should disappear
   - System now knows you have the screwdriver

2. **Enter Trigger Zone**: 
   - Walk into the PowerSupplyTriggerZone area
   - You should be inside the trigger collider

3. **Press 'E' Key Multiple Times**: 
   - **First 'E' press** → Cover should start opening (Y: -180° → -70°)
   - Wait for cover animation to complete
   - **Second 'E' press** → Switch should turn off (Z: -90° → 0°)
   - Wait for switch animation to complete
   - **Third 'E' press** → Fuse should animate and disappear
   
   Each step happens one at a time, so you can see each action clearly!

---

## Troubleshooting

### Nothing happens when pressing 'E':
- ✅ Check that player tag matches ("Player")
- ✅ Verify you're inside the trigger zone (check collider size)
- ✅ Ensure screwdriver was collected first (check ScrewdriverPickup.HasScrewdriver())
- ✅ Check that all three controllers are assigned in PowerSupplyTriggerZone
- ✅ Verify trigger collider has "Is Trigger" = true
- ✅ Make sure you're pressing 'E' multiple times (once for each step)
- ✅ Wait for each animation to complete before pressing 'E' again

### Cover doesn't open:
- ✅ Check CoverController is assigned in trigger zone
- ✅ Verify cover's initial rotation is correct (-180° on Y axis)
- ✅ Check if cover is already open (IsOpen() returns true)

### Switch doesn't turn off:
- ✅ Check SwitchController is assigned in trigger zone
- ✅ Verify switch's initial rotation is correct (-90° on Z axis)
- ✅ Check if switch is already off

### Fuse doesn't remove:
- ✅ Check FuseController is assigned in trigger zone
- ✅ Verify fuse GameObject is active
- ✅ Check if fuse was already removed

### Screwdriver doesn't disappear:
- ✅ Check "Disappear On Collection" is enabled in ScrewdriverPickup
- ✅ Verify XRGrabInteractable is properly configured
- ✅ Check if there are any errors in console

### Can't collect screwdriver:
- ✅ Verify XRGrabInteractable component is present
- ✅ Check collider is not set as trigger (should be solid for grabbing)
- ✅ Ensure XR Interaction Manager is in scene

---

## Customization

### Change Interaction Key:
- In PowerSupplyTriggerZone, change **Interaction Key** to any Key enum value
- Example: `Key.F` for F key, `Key.Space` for spacebar

### Adjust Step Delays:
- Change **Delay Between Steps** in PowerSupplyTriggerZone
- This controls how long to wait after each animation completes before allowing next step
- Default is 0.5 seconds

### Disable Screwdriver Requirement:
- Set **Require Screwdriver** = `false` in PowerSupplyTriggerZone
- Actions will trigger even without collecting screwdriver

### Adjust Animation Speeds:
- **Cover**: Change **Animation Duration** in PowerSupplyCoverController
- **Switch**: Change **Animation Duration** in PowerSupplySwitchController
- **Fuse**: Change **Removal Animation Duration** in PowerSupplyFuseController

### Change Rotation Angles:
- **Cover**: Modify **Closed Angle** and **Open Angle** if your model uses different rotations
- **Switch**: Modify **On Angle** and **Off Angle** if your switch model differs

---

## Quick Reference: Component Checklist

### Screwdriver:
- [ ] ScrewdriverPickup component
- [ ] XRGrabInteractable component
- [ ] Collider component (NOT trigger)
- [ ] Tag set to "Screwdriver"
- [ ] Disappear On Collection = true

### Cover:
- [ ] PowerSupplyCoverController component
- [ ] (Optional) AudioSource component

### Switch:
- [ ] PowerSupplySwitchController component
- [ ] (Optional) AudioSource component

### Fuse:
- [ ] PowerSupplyFuseController component
- [ ] (Optional) AudioSource component

### Trigger Zone:
- [ ] PowerSupplyTriggerZone component
- [ ] Collider component (IS Trigger = true)
- [ ] All three controllers assigned (Cover, Switch, Fuse)
- [ ] Player Tag = "Player"
- [ ] Interaction Key = E
- [ ] Require Screwdriver = true

---

## Code Reference

### Check if Screwdriver is Collected:
```csharp
if (ScrewdriverPickup.HasScrewdriver())
{
    // Player has the screwdriver
}
```

### Reset Screwdriver State (for testing):
```csharp
ScrewdriverPickup.ResetScrewdriverState();
```

### Manually Trigger Interaction (for testing):
```csharp
PowerSupplyTriggerZone triggerZone = GetComponent<PowerSupplyTriggerZone>();
triggerZone.TriggerInteraction();
```

---

## Notes

- The system works similar to the Radio system - simple trigger zone + keyboard input
- Screwdriver disappears on collection (like BatteryPickup)
- All three actions happen simultaneously when 'E' is pressed
- The trigger zone must be large enough for player to easily enter
- Make sure player GameObject has the correct tag ("Player")

---

Good luck with your setup! 🛠️
