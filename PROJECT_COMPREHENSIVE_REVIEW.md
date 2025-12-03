# Unity Project Comprehensive Review
## Complete Function and Procedure Documentation

---

## Table of Contents
1. [Core Systems](#core-systems)
2. [Save/Load System](#saveload-system)
3. [Portal System](#portal-system)
4. [UI System](#ui-system)
5. [Player Systems](#player-systems)
6. [Item Systems](#item-systems)
7. [Interaction Systems](#interaction-systems)
8. [AI Systems](#ai-systems)
9. [Audio Systems](#audio-systems)
10. [Light Systems](#light-systems)
11. [Game Flow](#game-flow)

---

## Core Systems

### GameManager.cs
**Purpose**: Central game state manager, scene transitions, save/load coordination

**Key Functions**:
- `BootstrapGameManager()` - RuntimeInitializeOnLoadMethod that ensures GameManager exists before scene load
- `Awake()` - Singleton initialization, creates SaveSystem if missing
- `Start()` - Auto-finds save button, loads game data if in MainScene
- `SaveGame()` - Calls SaveSystem.SaveGame() and shows feedback
- `LoadGame()` - Calls SaveSystem.LoadGame() and applies save data
- `LoadGameOnReturn()` - Loads game when returning to MainScene (only teleports if portal was used)
- `ReturnPlayerToPortalDelayed()` - Coroutine that teleports player back to portal after scene load
- `PlayerCaught()` - Triggers game over when player is detected
- `GameOver()` - Changes state to GameOver and loads GameOverScene
- `GoToGameManagerScene()` - Transitions to GameManagerScene
- `GoToMainScene()` - Transitions to MainScene
- `PauseGame()` / `ResumeGame()` - Time scale management

**Game States**:
- `Playing` - Normal gameplay
- `Paused` - Game paused
- `GameOver` - Player caught
- `Victory` - Game won
- `InMenu` - In menu

**Events**:
- `OnGameOver` - Fired when game over
- `OnPlayerCaught` - Fired when player detected
- `OnGameSaved` - Fired when save succeeds
- `OnGameLoaded` - Fired when load succeeds

---

## Save/Load System

### SaveData.cs
**Purpose**: Serializable data structure for all game state

**Data Fields**:
- `lastUsedPortalIndex` - Which portal was used (0, 1, or 2)
- `portalPositions[]` - Positions of all 3 portals
- `playerPosition` / `playerRotation` - Player transform
- `flashlightHasBattery` / `flashlightIsOn` / `flashlightIsGrabbed` - Flashlight state
- `flashlightPosition` / `flashlightRotation` - Flashlight transform
- `batteryPickedUp` / `batteryActive` / `batteryPosition` - Battery state
- `keypadPin` / `keypadUnlocked` - Keypad state
- `enemyData` - List of EnemySaveData (position, rotation, state, patrol index, etc.)
- `ropeCut` - Whether rope/handcuff has been cut
- `exitDoorCollectedDigits[]` - Array of bools tracking which keypad digits have been found
- `saveTime` - Timestamp of save

### SaveSystem.cs
**Purpose**: Singleton that handles JSON-based save/load to persistent data path

**Key Functions**:
- `SaveGame()` - Collects all data and writes to JSON file
- `LoadGame()` - Reads JSON file and deserializes to CurrentSaveData
- `SaveFileExists()` - Checks if save file exists
- `DeleteSave()` - Deletes save file (called on game over)
- `SavePortalIndexOnly()` - Quick save of just portal index (used when entering portal)
- `CollectSaveData()` - Gathers all saveable data from scene:
  - `CollectPlayerData()` - Player position/rotation
  - `CollectPortalData()` - Portal positions and last used index
  - `CollectFlashlightData()` - Flashlight state and transform
  - `CollectBatteryData()` - Checks flashlight.hasBattery and BatteryPickup objects
  - `CollectKeypadData()` - Keypad pin and unlock state
  - `CollectEnemyData()` - All KidnapperAI states
  - `CollectRopeData()` - Checks for active "Rope" tagged objects
  - `CollectPinData()` - Gets pin states from PlayerFollowUIManager
- `ApplySaveData()` - Applies all loaded data to scene
- `LoadGameWithoutPlayerPosition()` - Applies data except player position (for scene changes)
- `ApplyRopeData()` - Enables hands and destroys rope objects if ropeCut=true
- `ApplyBatteryData()` - Restores battery state to flashlight/radios, shows UI notification
- `ApplyPinData()` - Restores pin states and always shows notification

**Save File Location**: `Application.persistentDataPath/gamesave.json`

---

## Portal System

### Portal.cs
**Purpose**: Individual portal behavior and scene transitions

**Key Functions**:
- `Start()` - Auto-detects target scene based on current scene
- `OnTriggerEnter()` - Detects player entry with cooldown protection
- `EnterPortal()` - Handles scene transition:
  - Validates scene exists in build settings
  - If in MainScene: saves portal index to PortalManager
  - If in GameManagerScene: returns to MainScene
  - Loads target scene

**Settings**:
- `portalIndex` - Unique index (0, 1, 2 for MainScene, -1 for GameManagerScene)
- `targetSceneName` - Scene to load (auto-detected if empty)
- `spawnPoint` - Optional spawn transform for player return

### PortalManager.cs
**Purpose**: Manages all portals and tracks last used portal

**Key Functions**:
- `SetLastUsedPortal()` - Records which portal was used (in-memory only)
- `LoadLastUsedPortalIndex()` - Loads portal index from save file
- `GetReturnPortal()` - Returns the portal player should return through
- `ReturnPlayerToPortal()` - Teleports player to return portal:
  - Disables CharacterController temporarily
  - Sets position to spawnPoint or offset from portal
  - Rotates player to face away from portal
  - Re-enables CharacterController
  - Uses coroutines to reinforce position
- `FindMainScenePortals()` - Auto-finds portals in MainScene
- `EnsureAllPortalsVisible()` - Keeps portals active (called in Update)

**State**:
- `lastUsedPortalIndex` - In-memory tracking (not saved to file)
- `defaultPortalIndex` - Default portal if no save exists

---

## UI System

### PlayerFollowUIManager.cs
**Purpose**: Singleton manager for UI notifications and keypad pin tracking

**Key Functions**:
- `ShowNotification()` - Static method to show a notification
- `ShowCollectionNotification()` - Shows "Collected: [item]" notification
- `ShowTimer()` - Shows countdown timer
- `ShowHideTimer()` - Shows "Hiding in X" timer
- `RegisterExitDoorPin()` - Registers that a keypad digit was found
- `RegisterExitDoorPinInternal()` - Internal pin registration, auto-saves after pin found
- `ShowExitDoorKeypadNotification()` - Shows/updates keypad pin display
- `BuildExitDoorKeypadDisplayText()` - Builds display like "Exit Door Code: 1 5 _"
- `GetCollectedDigitsState()` - Returns bool array for save system
- `RestoreCollectedDigitsState()` - Restores pin states from save, always shows notification
- `CheckAndDisplaySavedPins()` - Coroutine that waits for SaveSystem and restores pins on scene load

**Pin Tracking**:
- `exitDoorCollectedDigits[]` - Bool array tracking which digits are found
- `exitDoorKeypadCode` - Correct code (e.g., "152")
- `exitDoorHiddenCharacter` - Character for unrevealed digits (default: '_')

### PlayerFollowUI.cs
**Purpose**: Canvas that displays notifications and timers

**Key Functions**:
- `ShowNotification()` - Creates and displays a notification
- `ShowTimer()` - Creates and displays a countdown timer
- `FindCamera()` - Finds Main Camera or XR Origin camera
- `SetupCanvasForCamera()` - Configures canvas for camera
- `PositionCanvasNearCamera()` - Positions canvas in world space (if using World Space mode)
- `CreateDefaultNotification()` - Creates notification UI if no prefab assigned
- `CreateDefaultTimer()` - Creates timer UI if no prefab assigned

**Settings**:
- `notificationPosition` / `timerPosition` - Screen position (TopLeft, TopRight, etc.)
- `distanceFromCamera` - Distance in world units
- `followSmoothing` - Smoothing speed for position updates
- `billboardMode` - Always face camera

**Render Modes**:
- `ScreenSpaceOverlay` - Always visible, no camera needed (current default)
- `ScreenSpaceCamera` - Requires camera reference
- `WorldSpace` - Positioned in 3D world

### UINotification.cs
**Purpose**: Individual notification/timer UI element

**Key Functions**:
- `Setup()` - Sets up regular notification with message and duration
- `SetupTimer()` - Sets up countdown timer with label and duration
- `UpdateMessage()` - Updates notification text
- `Hide()` - Fades out and destroys notification
- `FadeIn()` - Coroutine for fade-in animation
- `FadeOut()` - Coroutine for fade-out animation
- `AutoHide()` - Coroutine that hides after duration
- `TimerCountdown()` - Coroutine that counts down and updates text
- `UpdateTimerText()` - Updates timer display with color changes

**Features**:
- Auto-fade in/out
- Color changes for timer (red < 5s, yellow < 10s, white otherwise)
- Duration = 0 means never auto-hide (persistent)

---

## Player Systems

### HandMovementToggleInspector.cs
**Purpose**: Enables/disables hand manipulation in XR

**Key Functions**:
- `InitializeActions()` - Gets InputActionReferences from XR Device Simulator
- `ApplyHandState()` - Enables/disables manipulateLeft and manipulateRight actions
- `Reinitialize()` - Re-initializes action references

**State**:
- `handsEnabled` - Public bool that controls hand manipulation
- Used by rope cutting system to enable hands after rope is cut

### PlayerFootstepSounds.cs
**Purpose**: Plays footstep sounds based on player movement speed

**Key Functions**:
- `PlayFootstep()` - Plays random footstep sound from array
- `GetCurrentSpeed()` - Returns current movement speed
- `Update()` - Tracks speed and plays footsteps:
  - Calculates speed from position or CharacterController velocity
  - Uses smoothed speed to prevent sudden stops
  - Plays walking sounds if speed >= walkingSpeedThreshold
  - Plays running sounds if speed >= runningSpeedThreshold
  - Stops sounds when speed drops below threshold

**Detection Modes**:
- `Position` - Tracks XR Origin position changes
- `Input` - Tracks CharacterController velocity
- `Both` - Uses both methods

**Settings**:
- `walkingSpeedThreshold` - Minimum speed for walking (default: 0.1)
- `runningSpeedThreshold` - Speed to switch to running (default: 2.5)
- `footstepInterval` - Time between walking footsteps (default: 0.5s)
- `runningFootstepInterval` - Time between running footsteps (default: 0.3s)
- `speedSmoothing` - Smoothing factor to prevent sudden stops (default: 0.7)

---

## Item Systems

### FlashlightController.cs
**Purpose**: Controls flashlight light and battery state

**Key Functions**:
- `ToggleFlashlight()` - Toggles light on/off (requires battery and grabbed)
- `InsertBattery()` - Sets hasBattery = true

**State**:
- `hasBattery` - Whether battery is inserted
- `isOn` - Whether light is currently on
- `isGrabbed` - Whether player is holding flashlight

### FlashlightGrabTracker.cs
**Purpose**: Tracks when flashlight is grabbed/released

**Key Functions**:
- `OnGrab()` - Sets flashlight.isGrabbed = true
- `OnRelease()` - Sets flashlight.isGrabbed = false

**Events**: Subscribes to XRGrabInteractable selectEntered/selectExited

### FlashlightInput.cs
**Purpose**: Handles input for toggling flashlight

**Key Functions**:
- `Update()` - Checks InputActionProperty and calls flashlight.ToggleFlashlight()

### BatteryPickup.cs
**Purpose**: Battery pickup item that powers flashlight and radios

**Key Functions**:
- `OnGrabbed()` - Called when battery is grabbed:
  - Shows "Collected: Battery" notification
  - Calls flashlight.InsertBattery()
  - Calls radio.InsertBattery() for all assigned radios
  - Destroys battery object after delay

**Settings**:
- `flashlight` - FlashlightController to power
- `radios[]` - Array of RadioController to power
- `disappearDelay` - Delay before battery disappears (default: 0.1s)

### RadioController.cs
**Purpose**: Manages radio audio playback with distance-based volume

**Key Functions**:
- `ToggleRadio()` - Toggles radio on/off (requires battery and player in range)
- `InsertBattery()` - Sets hasBattery = true
- `SetPlayerInRange()` - Sets playerInRange flag
- `PowerOff()` - Forces radio off with optional fade
- `IsSoundPlaying()` - Returns true if sound is actually playing
- `FadeIn()` - Coroutine that fades in audio after playDelay
- `FadeOut()` - Coroutine that fades out audio
- `UpdateDistanceMultiplier()` - Calculates volume based on distance to player
- `ApplyVolume()` - Applies fadeVolume * distanceMultiplier to audio source
- `ShowDelayTimer()` - Shows countdown timer for playDelay
- `SetEmission()` - Enables/disables ActiveLight emission when radio is on

**Settings**:
- `playDelay` - Delay before sound starts after toggle (default: 5s)
- `fadeInDuration` / `fadeOutDuration` - Fade timing
- `targetVolume` - Maximum volume (default: 0.85)
- `useDistanceFalloff` - Enable distance-based volume
- `minDistance` / `maxDistance` - Distance falloff range
- `activeLight` - GameObject with emission material for visual feedback

### RadioTriggerZone.cs
**Purpose**: Trigger zone that allows radio interaction

**Key Functions**:
- `OnTriggerEnter()` - Sets radio.SetPlayerInRange(true)
- `OnTriggerExit()` - Sets radio.SetPlayerInRange(false, autoPowerOff)

### RadioInput.cs
**Purpose**: Handles input for toggling radio

**Key Functions**:
- `Update()` - Checks InputActionProperty and calls radio.ToggleRadio()

### ScrewdriverPickup.cs
**Purpose**: Screwdriver pickup item required for power supply interaction

**Key Functions**:
- `OnGrabbed()` - Called when screwdriver is grabbed:
  - Sets static hasScrewdriver = true
  - Shows "Collected: Screwdriver" notification
  - Notifies legacy components (if assigned)
  - Destroys screwdriver after delay
- `HasScrewdriver()` - Static method to check if player has screwdriver
- `ResetScrewdriverState()` - Static method to reset state

**State**:
- `hasScrewdriver` - Static bool tracked across all instances

---

## Interaction Systems

### MetalPlateTriggerZone.cs (formerly HandTriggerZone)
**Purpose**: Handles rope cutting interaction

**Key Functions**:
- `Start()` - Checks save data and auto-cuts rope if ropeCut=true
- `CheckSaveDataAndAutoCut()` - Coroutine that waits for SaveSystem and auto-cuts
- `OnTriggerEnter()` / `OnTriggerExit()` - Tracks player in trigger
- `Update()` - Checks for input (Space key or gamepad button)
- `EnableHandsAndDestroyRope()` - Coroutine that waits delay then cuts rope
- `PerformCutInstantly()` - Immediately enables hands and destroys rope:
  - Enables hands via HandMovementToggleInspector
  - Finds rope by tag "Rope" or name "RopeHandcuff"
  - Destroys rope object
  - Optionally plays sound
- `FindRopeHandcuffInScene()` - Recursively searches for rope object
- `FindChildRecursive()` - Helper to search transform hierarchy

**Settings**:
- `handToggle` - HandMovementToggleInspector reference
- `ropeHandcuff` - Rope GameObject (auto-found if null)
- `primaryKey` - Key to press (default: Space)
- `actionDelay` - Delay before cutting (default: 1s)
- `audioSource` / `ropeCutClip` - Sound to play

### ExitDoorKeypadPinsTrigger.cs
**Purpose**: Reveals keypad pins when player views them with flashlight

**Key Functions**:
- `OnTriggerEnter()` - Detects player entry
- `OnTriggerExit()` - Detects player exit
- `Update()` - Checks if player is inside and flashlight is on
- `TryRevealPin()` - Reveals pin if conditions met:
  - Checks if already revealed
  - Checks if flashlight is on (if required)
  - Calls PlayerFollowUIManager.RegisterExitDoorPin()
  - Shows/updates keypad notification
  - Auto-saves after pin found

**Settings**:
- `pinValue` - Digit value of this pin (e.g., 1, 5, 2)
- `requireFlashlightOn` - Whether flashlight must be on (default: true)

### KeypadController.cs
**Purpose**: Manages keypad PIN entry and unlock state

**Key Functions**:
- `AddDigit()` - Adds digit to current PIN entry
- `ClearPin()` - Clears current PIN
- `CheckPin()` - Checks if entered PIN matches correct PIN
- `Unlock()` - Unlocks keypad and fires OnUnlock event
- `Lock()` - Locks keypad and fires OnLock event
- `GetCurrentPin()` - Returns current PIN being entered
- `IsUnlocked()` - Returns unlock state
- `SetPin()` - Sets PIN (for save/load)

**Settings**:
- `correctPin` - The correct PIN code (default: "1234")
- `OnUnlock` / `OnLock` - UnityEvents

### PowerSupplyTriggerZone.cs
**Purpose**: Multi-step power supply interaction

**Key Functions**:
- `OnTriggerEnter()` / `OnTriggerExit()` - Tracks player in trigger
- `Update()` - Checks for input (E key or gamepad button)
- `TryInteract()` - Performs interaction steps:
  - Step 0 → Step 1: Opens cover
  - Step 1 → Step 2: Turns off switch AND removes fuse (both together)
  - Requires screwdriver if requireScrewdriver=true
- `WaitForNextStep()` - Coroutine that waits for animation then allows next step

**Settings**:
- `coverController` - PowerSupplyCoverController
- `switchController` - PowerSupplySwitchController
- `fuseController` - PowerSupplyFuseController
- `requireScrewdriver` - Whether screwdriver is required (default: true)
- `interactionKey` - Key to press (default: E)
- `delayBetweenSteps` - Delay between steps (default: 0.5s)

### PowerSupplySwitchController.cs
**Purpose**: Controls power switch animation and light control

**Key Functions**:
- `ToggleSwitch()` - Toggles switch on/off
- `TurnOff()` - Turns switch off:
  - Animates Z rotation from -90 to 0
  - Shows countdown timer for cutElectricDelay
  - Starts coroutine to turn off lights after delay
  - Notifies kidnappers after lights are cut
- `TurnOn()` - Turns switch on:
  - Animates Z rotation from 0 to -90
  - Turns on all assigned lights
- `AnimateSwitch()` - Coroutine that smoothly rotates switch
- `TurnOffLightsWithDelay()` - Coroutine that waits delay then turns off lights
- `TurnOffLights()` - Disables all assigned lights
- `TurnOnLights()` - Enables all assigned lights
- `ShowDelayTimer()` - Shows countdown timer for cutElectricDelay

**Settings**:
- `onAngle` / `offAngle` - Z rotation angles (default: -90 / 0)
- `animationDuration` - Switch animation time (default: 0.8s)
- `cutElectricDelay` - Delay before lights turn off (default: 0.5s)
- `lightsToTurnOff[]` - Direct Light component references
- `lightObjectsToTurnOff[]` - GameObjects with Light components
- `searchChildrenForLights` - Search children for lights (default: true)
- `powerRestoreDelay` - How long kidnapper waits before restoring (default: 300s)
- `kidnappers[]` - KidnapperAI agents to notify
- `autoNotifyKidnappersOnPowerCut` - Auto-notify on power cut (default: true)

### SaveTriggerZone.cs
**Purpose**: Trigger zone that saves game when player presses key

**Key Functions**:
- `OnTriggerEnter()` / `OnTriggerExit()` - Tracks player in trigger
- `Update()` - Checks for input (E key or gamepad button)
- `TriggerSave()` - Calls GameManager.SaveGame()
- `PlaySaveSound()` - Plays save sound if assigned

**Settings**:
- `interactionKey` - Key to press (default: E)
- `allowGamepadButton` - Allow gamepad input (default: true)
- `audioSource` / `saveSound` - Sound to play

### SaveButtonHandler.cs
**Purpose**: UI Button handler that connects to GameManager.SaveGame()

**Key Functions**:
- `Start()` - Finds GameManager and connects button.onClick to SaveGame()

---

## AI Systems

### KidnapperAI.cs
**Purpose**: Advanced AI with patrolling, chasing, attacking, sound investigation, and power restoration

**Key Functions**:
- `Start()` - Initializes NavMeshAgent, finds player, sets up sound detection
- `Update()` - Main update loop:
  - Checks for player detection (highest priority)
  - Checks for sounds (if not chasing/attacking/restoring power)
  - Updates state machine
  - Updates animations
- `CheckForPlayer()` - Visual detection with FOV and line-of-sight:
  - Checks distance and angle (FOV)
  - Uses RaycastAll to check for walls blocking line of sight
  - Walls ALWAYS block detection, even if very close
  - Triggers game over on first detection
  - Switches to Chase or Attack state based on distance
- `CheckForSounds()` - Detects player footsteps and radio sounds:
  - Footsteps: detects when player is running within footstepHearingRange
  - Radio: detects when radio is playing within radioHearingRange
  - Switches to InvestigatingSound state
  - Updates sound source position (for moving sounds like footsteps)
  - Handles investigation timeout based on sound type
- `Patrol()` - Moves between patrol points:
  - Waits at each point for patrolWaitTime
  - Cycles through patrol points
- `Chase()` - Chases player:
  - Updates path frequently for smooth following
  - Checks NavMesh for player position
  - Switches to Attack if in attack range
  - Uses lastKnownPlayerPosition if player lost
- `Attack()` - Attacks player:
  - Stops agent
  - Looks at player
  - Triggers attack animation
  - Switches back to Chase if player moves too far
- `InvestigateSound()` - Moves toward sound source:
  - For radio: goes directly to audio source
  - For footsteps: maintains safeDistance to prevent pushing
  - Updates path frequently
  - Stops when reached sound location
  - Looks around at sound location
- `Search()` - Searches last known player position
- `OnPowerCut()` - Called when power is cut:
  - Switches to RestoringPower state
  - Moves to power switch location
  - Waits for powerRestoreDelay
  - Turns switch back on
  - Returns to patrolling
- `RestorePower()` - State logic for power restoration:
  - Moves to power switch
  - Shows countdown timer to player
  - Waits at switch for powerRestoreDelay
  - Turns switch on
  - Returns to patrol
- `UpdateAnimations()` - Updates animator parameters (Speed, Idle, Attack)

**States**:
- `Patrolling` - Moving between patrol points
- `Chasing` - Chasing detected player
- `Attacking` - Attacking player in range
- `Searching` - Searching last known position
- `InvestigatingSound` - Moving toward sound source
- `RestoringPower` - Going to power switch to restore power

**Settings**:
- `patrolSpeed` / `chaseSpeed` - Movement speeds
- `detectionRange` - Visual detection range
- `attackRange` - Attack range
- `fieldOfView` - FOV angle (default: 120 degrees)
- `detectFromAllAngles` - 360-degree detection (default: false)
- `footstepHearingRange` - Footstep sound range (default: 15m)
- `radioHearingRange` - Radio sound range (default: 25m)
- `canInvestigateRadio` - Whether to investigate radio sounds (default: true)
- `canRestorePower` - Whether to restore power when cut (default: true)
- `powerRestoreWaitTime` - How long to wait at switch (default: 300s)
- `safeDistance` - Distance to maintain from targets (default: 2.5m)
- `patrolPoints[]` - Array of patrol point transforms

**Save/Load Methods**:
- `GetCurrentPatrolIndex()` - Returns current patrol point index
- `GetLastKnownPlayerPosition()` - Returns last known player position
- `IsPlayerDetected()` - Returns detection state
- `SetCurrentPatrolIndex()` - Sets patrol index
- `SetLastKnownPlayerPosition()` - Sets last known position
- `SetPlayerDetected()` - Sets detection state

---

## Audio Systems

### PlayerFootstepSounds.cs
(See Player Systems section)

### KidnapperFootstepSounds.cs
**Purpose**: Plays footstep sounds for kidnapper AI (similar to PlayerFootstepSounds)

---

## Light Systems

### ToggleLight.cs
**Purpose**: Simple light toggle component

**Key Functions**:
- `Toggle()` - Flips light on/off
- `TurnOn()` - Turns light on
- `TurnOff()` - Turns light off
- `Set()` - Explicitly sets light state

**Settings**:
- `targetLight` - Light component (auto-found if null)
- `searchInChildren` - Search children for light (default: true)

### GlichedLight.cs
**Purpose**: Light that randomly turns on/off with per-instance timings

**Key Functions**:
- `LightRoutine()` - Coroutine that cycles light:
  - Turns on for random duration (minOnTime to maxOnTime)
  - Turns off for fixed duration (offTime)
  - Repeats forever
  - Optional initial jitter to desync multiple lights

**Settings**:
- `minOnTime` / `maxOnTime` - Random on duration range (default: 5-10s)
- `offTime` - Fixed off duration (default: 3s)
- `maxInitialJitter` - Random start delay (default: 0.5s)

---

## Game Flow

### Initialization Flow
1. `GameManager.BootstrapGameManager()` runs before scene load
2. `GameManager.Awake()` initializes singleton and creates SaveSystem
3. `SaveSystem.Awake()` initializes singleton and creates SaveData
4. `SaveSystem.Start()` auto-loads save file if exists
5. `GameManager.Start()` loads game data if in MainScene
6. `PlayerFollowUIManager.Awake()` initializes UI system
7. `PortalManager.Start()` finds portals in MainScene
8. `MetalPlateTriggerZone.Start()` checks save data and auto-cuts rope if needed

### Save Flow
1. Player collects items (battery, screwdriver, pins) → Auto-saves immediately
2. Player cuts rope → Saved on next save button click
3. Player enters portal → Saves portal index only (quick save)
4. Player clicks save button → Full save of all game state
5. Save file written to `Application.persistentDataPath/gamesave.json`

### Load Flow
1. `SaveSystem.Start()` auto-loads save file
2. On scene change, `GameManager` subscribes to `SceneManager.sceneLoaded`
3. `OnSceneLoaded()` callback loads save data
4. `SaveSystem.ApplySaveData()` or `LoadGameWithoutPlayerPosition()` applies data:
   - Rope cut state → Enables hands, destroys rope
   - Battery state → Restores to flashlight/radios, shows UI
   - Pin states → Restores to PlayerFollowUIManager, shows UI
   - Flashlight state → Restores position/rotation if grabbed
   - Enemy states → Restores positions and AI states
5. If portal was used, player is teleported back to portal

### Game Over Flow
1. `KidnapperAI.CheckForPlayer()` detects player with line of sight
2. Calls `GameManager.PlayerCaught()`
3. `GameManager.GameOver()` changes state and loads GameOverScene
4. `GameOverController.Start()` deletes save file
5. `GameOverController` shows countdown timer (8 seconds default)
6. After countdown, `RestartGame()` loads MainScene

### Portal Flow
1. Player enters portal in MainScene
2. `Portal.OnTriggerEnter()` detects player
3. `Portal.EnterPortal()` saves portal index to PortalManager (in-memory)
4. `SaveSystem.SavePortalIndexOnly()` saves portal index to file
5. Scene loads GameManagerScene
6. Player clicks save button (optional)
7. Player enters return portal in GameManagerScene
8. Scene loads MainScene
9. `GameManager.LoadGameOnReturn()` loads save data
10. `PortalManager.ReturnPlayerToPortal()` teleports player to correct portal

### Rope Cutting Flow
1. Player enters MetalPlateTriggerZone
2. Player presses Space (or gamepad button)
3. Sound plays immediately
4. After `actionDelay`, `PerformCutInstantly()` is called:
   - Enables hands via HandMovementToggleInspector
   - Destroys rope object
5. On next save, `ropeCut` is saved as true
6. On scene load, `MetalPlateTriggerZone.Start()` checks save data
7. If `ropeCut=true`, automatically cuts rope without sound

### Battery Collection Flow
1. Player grabs battery with XR grab (G key)
2. `BatteryPickup.OnGrabbed()` is called:
   - Shows "Collected: Battery" notification
   - Calls `flashlight.InsertBattery()`
   - Calls `radio.InsertBattery()` for all radios
   - Destroys battery object
3. On save, `CollectBatteryData()` checks:
   - `flashlight.hasBattery` (primary check)
   - Active `BatteryPickup` objects (secondary check)
   - Sets `batteryPickedUp = true` if flashlight has battery OR no active pickups
4. On load, `ApplyBatteryData()`:
   - Restores battery state to flashlight and radios
   - Shows UI notification
   - Destroys any active battery pickups if already collected

### Pin Collection Flow
1. Player enters ExitDoorKeypadPinsTrigger
2. Player turns on flashlight (if required)
3. `TryRevealPin()` is called:
   - Registers pin with PlayerFollowUIManager
   - Shows/updates keypad notification
   - Auto-saves immediately after pin found
4. On save, `CollectPinData()` gets pin states from PlayerFollowUIManager
5. On load, `ApplyPinData()`:
   - Restores pin states to PlayerFollowUIManager
   - Always shows pin notification (even if no pins found)

### Power Supply Flow
1. Player collects screwdriver (required)
2. Player enters PowerSupplyTriggerZone
3. First E press: Opens cover
4. Second E press: Turns off switch AND removes fuse (both together)
5. `PowerSupplySwitchController.TurnOff()`:
   - Animates switch rotation
   - Shows countdown timer for cutElectricDelay
   - After delay, turns off all assigned lights
   - Notifies kidnappers to restore power
6. Kidnapper receives `OnPowerCut()`:
   - Switches to RestoringPower state
   - Moves to power switch
   - Shows countdown timer to player
   - Waits for powerRestoreDelay (default: 300s)
   - Turns switch back on
   - Returns to patrolling

---

## Key Design Patterns

### Singleton Pattern
- `GameManager` - Single instance, DontDestroyOnLoad
- `SaveSystem` - Single instance, DontDestroyOnLoad
- `PortalManager` - Single instance, DontDestroyOnLoad
- `PlayerFollowUIManager` - Single instance, DontDestroyOnLoad

### Event-Driven Architecture
- `GameManager` events (OnGameOver, OnPlayerCaught, OnGameSaved, OnGameLoaded)
- XR Interaction events (selectEntered, selectExited)
- UnityEvents for keypad (OnUnlock, OnLock)

### Coroutine-Based Delays
- Player teleportation (waits for scene initialization)
- Rope cutting (waits for actionDelay)
- Radio sound delay (waits for playDelay)
- Power cut delay (waits for cutElectricDelay)
- Light animations (smooth transitions)

### State Machines
- `GameManager.GameState` (Playing, Paused, GameOver, Victory, InMenu)
- `KidnapperAI.AIState` (Patrolling, Chasing, Attacking, Searching, InvestigatingSound, RestoringPower)

---

## File Locations

### Save File
- Path: `Application.persistentDataPath/gamesave.json`
- Format: JSON (using JsonUtility)
- Auto-loaded on game start
- Auto-saved when pins found
- Manual save via save button or trigger zone

### Scenes
- `MainScene` - Main game scene with 3 portals
- `GameManagerScene` - Save/load hub with 1 return portal
- `GameOverScene` - Game over screen with countdown

---

## Dependencies

### Unity Packages
- XR Interaction Toolkit
- Input System
- TextMesh Pro
- NavMesh (for AI)

### Tags Required
- "Player" - Player GameObject
- "Rope" - Rope/handcuff objects
- "MainCamera" - Main camera (optional)

### Layers Required
- "Wall" - For line-of-sight blocking (optional, can use tag)

---

## Notes

1. **Save System**: Uses JSON serialization with JsonUtility. Save file is human-readable.

2. **Portal System**: Portal index is saved in-memory (PortalManager) AND to file. In-memory is used for immediate scene transitions, file is used for persistence.

3. **Rope Cutting**: Rope is destroyed, not just disabled. This ensures it doesn't reappear.

4. **Battery State**: Battery state is determined by checking `flashlight.hasBattery` first, then checking for active `BatteryPickup` objects. This ensures accurate state even when saving from GameManagerScene.

5. **Pin Collection**: Pins are auto-saved immediately when found. They're also saved when save button is clicked.

6. **Player Teleportation**: CharacterController is temporarily disabled during teleportation to allow position changes.

7. **AI Detection**: Walls ALWAYS block line of sight, even if player is very close. This prevents detection through walls.

8. **Sound Investigation**: Radio sounds take priority over footsteps. Kidnapper will switch from footstep to radio investigation if radio is closer.

9. **Power Restoration**: Kidnapper shows a countdown timer to the player while waiting at the power switch, giving the player time to escape.

10. **UI Notifications**: Notifications with duration=0 never auto-hide (persistent). Used for collection notifications and keypad pin display.

---

## End of Document

