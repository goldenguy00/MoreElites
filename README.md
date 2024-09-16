# More Elites

**SUBMIT ANY ISSUES USING THE LINK ABOVE**

There's a config for disabling specific elites and the Tier 2 HP/Damage Multipliers. Some crown displays might be wonky.

## Tier 1

### Volatile (RoR1)

- All attacks explode
- Fires a missile in random intervals between 1 and 5 seconds (inclusive)
- Missile damage uses fixed scaling instead of base damage
- Fixed damage scaling values: 12 base +2.4/lvl for normal enemies and 18 base +3.6/lvl for boss class enemies

### Frenzied (RoR1)

- +2 base move speed
- +50% attack speed
- Teleport to closest enemy or in move direction

### Empowering

- 25m radius zone that applies a warbanner buff to itself and surrounding allies

## Tier 2

### Echo

- Spawns 2 clones of itself with 10% max HP
- Clones die if main body is killed
- Clones fire hunter projectiles
- Hunter Projectiles damage uses fixed scaling instead of base damage
- Fixed damage scaling values: 24 base +4.8/lvl for normal enemies and 32 base +7.2/lvl for boss class enemies
  - **I don't loop lmk if these values are too high or too low**

![3 elite beetles](https://i.ibb.co/4NJqjwk/moreelites.png)

## Changelog

**1.0.3**

- Updated for SOTS

**1.0.2**

- Removes debug logs (whoops)

**1.0.1**

- Changes Volatile Elites' missiles to use fixed damage scaling (Thanks Moffein)
- Changes Echo Elites' hunter projectiles to use fixed damage scaling

**1.0.0**

- Fixes wake of vultures or getting the volatile aspect giving behemoths
- Volatile elites' missiles do less damage (50% damage instead of 100%)
- Volatile elites' missiles have more turbulence (2 -> 6)
- Volatile elites' "behemoth" explosion damage reduced (25% damage instead of 60%)
- Reduces Echo elites' hunter projectile damage (300% -> 100%)
- Updated dependencies

**0.9.5**

- Adds Volatile Elite
- Changes Frenzied blink effect back
- Removes sound from Empowering warbanner area

**0.9.0**

- Fixes incompats (hopefully)
- Frenzied aspect now blinks on use instead of automatically
- Changes Frenzied blink effect
- Reduces Frenzied minimum distant to TP to player
- Adds sound to Echo spawn

**0.8.5**

- Adds a drop chance to the aspects
- Fixes Honor version of Empowering and Frenzied elites not having the correct damage/hp multipliers
- Enhances Frenzied TP to tp within a range around the player instead of directly on top of them

**0.8.3**

- Fixed Acqueducts bug
- Fixed crowns not showing up
- Added overlay to Echo Elites
- Changed Empowering Elite Ramp

**0.8.2**

- Fixed the config and Echo Elites can spawn now

**0.8.1**

- Makes Echo T2
- Adds multiplier config for T2 elites added by this mod

**0.8.0**

- Reworked
- 3 new T1 Elites
- Added config
