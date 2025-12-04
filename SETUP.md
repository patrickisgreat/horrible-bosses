# Horrible Bosses - Unity Setup Guide

## Quick Start

### 1. Install Unity
1. Download [Unity Hub](https://unity.com/download)
2. Install **Unity 2022.3 LTS** (or newer) with:
   - Windows/Mac/Linux Build Support
   - Visual Studio (Windows) or Visual Studio Code

### 2. Create Project
1. Open Unity Hub → New Project
2. Select **3D (URP)** template
3. Name: `HorribleBosses`
4. Copy all files from this repo's `Assets/` folder into your Unity project's `Assets/` folder

### 3. Install Required Packages
Open **Window → Package Manager** and install:
- **TextMeshPro** (should prompt automatically)
- **Input System** (optional, for new input)

---

## Recommended Free Asset Packs

### ESSENTIAL - Character Models & Animations

#### From Unity Asset Store (FREE):
1. **[Starter Assets - Third Person Controller](https://assetstore.unity.com/packages/essentials/starter-assets-third-person-character-controller-196526)**
   - Good base character rig
   - Basic animations included

2. **[Mixamo Characters](https://www.mixamo.com/)** (FREE with Adobe account)
   - Download character: "Business Man" or "Business Woman"
   - Download animations:
     - Idle
     - Walk / Run
     - Attack (punch, swing)
     - Take Damage / Hit Reaction
     - Death
     - Taunt / Yell
   - Export as FBX for Unity

3. **[Polygon Starter Pack](https://assetstore.unity.com/packages/3d/props/polygon-starter-pack-low-poly-3d-art-by-synty-156819)** (FREE)
   - Low-poly characters
   - Office props

### ESSENTIAL - Office Environment

1. **[Low Poly Office Pack](https://assetstore.unity.com/packages/3d/environments/low-poly-office-pack-118516)** (FREE)
   - Desks, chairs, computers
   - Cubicles, meeting rooms
   - Office props

2. **[Simple Office Interiors](https://assetstore.unity.com/packages/3d/environments/simple-office-interiors-cartoon-assets-75662)** (FREE or similar)
   - Alternative office pack

### ESSENTIAL - Weapons & Effects

1. **[War FX](https://assetstore.unity.com/packages/vfx/particles/war-fx-5669)** (FREE)
   - Muzzle flashes
   - Bullet impacts
   - Explosions

2. **[Low Poly Weapons](https://assetstore.unity.com/packages/3d/props/weapons/low-poly-weapons-71680)** (FREE)
   - Pistol, shotgun models

### ESSENTIAL - Audio

1. **[Universal Sound FX](https://assetstore.unity.com/packages/audio/sound-fx/universal-sound-fx-17256)** (FREE)
   - Weapon sounds
   - Impact sounds
   - UI sounds

2. **FREE Music** - Search Asset Store for action/combat music

### NICE TO HAVE - UI

1. **[Clean & Minimalist GUI Pack](https://assetstore.unity.com/packages/2d/gui/clean-minimalist-gui-pack-75123)** (FREE)
   - Buttons, sliders, panels

---

## Project Setup After Importing

### 1. Create Scenes
Create in `Assets/Scenes/`:
- `MainMenu.unity`
- `BossCreator.unity`
- `Arena.unity`

### 2. Set Up Arena Scene
1. Create ground plane (20x20 units)
2. Add NavMesh: Window → AI → Navigation → Bake
3. Add Directional Light
4. Place office props (desks, cubicles, water coolers)

### 3. Set Up Player
```
Player (GameObject)
├── CharacterController
├── PlayerController.cs
├── PlayerHealth.cs
│
├── CameraHolder (child)
│   ├── Camera
│   └── PlayerCamera.cs
│
└── WeaponHolder (child)
    ├── WeaponManager.cs
    ├── Pistol (child) - Pistol.cs
    ├── Shotgun (child) - Shotgun.cs
    ├── Chainsaw (child) - Chainsaw.cs
    └── Flamethrower (child) - Flamethrower.cs
```

### 4. Set Up Boss Prefab
```
Boss (Prefab)
├── NavMeshAgent
├── Animator
├── CapsuleCollider
├── BossController.cs
├── BossHealth.cs
├── BossAI.cs
└── BossCustomization.cs
```

### 5. Create Managers
```
GameManager (GameObject, DontDestroyOnLoad)
├── GameManager.cs
├── AudioManager.cs
└── SaveManager.cs
```

### 6. Create Boss Presets
1. Right-click Project → Create → Horrible Bosses → Boss Data
2. Name it (e.g., "MrMicromanager")
3. Fill in boss details in Inspector
4. Assign to GameManager

---

## Folder Structure
```
Assets/
├── Animations/          ← Mixamo animations
├── Audio/
│   ├── Music/
│   └── SFX/
├── Materials/
├── Models/              ← Character/weapon models
├── Prefabs/
│   ├── Player/
│   ├── Weapons/
│   ├── Boss/
│   └── Effects/
├── Scenes/
├── ScriptableObjects/
│   └── BossPresets/
├── Scripts/             ← Already done!
├── StreamingAssets/
│   └── PresetBosses/    ← JSON boss saves
└── Textures/
```

---

## Testing Checklist

- [ ] WASD + mouse look works
- [ ] Weapons fire (1-4 keys)
- [ ] Boss spawns and takes damage
- [ ] Boss AI chases player
- [ ] HUD shows health/ammo
- [ ] Pause menu (ESC)
- [ ] Boss creator saves/loads

---

## Current Script Count: 24 files

| Folder | Scripts |
|--------|---------|
| Boss/ | BossAI, BossController, BossCustomization, BossData, BossHealth |
| Combat/ | IDamageable |
| Managers/ | AudioManager, GameManager, SaveManager |
| Player/ | PlayerCamera, PlayerController, PlayerHealth |
| UI/ | BossCreatorUI, GameOverUI, HUDManager, MainMenu, PauseMenu |
| Weapons/ | Chainsaw, Flamethrower, Pistol, Shotgun, WeaponBase, WeaponManager |
