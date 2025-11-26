# Portal Index Verification Guide

## Problem: Portal Always Saves as Index 0

If no matter which portal you use, it always saves as 0, check these:

## ✅ Portal Setup Verification

### In MainScene (Should have 3 portals):

**Portal 0:**
- [ ] GameObject name: "Portal_0" (or any name)
- [ ] Portal script → **Portal Index** = `0` (NOT -1, NOT 1, NOT 2)
- [ ] Box Collider → Is Trigger = ✅

**Portal 1:**
- [ ] GameObject name: "Portal_1" (or any name)
- [ ] Portal script → **Portal Index** = `1` (NOT 0, NOT -1, NOT 2)
- [ ] Box Collider → Is Trigger = ✅

**Portal 2:**
- [ ] GameObject name: "Portal_2" (or any name)
- [ ] Portal script → **Portal Index** = `2` (NOT 0, NOT 1, NOT -1)
- [ ] Box Collider → Is Trigger = ✅

### In GameManagerScene (Should have 1 portal):

**Return Portal:**
- [ ] Portal script → **Portal Index** = `-1` (or any value, doesn't matter)
- [ ] Box Collider → Is Trigger = ✅

## 🔍 How to Check Portal Index

1. **Select the Portal GameObject** in Hierarchy
2. **Look at Inspector** → Find "Portal" component
3. **Check "Portal Index" field** - Should be exactly 0, 1, or 2 for MainScene portals

## 🐛 Common Issues

### Issue 1: All Portals Have Same Index

**Problem:** All portals have index 0

**Fix:**
- Select Portal 1 → Set Portal Index to `1`
- Select Portal 2 → Set Portal Index to `2`
- Verify each portal has a unique index (0, 1, 2)

### Issue 2: Portal Index Not Set in Inspector

**Problem:** Portal Index field is 0 by default, but you didn't change it

**Fix:**
- Manually set each portal's index in Inspector
- Don't rely on default values

### Issue 3: Multiple Portals Triggering

**Problem:** Multiple portals might be overlapping or too close

**Fix:**
- Make sure portals are far enough apart
- Check that only one portal's trigger is being entered
- Look at console logs to see which portal is actually triggering

## 📋 Testing with Debug Logs

With the new debug logging, when you enter a portal, you should see:

```
[PORTAL] Player entered trigger of portal: Portal_1, Portal Index: 1
[PORTAL DEBUG] EnterPortal called on GameObject: Portal_1, Portal Index: 1, Current Scene: MainScene
[PORTAL DEBUG] MainScene portal detected! Index: 1, GameObject: Portal_1
[PORTAL] Portal 1 in MainScene - saving index and going to GameManagerScene
[PORTAL MANAGER] SetLastUsedPortal called with index: 1
[PORTAL MANAGER] ✓ Portal 1 was used to enter GameManager scene (saved in memory). LastUsedPortalIndex is now: 1
[PORTAL] ✓ Portal index 1 saved to file immediately. Save data now has: 1
```

**If you see index 0 when entering Portal 1, the problem is:**
- Portal 1's Portal Index field is set to 0 in Inspector
- OR Portal 0 is triggering instead of Portal 1

## ✅ Quick Fix Steps

1. **Select Portal 1 GameObject** in MainScene
2. **In Inspector**, find "Portal" component
3. **Change "Portal Index" from 0 to 1**
4. **Do the same for Portal 2** (set to 2)
5. **Test again** and check console logs

## 🔍 Verify in Console

When you enter Portal 1, the console should show:
- `Portal Index: 1` (NOT 0)
- `Portal 1 in MainScene`
- `Portal index 1 saved to file`

If it shows `Portal Index: 0` when entering Portal 1, then Portal 1's index is incorrectly set to 0 in the Inspector.

## 📝 PortalManager Setup

Make sure PortalManager has all 3 portals assigned:

1. **Select PortalManager** in MainScene
2. **In Inspector**, find "Portal Manager" component
3. **Main Scene Portals** array should have:
   - [0] = Portal_0 (with index 0)
   - [1] = Portal_1 (with index 1)
   - [2] = Portal_2 (with index 2)

If portals aren't assigned, PortalManager will auto-find them, but it's better to assign them manually.

---

**The key is: Each portal's "Portal Index" field in the Inspector must match the portal number (0, 1, or 2)!**

