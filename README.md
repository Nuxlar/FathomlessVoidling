# Fathomless Voidling

**UNMAINTAINED (UPDATED WITH DEVOTION, FUTURE UPDATES MIGHT BREAK IT). IF YOU'D LIKE TO CONTINUE/MAINTAIN THE MOD MESSAGE ME IN THE ROR2 MODDING DISCORD**

**SUBMIT UNKNOWN ISSUES USING THE LINK ABOVE, EVEN IF UNMAINTAINED, USEFUL FOR FUTURE MAINTAINER(S)**

## Info

- Your run ends after Voidling dies (will be fixed next update - probably)
- You cannot attack Voidling's main body just the joints
- Voidling doesn't take damage at the healthbar points until he phase transitions (check phase info below)
- Small Voidling's attacks are changed as well (in case you spawn them in using debugtoolkit/aerolt)
- Attacks might change for the 1.0 (adding/swapping)

## Known Issues

- No multiplayer compatibility for now
- Acrid's epidemic can get through the barrier and poison the main body
- Music repeats at the end
- Run ends after big voidling dies

## Attack/Phase Info

<details>
  <summary>Expand</summary>

### Phase 1

- Primary: Void Missiles - Increased size, now have an explosive radius, less tracking, oscillating
- Secondary: Portal Beams - Summons portals that fire predictive lasers at the closest enemy to the portal
- Utility: Void Laser - Fires a large laser and spins around the arena
- Special: Singularity - Creates a black hole under itself, instantly killing anything that enters

### Phase 2

- Primary: Void Missiles - Same
- Secondary: Portal Beams - More lasers
- Utility: Laser Blast - Aims and fires a large laser
- Special: Wandering Singularity - Creates a small black hole that slowly follows enemies until it collapses, killing anything it touches

### Phase 3

- Primary: Void Missiles - Same
- Secondary: Portal Beams - More lasers
- Utility: Portal Blast - Creates a portal near a random enemy, firing a large laser through the portal
- Special: Wandering Singularity - Creates a small black hole that slowly follows enemies until it collapses, killing anything it touches

### In-Between Phases

- Ward Wipe: Charges up to kill everything in the vicinity, take cover in a safe ward.

### Phase 4

- Certain Death: Charges up to kill everything in the vicinity, kill it first.

</details>

## Changelog

**0.9.10**

- Adds basic config
- Nerfs black hole pull back to vanilla (without a movement ability or a bunch of speed it's immediate death)

**0.9.9**

- Fixes crash from the Devotion Update

**0.9.8**

- Fixes single joint breaking multiple times instead of once

**0.9.7**

- Removes debug log spam
- Added basic loop support until config is made (HP is multiplied by loop count)
- Reduced main body HP again
- Removed oscillation from primary attack
- Increased tracking on primary attack
- Increased speed of Wandering Singularity by 50%
- Increased size of Phase 2/3's big laser attack indicator
- Reduced Portal Beam spawn frequency for Phase 2 and Phase 3
- Increased safe wards for Ward Wipe attack
- Removed joint break Stun state
- Removed Collapse state (falling through the donut)
- Added joints regenerating on the third break instead of collapsing

**0.9.6**

- Drastically reduced main body HP
- Reduced joint HP
- Gave joints Adaptive Armor (mithrix armor)
- Made joints count as bosses
- Increased Laser Blast's charge delay (2s -> 3s)
- Added attack indicator for Phase 2/3's big laser attacks
- Reduced big laser attack radius to match the VFX

**0.9.5**

- 1.0 "Beta"
