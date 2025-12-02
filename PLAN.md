# Horrible Bosses - Unity Prototype Plan

## Overview
A first-person action game where players fight customizable boss characters with various weapons. Think Doom Eternal meets office revenge fantasy.

## Technology Stack
- **Engine:** Unity 2022 LTS (or newer)
- **Render Pipeline:** URP (Universal Render Pipeline) - good graphics, performant, beginner-friendly
- **Language:** C#
- **Platform:** Windows/Mac/Linux desktop

## Core Systems to Implement

### 1. First-Person Controller
- WASD movement with sprint (Shift)
- Mouse look (adjustable sensitivity)
- Jumping and basic physics
- Head bobbing for immersion

### 2. Weapon System
Four weapon types with distinct behaviors:

| Weapon | Primary Fire | Special | Ammo |
|--------|-------------|---------|------|
| Pistol | Single shot, hitscan | Quick reload | 12/unlimited |
| Shotgun | Spread shot, high damage | Pump action | 8/32 |
| Chainsaw | Continuous melee damage | Glory kill finisher | Fuel gauge |
| Flamethrower | Cone of fire, DOT | Fuel burst | Fuel gauge |

- Weapon switching (1-4 keys + scroll wheel)
- Ammo pickups
- Visual feedback (muzzle flash, screen shake)

### 3. Boss Creator System
Customizable attributes:
- **Name:** Text input for boss name (displayed above head)
- **Body Type:** Skinny, Average, Large, Huge
- **Outfit:** Business suit, Casual, Hawaiian shirt, etc.
- **Head:** Various face options
- **Colors:** Skin tone, clothing colors, hair color
- **Voice:** Different grunt/taunt sounds
- **Difficulty Preset:** Easy, Medium, Hard, Nightmare

### 4. Boss AI System
Behavior tree / state machine:
- **Idle:** Patrols or waits
- **Alert:** Noticed player, preparing to engage
- **Chase:** Pursuing player
- **Attack:** Melee swings, throws objects, charges
- **Stunned:** After taking heavy damage
- **Rage:** Low health, faster and more aggressive
- **Death:** Ragdoll + celebration

Difficulty scaling:
- Health multiplier
- Damage multiplier
- Speed multiplier
- Attack frequency
- Special abilities unlock at higher difficulties

### 5. Combat System
- Player health (100 HP, regenerates slowly)
- Boss health bars (UI)
- Damage numbers (floating text)
- Hit feedback (screen flash, controller rumble)
- Death/respawn system
- Score tracking

### 6. Arena/Level
- Office-themed battle arena
- Destructible objects (desks, computers, water coolers)
- Ammo/health pickups scattered around
- Multiple areas/rooms
- Hazards (fire, electricity)

### 7. UI System
- Main menu (Play, Boss Creator, Options, Quit)
- HUD (health, ammo, weapon, boss health)
- Boss creator interface
- Pause menu
- Victory/defeat screens

### 8. Audio
- Weapon sounds
- Boss grunts and taunts
- Background music (intense combat music)
- UI sounds

## Project Structure

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── PlayerHealth.cs
│   │   ├── PlayerCamera.cs
│   │   └── PlayerInteraction.cs
│   ├── Weapons/
│   │   ├── WeaponBase.cs
│   │   ├── WeaponManager.cs
│   │   ├── Pistol.cs
│   │   ├── Shotgun.cs
│   │   ├── Chainsaw.cs
│   │   └── Flamethrower.cs
│   ├── Boss/
│   │   ├── BossController.cs
│   │   ├── BossAI.cs
│   │   ├── BossHealth.cs
│   │   ├── BossCustomization.cs
│   │   └── BossData.cs (ScriptableObject)
│   ├── Combat/
│   │   ├── DamageSystem.cs
│   │   ├── Projectile.cs
│   │   ├── HitEffect.cs
│   │   └── DamageNumber.cs
│   ├── UI/
│   │   ├── MainMenu.cs
│   │   ├── HUDManager.cs
│   │   ├── BossCreatorUI.cs
│   │   ├── PauseMenu.cs
│   │   └── GameOverUI.cs
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   ├── AudioManager.cs
│   │   ├── ScoreManager.cs
│   │   └── SaveManager.cs
│   └── Utilities/
│       ├── ObjectPool.cs
│       └── Helpers.cs
├── Prefabs/
│   ├── Player/
│   ├── Weapons/
│   ├── Boss/
│   ├── Effects/
│   └── Pickups/
├── Materials/
├── Textures/
├── Audio/
│   ├── SFX/
│   └── Music/
├── Scenes/
│   ├── MainMenu.unity
│   ├── BossCreator.unity
│   └── Arena.unity
└── ScriptableObjects/
    ├── WeaponData/
    └── BossPresets/
```

## Recommended Free Assets

### From Unity Asset Store (Free):
1. **Starter Assets - First Person** - Unity's official FPS controller
2. **Low Poly Office Pack** - Office environment
3. **Polygon Starter Pack** - Basic character models
4. **War FX** - Weapon effects (muzzle flash, impacts)
5. **AllSky Free** - Skyboxes

### From Mixamo (Free):
- Character models and animations
- Boss idle, walk, run, attack animations

### From Kenney.nl (Free, CC0):
- Weapon models
- UI elements

## Implementation Order

### Phase 1: Foundation (Start Here)
1. Create Unity project with URP
2. Import Starter Assets - First Person
3. Set up basic scene with ground plane
4. Test movement works

### Phase 2: Weapons
5. Create WeaponBase abstract class
6. Implement Pistol (simplest weapon)
7. Add weapon switching system
8. Implement Shotgun
9. Implement Chainsaw (melee)
10. Implement Flamethrower (particle-based)

### Phase 3: Boss
11. Create basic boss prefab with NavMesh agent
12. Implement BossAI state machine
13. Add boss health and damage
14. Create BossCustomization system
15. Build Boss Creator UI

### Phase 4: Polish
16. Add HUD (health, ammo, boss health bar)
17. Implement main menu
18. Add audio
19. Victory/defeat conditions
20. Save custom bosses

## Getting Started Instructions

1. **Download Unity Hub** from unity.com
2. **Install Unity 2022.3 LTS** (or newer) with:
   - Windows Build Support
   - (Optional) Mac/Linux Build Support
3. **Create new project:**
   - Select "3D (URP)" template
   - Name: "HorribleBosses"
4. **Import assets from Package Manager:**
   - Starter Assets - First Person Controller
5. **I will provide all the C# scripts**

## Notes
- All scripts will be fully commented for learning
- Each system will be modular and extensible
- Focus on gameplay feel before graphics
- Can upgrade to HDRP later for better graphics
