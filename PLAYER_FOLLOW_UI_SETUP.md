# Player Follow UI Setup Guide

This guide will help you set up the Player Follow UI system that displays notifications, timers, and other UI elements that follow the player's head/camera.

## Features

- **Follows Player Camera**: UI canvas automatically follows and faces the player's head
- **Collection Notifications**: Shows notifications when items are collected (already integrated with Screwdriver and Battery pickups)
  - **Do NOT auto-hide by default** - stay visible until manually hidden or replaced
- **Timers**: Display countdown timers that auto-hide when complete
- **Auto-Hide Notifications**: Regular notifications can auto-hide after a set duration (optional)
- **Easy Integration**: Simple static methods to show notifications from anywhere

## Quick Setup

### Option 1: Automatic Setup (Recommended)

1. **Add PlayerFollowUIManager to Scene**:
   - In your main scene, create an empty GameObject
   - Name it "PlayerFollowUIManager"
   - Add the `PlayerFollowUIManager` component to it
   - The system will automatically create the UI canvas if needed

2. **Configure Positions** (Optional but Recommended):
   - Find the `PlayerFollowUI` GameObject in the Hierarchy (created automatically)
   - In the Inspector, find the **UI Position Settings** section
   - Set **Notification Position** to `BottomRight`
   - Set **Timer Position** to `TopLeft`
   - Adjust offsets if needed (default `20, 50` works well)

3. **Done!** The system is ready to use.

### Option 2: Manual Setup

1. **Create UI Canvas**:
   - Create a new GameObject in your scene
   - Name it "PlayerFollowUI"
   - Add a `Canvas` component
   - Set Canvas `Render Mode` to `World Space`
   - Add the `PlayerFollowUI` component

2. **Configure PlayerFollowUI**:
   - Assign the Main Camera (or XR Camera) to `Target Camera` field (optional - auto-found if null)
   - Adjust `Distance From Camera` (default: 2 units) - only for World Space mode
   - Adjust `Height Offset` (default: 0.2 units above camera) - only for World Space mode
   - Set `Canvas Scale` (default: 0.002 for VR, 0.003 for editor)
   
3. **Configure UI Positions** (in `UI Position Settings` section):
   - **Notification Position**: Set where collection notifications appear (default: `BottomRight`)
     - Recommended: `BottomRight` for collection notifications
   - **Timer Position**: Set where timers appear (default: `TopLeft`)
     - Recommended: `TopLeft` for timers
   - **Notification Screen Offset**: Pixel offset from screen edge for notifications (default: `20, 50`)
   - **Timer Screen Offset**: Pixel offset from screen edge for timers (default: `20, 50`)
   - **Notification Spacing**: Spacing between multiple notifications (default: `10`)
   - **Timer Spacing**: Spacing between multiple timers (default: `10`)

3. **Add PlayerFollowUIManager**:
   - Create an empty GameObject named "PlayerFollowUIManager"
   - Add the `PlayerFollowUIManager` component
   - Assign the PlayerFollowUI GameObject to the `Player Follow UI` field

## Usage Examples

### Show a Collection Notification

```csharp
// Collection notification (does NOT auto-hide by default - stays visible)
PlayerFollowUIManager.ShowCollectionNotification("Screwdriver");

// If you want it to auto-hide, specify a duration
PlayerFollowUIManager.ShowCollectionNotification("Battery", 3f); // Auto-hides after 3 seconds
```

### Show a Custom Notification

```csharp
// Show a notification with custom message and duration
PlayerFollowUIManager.ShowNotification("Game Saved!", 2f);

// With icon (if you have a sprite)
PlayerFollowUIManager.ShowNotification("Item Collected!", 3f, iconSprite);
```

### Show a Timer

```csharp
// Show a countdown timer
PlayerFollowUIManager.ShowTimer("Hiding in", 10f, () => {
    Debug.Log("Timer complete!");
    // Do something when timer ends
});

// Simple hide timer
PlayerFollowUIManager.ShowHideTimer(5f, () => {
    // Hide something after 5 seconds
});
```

### Direct Access (Alternative)

If you have a reference to the PlayerFollowUI component:

```csharp
PlayerFollowUI ui = FindObjectOfType<PlayerFollowUI>();
ui.ShowNotification("Custom Message", 3f);
ui.ShowTimer("Countdown", 10f);
```

## Configuration Options

### PlayerFollowUI Component

#### Target & Follow Settings
- **Target Camera**: The camera/head transform to follow (auto-found if null)
- **Distance From Camera**: How far in front of camera (default: 2) - only for World Space mode
- **Height Offset**: Vertical offset from camera (default: 0.2) - only for World Space mode
- **Billboard Mode**: Should UI always face camera? (default: true) - only for World Space mode
- **Follow Smoothing**: Smoothing speed for movement (default: 10) - only for World Space mode

#### Canvas Settings
- **Canvas Scale**: Scale for VR UI (default: 0.002)
- **Non-VR Canvas Scale**: Scale for editor/simulator mode (default: 0.003)

**Note**: The system automatically detects if you're playing in the editor (MacBook) vs VR mode and adjusts the canvas scale accordingly. The canvas uses Screen Space - Overlay mode by default, which doesn't require world positioning.

#### UI Position Settings
- **Notification Position**: Where notifications appear on screen
  - Options: `TopLeft`, `TopCenter`, `TopRight`, `MiddleLeft`, `MiddleCenter`, `MiddleRight`, `BottomLeft`, `BottomCenter`, `BottomRight`
  - **Recommended**: `BottomRight` for collection notifications
- **Timer Position**: Where timers appear on screen
  - Same options as Notification Position
  - **Recommended**: `TopLeft` for timers
- **Notification Screen Offset**: Pixel offset from screen edge for notifications (X, Y)
  - Example: `(20, 50)` = 20px from left/right, 50px from top/bottom
- **Timer Screen Offset**: Pixel offset from screen edge for timers (X, Y)
  - Example: `(20, 50)` = 20px from left/right, 50px from top/bottom
- **Notification Spacing**: Spacing between multiple notifications in pixels (default: 10)
- **Timer Spacing**: Spacing between multiple timers in pixels (default: 10)
- **Notification Size**: Size of notification UI elements in pixels (width, height) (default: 300, 80)
  - Adjust if notifications are too big or too small
  - Example: `(250, 60)` for smaller notifications, `(400, 100)` for larger
- **Timer Size**: Size of timer UI elements in pixels (width, height) (default: 250, 70)
  - Adjust if timers are too big or too small
- **Notification Font Size**: Font size for notification text (default: 0 = auto-calculate)
  - Set to `0` to auto-calculate based on notification size
  - Set to a specific value (e.g., `18`, `24`, `30`) to use that font size
  - Recommended: `18-24` for most cases
- **Timer Font Size**: Font size for timer text (default: 0 = auto-calculate)
  - Set to `0` to auto-calculate based on timer size
  - Set to a specific value (e.g., `20`, `26`, `32`) to use that font size
  - Recommended: `20-28` for most cases

**Example Setup**:
- Notifications at `BottomRight` with offset `(20, 50)` - appears 20px from right, 50px from bottom
- Notification Size: `(250, 60)` - smaller notifications that fit better in corner
- Timers at `TopLeft` with offset `(20, 50)` - appears 20px from left, 50px from top
- Timer Size: `(200, 50)` - compact timers

**Adjusting Notification Size and Text**:
If notifications are too big and getting cut off (especially at BottomRight):
1. Select `PlayerFollowUI` in Hierarchy
2. In Inspector, find **UI Position Settings**
3. Set **Notification Size** to smaller values, e.g.:
   - `(250, 60)` - Medium size
   - `(200, 50)` - Small size
   - `(300, 80)` - Default size
4. Adjust **Notification Font Size**:
   - Set to `0` for auto-sizing (recommended)
   - Or set a specific size like `18`, `20`, `24` for manual control
   - Smaller notifications work well with `16-20`
   - Larger notifications work well with `24-30`

**Adjusting Timer Size and Text**:
1. Set **Timer Size** as needed (default: `250, 70`)
2. Adjust **Timer Font Size**:
   - Set to `0` for auto-sizing (recommended)
   - Or set a specific size like `20`, `24`, `28` for manual control

### UINotification Component

Each notification can be customized:
- **Fade In Duration**: How long to fade in (default: 0.3s)
- **Fade Out Duration**: How long to fade out (default: 0.3s)
- **Fade Curve**: Animation curve for fade effect

## Integration with Existing Systems

The system is already integrated with:
- ✅ **ScrewdriverPickup**: Shows "Collected: Screwdriver" notification
- ✅ **BatteryPickup**: Shows "Collected: Battery" notification

To add notifications to other pickup scripts, simply add:

```csharp
PlayerFollowUIManager.ShowCollectionNotification("Item Name");
```

## Custom Notification Prefabs

You can create custom notification prefabs for better styling:

1. Create a UI prefab with:
   - Background Image
   - TextMeshProUGUI for message
   - Optional: Image for icon
   - UINotification component

2. Assign the prefab to `Notification Prefab` in PlayerFollowUI

3. The system will use your custom prefab instead of the default

## Custom Timer Prefabs

Similarly, create timer prefabs:

1. Create a UI prefab with:
   - Background Image
   - TextMeshProUGUI for timer display
   - UINotification component

2. Assign to `Timer Prefab` in PlayerFollowUI

## Tips

- **Separate Positioning**: Notifications and timers can be positioned independently
  - Set notifications to `BottomRight` for collection feedback
  - Set timers to `TopLeft` for countdown displays
- **Multiple Notifications**: Multiple notifications stack vertically automatically from their anchor point
  - Top positions stack downward
  - Bottom positions stack upward
- **Timer Colors**: Timers change color as time runs out (white → yellow → red)
- **Performance**: The system uses object pooling concepts - notifications are destroyed after use
- **XR Compatibility**: Works with both VR and non-VR setups
- **Screen Space Overlay**: The canvas uses Screen Space - Overlay mode, so it's always visible regardless of camera position

## Troubleshooting

**UI not appearing?**
- Check that PlayerFollowUIManager exists in the scene
- Verify the camera is assigned correctly
- Check canvas scale (should be very small for VR: 0.001-0.002, larger for editor: 0.01-0.02)
- In editor/MacBook mode, the UI scale is automatically increased for visibility

**UI not following player?**
- Ensure Target Camera is assigned (auto-found if using XR Origin)
- Check that the camera has the "MainCamera" tag
- Verify Follow Smoothing is not too low
- The system will retry finding the camera if XR initializes late

**Playing on MacBook/Editor (Non-VR)?**
- The system automatically detects editor mode and uses larger UI scale
- Camera detection works with XR Device Simulator
- If camera isn't found, check console for debug messages
- You can manually assign the camera in the inspector if needed

**Notifications not showing?**
- Check console for warnings
- Ensure PlayerFollowUIManager.Instance is not null
- Verify the UI canvas is active
- Check that camera was found (see console logs)

## Example Use Cases

1. **Item Collection**: Already integrated with pickups
   - Shows at `BottomRight` (configurable)
   - Example: "Collected: Screwdriver"
   
2. **Save Game Feedback**: Show "Game Saved!" notification
   - Appears at notification position (default: `BottomRight`)
   
3. **Countdown Timers**: Show "Hiding in 10s" when player needs to hide
   - Appears at timer position (default: `TopLeft`)
   - Changes color as time runs out
   
4. **Quest Updates**: "Objective Complete!"
   - Uses notification position
   
5. **Warning Messages**: "Low Battery!" or "Enemy Nearby!"
   - Uses notification position

## Recommended Setup Example

For a typical game setup:
- **Notifications** (collection, quests, warnings): `BottomRight` with offset `(20, 50)`
- **Timers** (countdowns, hide timers): `TopLeft` with offset `(20, 50)`

This keeps important countdowns visible at the top while collection feedback appears at the bottom, out of the way.

## API Reference

### PlayerFollowUIManager Static Methods

- `ShowNotification(string message, float duration, Sprite icon)` - Show a notification
  - `duration`: Auto-hide after this many seconds. Use `0` to never auto-hide (default: 3f)
- `ShowCollectionNotification(string itemName, float duration)` - Show collection notification
  - **Does NOT auto-hide by default** (duration = 0)
  - Set `duration > 0` if you want it to auto-hide
- `ShowTimer(string label, float duration, Action onComplete)` - Show countdown timer
  - Auto-hides when timer reaches 0
- `ShowHideTimer(float duration, Action onComplete)` - Show hide timer

### UINotification Methods

- `Setup(string message, float duration, Sprite icon)` - Setup as notification
- `SetupTimer(string label, float duration, Action onComplete)` - Setup as timer
- `Hide()` - Manually hide notification
- `UpdateMessage(string newMessage)` - Update notification text
- `GetRemainingTime()` - Get remaining time (for timers)

