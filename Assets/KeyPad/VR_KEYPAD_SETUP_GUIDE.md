# VR Keypad Setup Guide

This guide will walk you through setting up a fully functional VR keypad with XR Interaction Toolkit.

## Overview

The VR keypad system consists of:
1. **KeyPadScript** - Main controller that manages code input and validation
2. **VRKeypadButton** - Script attached to each button for VR interaction
3. **Number** - Existing script that defines button numbers (0-9)

## Features

- ✅ **VR Interaction** - Works with XR hands/controllers
- ✅ **Max Length: 3 digits** - Configurable in inspector
- ✅ **Correct Code: 152** - Set in inspector
- ✅ **"WRONG" Message** - Displays when incorrect code is entered
- ✅ **Auto-reset** - Resets after wrong code (1.5 second delay)
- ✅ **Audio Support** - Optional button press, correct, and wrong sounds
- ✅ **Events** - UnityEvents for correct/wrong code (for custom actions)

---

## Step-by-Step Setup

### Step 1: Setup KeyPadScript (Main Controller)

1. **Select your Keypad GameObject** (the parent object that contains all buttons)

2. **Add/Update KeyPadScript Component:**
   - Add `KeyPadScript` component if not already present
   - Configure the following:
     - **Max Code Length**: `3`
     - **Correct Code**: `152`
     - **Screen**: Drag your display GameObject (the one with TextMeshPro) here

3. **Optional Audio Setup:**
   - Add an `AudioSource` component to the keypad GameObject
   - Assign it to **Audio Source** field in KeyPadScript
   - Assign audio clips:
     - **Button Press Sound**: Sound when any button is pressed
     - **Correct Sound**: Sound when correct code is entered
     - **Wrong Sound**: Sound when wrong code is entered

4. **Optional Events:**
   - **On Code Correct**: Add listeners for when correct code is entered (e.g., open door, unlock item)
   - **On Code Wrong**: Add listeners for when wrong code is entered

---

### Step 2: Setup Each Button for VR Interaction

For **EACH** button (Button0, Button1, Button2, etc.):

1. **Select the Button GameObject**

2. **Add Required Components:**
   - **Collider** (Box Collider or Mesh Collider) - Must be set as **Trigger**
   - **XR Simple Interactable** component:
     - Go to `Add Component` → Search `XR Simple Interactable`
     - This enables VR interaction

3. **Add VRKeypadButton Script:**
   - Add `VRKeypadButton` component
   - **Button Number**: Will auto-detect from `Number` component
   - **Keypad Script**: Drag the GameObject with `KeyPadScript` here (or leave empty to auto-find)

4. **Verify Number Component:**
   - Ensure the button has a `Number` component
   - The `number` field should match the button (0-9)

---

### Step 3: Setup Display Screen

1. **Select your Display GameObject** (the screen that shows the code)

2. **Ensure it has:**
   - **TextMeshPro** component (not regular Text)
   - Text should be visible and readable

3. **Assign to KeyPadScript:**
   - Drag this GameObject to the **Screen** field in KeyPadScript

---

### Step 4: Test in VR

1. **Enter Play Mode**

2. **Test Button Presses:**
   - Point at a button with your VR controller/hand
   - Press the trigger/button to interact
   - The number should appear on the display

3. **Test Code Entry:**
   - Press buttons: `1`, `5`, `2` (in order)
   - Should show "CORRECT" or trigger your OnCodeCorrect event
   - Try wrong code (e.g., `1`, `2`, `3`)
   - Should show "WRONG" in red, then reset after 1.5 seconds

---

## Configuration Summary

### KeyPadScript Settings:
```
Max Code Length: 3
Correct Code: "152"
Screen: [Your Display GameObject]
```

### Each Button Needs:
- ✅ Collider (Trigger)
- ✅ XR Simple Interactable
- ✅ VRKeypadButton script
- ✅ Number component

---

## Troubleshooting

### Buttons not responding in VR:
- ✅ Check that each button has `XR Simple Interactable` component
- ✅ Check that colliders are set as **Trigger**
- ✅ Verify `VRKeypadButton` script is attached and `Keypad Script` is assigned
- ✅ Ensure you have XR Interaction Manager in the scene

### Display not updating:
- ✅ Check that Screen GameObject has `TextMeshPro` component (not regular Text)
- ✅ Verify Screen field in KeyPadScript is assigned
- ✅ Check console for error messages

### Code not validating:
- ✅ Verify `Correct Code` is set to "152" (as string, not number)
- ✅ Check `Max Code Length` is set to 3
- ✅ Look at console logs to see what code is being entered

### "WRONG" message not showing:
- ✅ Check that Screen GameObject is assigned
- ✅ Verify TextMeshPro component exists
- ✅ Check console for errors

---

## Advanced: Custom Actions on Correct Code

You can add custom actions when the correct code is entered:

1. In KeyPadScript inspector, find **On Code Correct** event
2. Click **+** to add a listener
3. Drag the GameObject you want to affect
4. Select the method to call (e.g., `Door.Open()`, `GameObject.SetActive(true)`, etc.)

Example: Open a door when correct code is entered:
- Add listener to `On Code Correct`
- Drag your Door GameObject
- Select method: `Door.Open()` or `Animator.SetTrigger("Open")`

---

## Notes

- The keypad supports both **VR interaction** (XR) and **mouse clicks** (for testing in editor)
- Code automatically resets after wrong entry (1.5 second delay)
- Display shows dashes (`---`) for empty slots
- Display shows entered digits as you type
- Display shows "WRONG" in red when incorrect code is entered

