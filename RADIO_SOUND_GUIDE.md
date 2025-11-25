## Radio Sound Setup Guide (Trigger-Based Toggle)

Follow these steps to set up the radio sound effect so it behaves like the flashlight (requires battery, toggle via input while near the radio).

---

### 1. Scene Hierarchy & Components
1. **Radio GameObject** (the visible radio mesh/prefab)
   - Ensure it has:
     - `AudioSource` (the clip will go here)
     - `RadioController`
     - `RadioInput`
2. **Trigger Child** (or a separate GameObject)
   - Add a collider (`BoxCollider`/`SphereCollider`, etc.)
   - Enable **Is Trigger**
   - Add `RadioTriggerZone`
   - Assign the `RadioController`

---

### 2. Configure the AudioSource
1. Select the Radio GameObject.
2. In the `AudioSource`:
   - Drag your radio loop/clip into **Audio Clip**.
   - Enable **Loop** (if you always want continuous noise). `RadioController` can also enforce looping.
   - Set **Spatial Blend** to `1 (3D)` if you want positional audio.
   - Adjust **Min Distance** (full volume range) and **Max Distance** (where it becomes inaudible).
3. In `RadioController`, make sure the `Radio Audio Source` field is linked to this AudioSource (auto-filled if on the same object).

---

### 3. Input Action
1. On `RadioInput`, assign:
   - `Radio`: reference to the `RadioController`.
   - `Toggle Action`: choose an existing `InputActionProperty` (e.g., the same one used for flashlight) or create a new action in your Input Actions asset.
2. The radio now listens for that action while the player is inside the trigger volume.

---

### 4. Trigger Zone
1. On the collider with `RadioTriggerZone`:
   - `Radio`: drag the `RadioController`.
   - `Player Tag`: leave as `Player` (matches XR Origin tag) or change if needed.
   - `Auto Power Off On Exit`: checked by default so the radio fades out when the player leaves.
2. Resize the collider so the player can get close enough before toggling.

---

### 5. Battery Requirement
1. Open the battery prefab / object that the player picks up (`BatteryPickup`).
2. In the inspector:
   - `Flashlight`: already assigned.
   - `Radios`: click the **+** icon and drag in all `RadioController` references that should be powered by this battery.
3. Result: when the battery is picked up, both flashlight and radio get `hasBattery = true`.

---

### 6. Quick Checklist
| Item | Where | Notes |
|------|-------|-------|
| Audio Clip assigned | Radio → `AudioSource` | Drag clip into `Audio Clip` |
| RadioController configured | Radio | `Radio Audio Source`, fade times, target volume |
| RadioInput action | Radio | `InputActionProperty` assigned |
| Trigger collider | Radio child / nearby object | Collider set to `Is Trigger`, `RadioTriggerZone` attached |
| Battery powers radio | Battery prefab | `Radios` list populated |
| Player tag | XR Origin | Should be `Player` (or match `RadioTriggerZone.playerTag`) |

---

### 7. Testing
1. Enter Play Mode.
2. Pick up the battery (battery object disappears).
3. Walk into the radio’s trigger zone; your console can log `RadioController: playerInRange = true`.
4. Press the toggle input → radio audio fades in.
5. Walk out of the trigger (if auto power-off enabled) → audio fades out.
6. Try toggling before grabbing the battery or outside the trigger: nothing happens (expected).

---

### 8. Optional Tweaks
- **On-screen prompt**: add a UI canvas that becomes visible when `RadioTriggerZone` detects the player.
- **Distraction mechanic**: have `KidnapperAI` listen for radio toggles to investigate.
- **Multiple clips**: script can be extended to cycle through stations or static.

You now have a fully functional radio sound effect that behaves just like the flashlight—battery-gated, proximity-based, and input-toggled. 🎧📻

