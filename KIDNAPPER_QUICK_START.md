# Kidnapper Bot - Quick Start Guide

## 🚀 Quick Setup (5 Minutes)

### Step 1: Get Animations (2 min)
1. Go to [Mixamo.com](https://www.mixamo.com) and sign in
2. Download these animations as **FBX for Unity**:
   - "Idle" (idle animation)
   - "Walking" (walk animation)
   - "Running" (run animation)
   - "Punching" or "Kicking" (attack animation)

### Step 2: Import to Unity (1 min)
1. Create folder: `Assets/Animations/Kidnapper/`
2. Drag all FBX files into this folder
3. For each animation, select it → Inspector → **Rig** tab → Set to **Humanoid** → Apply

### Step 3: Create Animator Controller (1 min)
1. Right-click in `Assets/Animations/Kidnapper/` → Create → **Animator Controller**
2. Name it `KidnapperAnimatorController`
3. Open it, add states: `Idle`, `Walk`, `Run`, `Attack`
4. Assign animations to each state
5. Add parameter: `Speed` (Float)
6. Create transitions:
   - Idle ↔ Walk (Speed > 0.1)
   - Walk ↔ Run (Speed > 3)

### Step 4: Set Up Kidnapper in Scene (1 min)
1. Create empty GameObject → Name it "Kidnapper"
2. Add Component → **KidnapperSetupHelper**
3. Drag your NPC model into **Character Model** field
4. Drag your Animator Controller into **Animator Controller** field
5. Right-click the component → **Setup Kidnapper** (or click button in Inspector)
6. Right-click component → **Create Patrol Points** (creates 3 default points)

### Step 5: Bake NavMesh (30 sec)
1. Select all floor/ground objects → Check **Navigation Static** in Inspector
2. Window → AI → Navigation → **Bake** tab → Click **Bake**

### Step 6: Test!
Press Play and watch your kidnapper patrol!

---

## 📋 Component Checklist

- [ ] NPC Character Model (from npc_casual_set_00)
- [ ] Animations from Mixamo (Idle, Walk, Run, Attack)
- [ ] Animator Controller with animations assigned
- [ ] Kidnapper GameObject with:
  - [ ] NavMesh Agent
  - [ ] Capsule Collider
  - [ ] KidnapperAI script
- [ ] Character Model child with:
  - [ ] Animator component
  - [ ] Animator Controller assigned
- [ ] Patrol Points (empty GameObjects)
- [ ] NavMesh baked
- [ ] Player tagged as "Player"

---

## 🎮 Script Features

The **KidnapperAI** script includes:
- ✅ Patrol system (moves between patrol points)
- ✅ Player detection (range + field of view + line of sight)
- ✅ Chase behavior (follows player when detected)
- ✅ Attack behavior (attacks when close)
- ✅ Search behavior (searches last known position if player escapes)
- ✅ Animation integration (automatically updates animations)
- ✅ Gizmos visualization (see detection range, FOV, patrol path in Scene view)

---

## ⚙️ Configuration Tips

### Make Kidnapper More Aggressive:
- Increase **Detection Range** (10 → 15)
- Increase **Field Of View** (90 → 120)
- Increase **Chase Speed** (4 → 6)

### Make Kidnapper Less Aggressive:
- Decrease **Detection Range** (10 → 5)
- Decrease **Field Of View** (90 → 60)
- Decrease **Patrol Speed** (2 → 1.5)

### Adjust Animation Parameters:
The script uses these parameter names by default:
- `Speed` (Float) - for walk/run blending
- `Attack` (Trigger) - for attack animation
- `Idle` (Bool) - for idle state

If your Animator Controller uses different names, update them in the KidnapperAI script.

---

## 🐛 Common Issues

**Kidnapper not moving?**
- Check NavMesh is baked (blue overlay in Scene view)
- Verify patrol points are on NavMesh
- Check NavMesh Agent component is enabled

**Animations not playing?**
- Verify Animator Controller is assigned
- Check animation parameter names match script
- Ensure Avatar is configured on character model

**Player not detected?**
- Verify Player has "Player" tag
- Check Detection Range is large enough
- Ensure no obstacles blocking line of sight

**Kidnapper falls through floor?**
- Add Rigidbody component (set to Kinematic)
- Or ensure NavMesh Agent is properly configured

---

## 📚 Full Documentation

See `KIDNAPPER_BOT_SETUP_GUIDE.md` for detailed step-by-step instructions.


