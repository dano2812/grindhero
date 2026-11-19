# Game and mechanics specification

**Status:** Draft for owner input  
**Owner:** Game owner (you)  
**Last updated:** 2026-10-10  
**Related documents:** [Game brief](game-design.md), [roadmap](roadmap.md)

Use this to describe the game players will experience, not how Unity code must
be written. Replace `[Your answer]`, check the options that fit, and add a
sentence/example where useful. If you are unsure, write **"Please recommend"**
and I can present options and trade-offs. Leave later-MVP questions blank until
they are needed. A prototype can answer feel questions better than guessing
precise numbers now.

Do not put passwords, store keys, signing certificates, or real player data in
this document.

## 1. Already confirmed

These came from the owner and do not need to be answered again:

- Mobile game for iOS and Android.
- Simple-looking 3D game with a fixed camera angle.
- The player controls a hero who fights enemies and receives stats/progression
  and drops.
- The game will include different weapons, armor, and skins; quests, hidden
  treasure/easter eggs, and a simple story.
- Maps have sections; defeating a section's boss unlocks the next section.
- The game should support adding sections and mechanics without rewrites.
- In-app purchases may be added in the future, not in the first playable slice.

## 2. Player promise and first release

**Describe the game in one sentence from the player's point of view:**  
[Your answer]

**What should feel most enjoyable? Choose up to three, then explain:**  
- [ ] Responsive combat
- [x] Building a powerful hero
- [ ] Finding rare equipment
- [ ] Exploring and discovering secrets
- [x] Beating challenging bosses
- [ ] Following the story
- [ ] Short, relaxing play sessions
- [ ] Other: Grinding,

**Who do you imagine playing, and what age range/content rating do you have in
mind?** (This can be provisional.)  
for players which enjoy farm stats, getting stronger and beating enemies. Game should be for wide age range, but 15+, main target are players who can afford paying for game stuff in the future, but the game can't be P2W, buing stats will just yease the progress and 

**What belongs in the first playable slice?** One small section; hero movement;
one basic axe; head and torso armor; Basic enemies with HP-drop and Axe
Attack-drop subtypes; one Advanced enemy; one boss; deterministic stat rewards;
and a locked exit that opens after the boss. Confirmed.

**What should explicitly not be in the first release?** Multiplayer, ads, PvP,
trading, accounts, and real-money purchases are currently excluded unless you
change this scope.  
Confirming

## 3. Play session, camera, and controls — decide before the first slice

**Preferred screen orientation:**  
- [ ] Portrait
- [x] Landscape
- [ ] Please prototype both and recommend
- Notes: [Your answer]

**Typical play session:**  
- Target length: Clearing a base map should take less than an hour. Respawning
  mobs can support longer grinding sessions; first-slice cooldowns and the
  daily respawn cap are specified in section 7.
- Should a player be able to stop safely at almost any time? Yes, when game is overlayed, hidden or minimezed, game have to be paused, except mobs cooldowns, and some other cooldowns, each cooldown type will be specified if has to be paused or not.
- Primarily one-handed or two-handed?Game should be one handed. Movement of the hero will be one finger, something like joystick, center of the joystick will be at the place where player put finger on the screen and finger movemnent will decide the vector of the movement. 

**Camera:** The angle stays fixed, but it can still follow the hero. Describe
what "fixed" means to you:
- [x] Camera follows the hero while keeping its angle
- [ ] Camera stays at a fixed point within each room/area
- [ ] Camera can move only at section transitions
- [ ] Please recommend
- How should walls/objects behave when they hide the hero?Objects should stay the same. 

**Movement and targeting:**  
- [x] Virtual joystick movement
- [ ] Tap a location to move
- [ ] Directional buttons
- [ ] Please prototype/recommend
- How should movement feel (direct, weighty, fast, deliberate)? Recommend, or implement more types and we willdecide after trying it
- [ ] Player manually selects an enemy
- [x] Game automatically targets a nearby enemy
- [ ] Player taps an enemy to target it
- [ ] Please recommend

**Attacking:**  
- [ ] Tap/hold an attack button
- [x] Automatically attack a valid nearby enemy
- [ ] Each weapon uses a different input/attack pattern
- [ ] Please recommend
- No active skills in the first slice. Dodge and block are passive defensive
  chance checks earned through stat points; see section 6. Hero healing is
  automatic and always active while alive, not a manual control (owner
  clarification, 2026-11-19; implemented with provisional 2 HP/second tuning).

**Touch controls that must always be reachable:** [Your answer]  
**Accessibility or control options important from the first slice:** [Your answer]

## 4. Weapon and armor types and equipment slots — decide before equipment systems

Choose which kinds of equipment exist and how they fit the hero. Exact damage,
defense, drop odds, and balance can be decided later in section 6. You do not
need a large equipment catalog for the first playable slice.

**First-slice equipment scope:**  
- Starting weapon: one basic axe.
- Starting armor: separate head and torso armor that defend against axe-type
  attacks. Exact armor values are not set yet.
- Other armor slots, weapon types, rarity tiers, special abilities, and
  equipment-piece upgrades are future content, not needed for the first slice.

**Future equipment direction:** Each weapon type has its own damage type.
Matching armor reduces damage of that type. The planned rarity tiers are common,
uncommon, rare, epic, legendary, and mythic, with room for more later. Weapons
may have special abilities; collecting equipment pieces may increase equipment
level/stats and unlock abilities. The progression details are not part of the
first-slice rules.

**Weapon types:** Possible examples include sword, axe, dagger, bow, staff, and
spear. Choose the types that fit your game; add or remove rows as needed.

| Weapon type | Melee or ranged | How it should feel/work | Slice, MVP, or later |
| --- | --- | --- | --- |
| Axe | melee | cutting | Slice |
| Hammer | melee | slash | later |
| spear | melee | pointing | later |
| dagger | melee | stabbing | later |
| staff | melee | magic movement | later |

**Weapon slots — how many weapons can the hero equip at once?**
- **First slice:** one unlocked weapon slot, holding one basic axe; the other
  weapon slots start locked.
- **Later:** up to five weapon slots may be unlocked. Each slot represents an
  active hand holding one weapon; all equipped weapons attack at the same time.
  Unequipped weapons remain in inventory.
- Any weapon type may be equipped in any unlocked weapon slot; slots are not
  tied to a particular weapon type.
- The player may open the inventory and change equipment during combat.
- How additional weapon slots are unlocked: [Decide later.]
- Shield use is future scope. The current preference is for a shield to use an
  off-hand weapon slot; how this interacts with simultaneous attacks remains
  undecided.

**Armor types:** Possible examples include helmet/head, chest/body, gloves/arms,
legs, and boots/feet. Decide which are individual items and which, if any, form
one combined armor item.

For **"Include in slice, MVP, or later?"**, choose when that armor type should
be added:
- **Slice:** The small first playable prototype described in the [game brief](game-design.md),
  used to test the core gameplay loop.
- **MVP:** The fuller minimum version of the game intended for its first release.
- **Later:** Not needed for the slice or MVP; it can be considered for a future update.

For example, marking helmets **Later** means the first prototype and MVP can
launch without a helmet equipment type. You can also write **"not planned"** if
you do not want that type at all.

| Armor type or body area | Separate equipment slot? | Include in slice, MVP, or later? | Visible on hero? |
| --- | --- | --- | --- |
| Head / helmet | yes | Slice | yes |
| Chest / body | yes | Slice | yes |
| Arms / gloves | yes | later | yes |
| Legs | yes | later | yes |
| Feet / boots | yes | later | yes |
| Shield (if used) | yes | later | yes |
| Other: [Your answer] | [Your answer] | [Your answer] | [Your answer] |

**Armor slots and rules:**
- [ ] One slot for a complete armor item
- [x] Separate slots for individual armor pieces
- [x] Shield uses the off-hand weapon slot
- [ ] Shield has its own slot
- [ ] Accessories have separate slots
- Other slot(s): No other slots for now
- **First slice:** two separate armor slots, head and torso. Arms, legs, and
  boots are planned for later releases.
- Should armor change gameplay stats, be cosmetic only, or allow both kinds?
  Both
- Can a player equip more than one item of the same type?forweapon slots yes, there could be any weapon in any weapon slot.Armor type has its won armor type slot

## 5. Core game loop and combat — decide before the first slice

`Enter section -> fight enemies -> receive predictable rewards and improve
stats/equipment -> defeat the section boss -> unlock the next section.`

**Is combat:**  
- [x] Real-time action
- [ ] Turn-based
- [ ] Other: [Your answer]

**Hero attack rules:** The hero automatically targets the closest valid enemy
within range and attacks. Attack range and cooldown depend on weapon type.
Enemies do not interrupt the hero's attacks. Each active weapon slot attacks
independently, so multiple equipped weapons attack at the same time. Chance-based
dodge, critical hit, and block checks are part of hit resolution; their exact
order is recommended in section 6 and should be confirmed before implementation.

**Enemy focus and attack rules:** Every enemy's focus (aggro) range is exactly
twice its attack range. The focus range is derived from attack range, not tuned
separately. An idle enemy stays idle outside focus range, begins chasing when
the hero enters focus range but remains outside attack range, and engages when
the hero enters attack range. All enemy types (Basic, Advanced, and boss) use a
1.0-second attack cycle, measured from the start of one attack to the start of
the next. Do not deal damage immediately when the hero enters attack range;
show the attack's wind-up before damage.

**Attack cadence and wind-up:** The 1.0-second enemy cycle is user-specified.
Wind-up lengths are still prototype suggestions; each wind-up is included in
the start-to-start cycle. Each active hero weapon slot has its own cycle.

| Actor/attack | Cycle | Wind-up/telegraph | Notes |
| --- | --- | --- | --- |
| Hero's basic axe | 1.0 second | 0.25 second | Starting recommendation for automatic attacks. |
| Basic mob (both subtypes) | 1.0 second | 0.5 second (suggested) | Both Basic variants share the same combat profile. |
| Advanced mob | 1.0 second | 0.65 second (suggested) | Exact attack behavior remains to be designed. |
| Boss attacks | 1.0 second | 0.6 second (suggested) | The boss's specific attacks and tells remain to be designed. |

The earlier suggestion for a separate 9-second boss heavy-attack cadence is
withdrawn. Define any boss attack pattern within the 1-second cycle unless that
rule is explicitly changed. Tune wind-ups after testing actual animations, hit
ranges, and mobile controls.

**Mob chase and recovery:** When the hero moves outside a mob's attack range, the
mob follows for up to 5 seconds. If the player does not hit that mob within
those 5 seconds, it returns to its original spawn point and starts healing
while it returns at 10% of its maximum HP per second, capped at maximum HP. A
player hit during the chase or return interrupts return/healing and resets the
5-second no-hit timer; if the hero remains outside attack range, the mob gets a
fresh chase window. This behavior is separate from defeated-enemy respawn
cooldowns in section 7. This rule applies to every enemy type, including the
boss. If a mob reaches its spawn before it is fully healed, it remains there and
continues healing while idle until its HP is full. Entering combat interrupts
this recovery.

**Actor health display:** Show a compact banner above the hero and each living
mob. Each banner displays the actor name, current/max HP, and a proportional
health bar. Position banners using the actor's camera projection; hide them
when the actor is outside the usable screen area, and remove a mob's banner
when it is defeated. Keep the existing hero stats and mob respawn markers.

**Killer retreat after player death:** When an enemy's attack kills the hero,
that enemy immediately returns toward its original spawn point and cannot attack
again for 3 seconds. If the hero hits it during that period, it leaves the
retreat and resumes normal combat; otherwise its normal behavior resumes when
the timer expires. This applies to every enemy type, including the boss, and
the boss also uses the return-and-heal behavior above. No enemy can damage the
hero while the hero is in the respawn countdown. The 3-second countdown uses
active gameplay time and pauses when gameplay is paused. If the app closes
during the countdown, the saved zero health starts a fresh 3-second countdown
on the next launch.

**Enemies:** List 2-4 useful archetypes for the first content slice. For each,
describe its visible tell, threat, and counterplay.

| Enemy | What it does | How the player can respond |
| --- | --- | --- |
| Basic — HP subtype | Attacks on a 1-second cycle when the hero is in range. Drops HP stat points that increase maximum HP. | Move outside its attack range to avoid an attack. Land a hit within 5 seconds if you want to keep fighting; prioritize this subtype when seeking HP points. |
| Basic — Attack subtype | Attacks on a 1-second cycle when the hero is in range. Drops attack stat points that increase base axe attack. | Use the same movement response as the Basic HP subtype; prioritize this subtype when seeking Axe Attack points. |
| Advanced | Attacks on a 1-second cycle when the hero is in range. Specific attack/reward: [Decide later.] | Move outside its attack range; land a hit within 5 seconds if you want to keep fighting. Its distinctive threat and reward remain to be designed. |

**Boss:** What should make a boss different from a larger normal enemy? Describe
one or two attacks/telegraphs and the intended challenge.

The boss's primary gameplay purpose is to unlock the next, more challenging
location. For now, the owner selected ordinary mob attack behavior with higher
damage and HP rather than a distinct attack pattern (2026-11-19). Specific
boss-only attacks and telegraphs are deferred.
Its attack cycle is 1 second. It uses the same 5-second chase timeout and
return-and-heal behavior as other enemies.

**Death and respawn:**  
- What happens to the player's location and health? After a 3-second countdown,
  the hero returns to the spawn point with half of maximum HP.
- Are any rewards, currency, or progress lost? No
- Where does the hero return?Spawn point
- Can ordinary enemy rewards be earned again after a respawn? Yes, within the
  daily respawn limit in section 7. Whether boss or treasure rewards can be
  earned repeatedly remains undecided.

**Pause/background behavior:** Combat pauses when the app is hidden, minimized,
or interrupted. On return, gameplay continues from where it stopped. Mob respawn
cooldowns continue while the app is away; the pause behavior of other cooldowns
will be specified individually.

## 6. Hero growth, equipment, and rewards — decide before systems work

**First-slice progression:** The player collects stat points. The Basic HP
subtype drops points that increase maximum HP; the Basic Attack subtype drops
points that increase base Axe Attack. Final design amounts are not approved;
the graybox currently grants one point per corresponding Basic defeat (see the
prototype values below). There are no active skills in the first slice; dodge
and block are passive probability checks.

**Planned stats:** Health (increases maximum HP), healing (increases healing
per second), weapon-type-specific attack, critical chance, critical damage,
dodge chance, and block chance. Dodge and block chance start at zero and
increase from collected points. A successful dodge avoids damage. A successful
block means the hero receives 80% of the damage; this rule may be adjusted
later. The first-slice Basic enemy variants only provide HP and Axe Attack
points; sources for the other stats remain undecided.

**Owner clarification (2026-11-19):** Hero healing is automatic and always
active, rather than a manual action or out-of-combat-only effect. This is a
requirement for progression work, not implemented hero regeneration in the
current slice. Its starting rate, per-point scaling, and sources of Healing,
critical, Dodge, and Block points still need concrete values/content before
implementation; "the same for other stats" does not specify those sources.

**Damage calculation (current rule):**
- Weapon attack damage is the base damage for its weapon type plus the weapon's
  own damage. For the slice: **Axe Attack = base Axe damage + axe weapon damage**.
- Matching armor subtracts damage of that type. Example: 100 attack damage
  against 20 matching armor deals 80 damage. If armor is greater than attack
  damage, received damage is 0.

**Reference hero and enemy durability targets (provisional):** Use a 100-HP
hero with 20 total Axe damage per hit, a 1.0-second axe cycle, zero dodge/block
chance, and no stat upgrades or critical hits for the first balance pass. The
example armor loadout is 2 matching armor on the head plus 3 on the torso, for
5 total; test with both 0 and 5 armor until it is decided whether the hero
starts equipped.

| Enemy | Reference HP | Hero hits to defeat |
| --- | ---: | ---: |
| Basic HP or Attack subtype | 80 | 4 |
| Advanced | 180 | 9 |
| Boss | 450 | 23 |

These hit counts assume every hero attack lands for 20 damage and exclude
critical hits, movement, and other interruptions. The HP values are starting
targets, not final decisions. Enemy attack damage per hit must be tuned against
the new 1-second enemy cycle; the earlier raw-damage examples assumed slower
cycles and are withdrawn. Test both 0 and 5 matching armor, and tune damage so
Basic enemies are low pressure, an Advanced enemy is dangerous but beatable
alone, and the boss rewards moving out of attack range.

**Recommended damage-resolution order:**
1. Get raw damage from the attacker's definition. For the hero's axe, this is
   base Axe damage plus the axe weapon's own damage. If critical hits are enabled
   later, apply a configurable 1.5x critical multiplier here; keep critical
   chance at 0 for the initial balance baseline.
2. Roll the defender's dodge chance. A successful dodge deals 0 damage and
   skips the block check.
3. Add head and torso armor values that match the incoming damage type, then
   subtract that total from raw damage and clamp the result to 0.
4. If the block check succeeds, multiply the remaining damage by 0.8, so the
   hero receives 80% of the post-armor damage.

This preserves the current flat-armor rule and makes matching head/torso armor
additive. Since the rule permits armor to reduce a weak hit to zero, keep early
armor modest relative to enemy attacks and test against the strongest expected
armor loadout; do not add a hidden minimum-damage floor.

**Make combat balance adjustable:** Keep the damage formula in tested combat
code, but store tuning values in version-controlled data assets (Unity
ScriptableObjects are suitable) rather than constants buried in enemy or weapon
behaviors. Weapon definitions should own damage, cycle, wind-up, and attack
range; enemy definitions should own health, attack damage/type, cycle, wind-up,
attack range, telegraph, and special-attack timing. Enemy focus range is derived
as exactly twice attack range, not stored as separate tuning. A shared
combat-tuning asset should own values such as the block multiplier and critical
multiplier. Validate that health and damage are non-negative and attack cycles
are greater than zero. Changing these values should not require changing
combat code.

**Longer-term progression:** Character-building choices will be decided later.
Equipment may be leveled by collecting matching equipment pieces, increasing
its stats and potentially unlocking abilities. Duplicate/unused equipment can
be used for leveling. Skins are visual only. The player may change equipment
during combat through the inventory.

**Rewards:** Drops are deterministic, not random, and the player should see the
reward when an enemy is defeated. Basic HP enemies reward HP points; Basic
Attack enemies reward Axe Attack points. The first slice has no currency.
Whether equipment itself drops in the slice and the exact point quantities
remain undecided. The main story must not require a specific random drop.

Do not specify exact balance numbers unless you have a concrete target. Describe
the desired relative effect; we can tune numbers in a device-tested prototype.

### Current first-slice implementation values (prototype only; not owner sign-off)

The generated first section uses the following adjustable ScriptableObject values.
They are implementation assumptions to make the loop playable and testable;
they are not approved final balance or a commitment to the starting loadout.

| Actor/content | Current prototype values |
| --- | --- |
| Hero | 100 starting max HP; 15 base Axe damage + 5 axe-weapon damage; 1-second attack cycle with 0.25-second wind-up; starts with 2 head and 3 torso Axe armor equipped; 0% dodge/block; block multiplier 0.8; defeat respawn at 50% max HP; 2 HP/second healing while alive |
| Basic — HP | 80 HP; 11 Axe damage; 0.5-second wind-up; 2.1 attack / 4.2 focus range; 3.2 movement speed; drops 1 permanent HP point (+5 max HP); 120-second respawn cooldown |
| Basic — Axe Attack | 80 HP; 13 Axe damage; 2.1 attack / 4.2 focus range; otherwise shares the Basic HP profile; drops 1 permanent Axe Attack point (+1 base Axe damage); 120-second respawn cooldown |
| Advanced | 180 HP; 18 Axe damage; 0.65-second wind-up; 2.25 attack / 4.5 focus range; 3.4 movement speed; no stat reward yet; 300-second respawn cooldown |
| Boss | 450 HP; 22 Axe damage; 0.6-second wind-up; 2.5 attack / 5.0 focus range; 3.6 movement speed; returns and heals at 10% max HP per second after a 5-second chase; no reward or respawn yet |

All enemy attack cycles are 1 second, as specified. All enemies currently
chase for 5 seconds after the hero leaves attack range and heal at 10% of max
HP per second while returning, capped at full HP. If they reach spawn before
full health, they continue healing while idle until full.
The three-respawn cap is currently **per spawn point per UTC calendar day**.
That cap and UTC reset remain implementation assumptions for owner review.
Mosswatch includes the first main quest, grove treasure, and short story
described in section 11. The five-slot inventory system and equipment drops
remain unimplemented.

### Required difficulty and progression pacing tool

**Owner requirement, 2026-11-19:** A difficulty/progression calculation and
testing tool must be part of development, not an optional task after content
is finished. It must help calculate and tune how much farming is needed in
each location before the hero can defeat its boss.

**Illustrative pacing targets, not final balance or current behavior:**

| Progression point | Owner's example |
| --- | --- |
| First location, Mosswatch Hollow | Roughly 1 hour of play, farming the available mobs, before the hero is ready to defeat the boss. |
| Final location, The Eclipse Throne | Roughly 1-3 months of farming progression before the hero is ready to defeat the boss. |

These are examples to evaluate with the tool, not approved exact durations,
mandatory waiting timers, or a claim that the current slice meets them.
"Final map" follows the clarified location terminology: the final location
within this map. Whether the month-scale example means elapsed calendar time
or accumulated active play, and the assumed daily play schedule, remain to be
specified.

The baseline must be **free-to-play, with no paid progression boosts**.
Evaluate the early/late pacing examples against that baseline. Future paid
acceleration may shorten progression, but must never be required to reach a
boss, complete a quest, unlock a location/map, or finish the game.

The required tool must:

- Estimate per-location combat difficulty, stat growth, farming effort, and
  time to boss readiness from an explicit expected hero entry state, retaining
  progress earned in earlier locations rather than assuming a fresh hero.
- Use the game's authored balance data and gameplay rules: combat stats and
  timing, armor/healing, enemy rewards, respawn cooldowns, daily allowances,
  and offline cooldown passage. Account for treasure/quest rewards and
  weakness-based quest encounters as those designs become specified.
- State player-behavior assumptions, including combat skill, use of weaknesses,
  farming routes/backtracking, optional rewards, deaths/retries, and play
  frequency. Do not present one unexplained estimate as universal.
- Distinguish active play/farming time from elapsed calendar time, including
  waiting imposed by cooldowns and daily limits.
- Produce repeatable reports showing assumptions, relevant balance-data
  version, expected boss-readiness progression, and comparison with configured
  pacing targets. Flag unreachable progression or conflicting limits instead
  of returning a misleading completion-time estimate.

Tool interface, calculation/simulation approach, exact skill/session scenarios,
boss-readiness criteria, target durations/tolerances for every location, and
automation/CI integration remain design work. The tool must inform balancing
alongside playtesting; it does not prove that a long grind is enjoyable.
**Status: required future development deliverable; not implemented.**

## 7. Sections, exploration, quests, and story

**Section structure:**  
- Are sections linear, branching, or connected through a hub?Section should be neighboring areas
- Can the player revisit completed sections? Yes, player can return to the location, mobs can respown for a limiterd times, and the counter is resetted on new day
- Do ordinary enemies respawn when the player returns? There is respawn cooldown (also running when the player is not in the game)
- Are there checkpoints or safe areas? No
- Precisely what unlocks the next section?Defeating boss unlocks new section, in first slice there will be only one location

### Approved world progression and location themes

**Owner-approved direction, 2026-11-19:** One large, connected world with the
following 20 themed locations in progression order. Location 1 is the starting
region. Defeating a location's boss permanently unlocks the next location,
which is harder and supports further hero progression. After unlocking a
location, the hero can freely cross between it and previously unlocked
locations; advancing does not close earlier routes. Location unlocks must
survive saving and restarting.

The names, themes, and sequence below are approved world-design direction,
not a claim that all 20 locations are built. Only Mosswatch Hollow is currently
implemented. Exact geography, connecting paths, later bosses, enemy rosters,
quests, rewards, and difficulty values remain to be designed.

| Order | Location | Theme and visual identity |
| --- | --- | --- |
| 1 | Mosswatch Hollow | Overgrown woodland ruins, mossy stones, lantern paths, and an abandoned watchtower. |
| 2 | Goldenfield Marches | Autumn farmland with enormous wheat fields, deserted windmills, and ruined estates. |
| 3 | Whisperfen Marsh | Misty wetlands, crooked trees, glowing reeds, and villages built on stilts. |
| 4 | Wreckwater Coast | Rugged beaches, shattered ships, sea caves, and a weathered lighthouse. |
| 5 | Coral Crown | Exposed coral terraces, turquoise tidal pools, giant shells, and pearl-white ruins. |
| 6 | Sunscar Dunes | Golden desert, half-buried monuments, oasis gardens, and sun-bleached bones. |
| 7 | Redfang Canyon | Towering red cliffs, narrow ravines, rope bridges, and cliffside settlements. |
| 8 | The Silent Necropolis | An ancient burial city filled with monumental tombs, statues, and spectral lanterns. |
| 9 | Sporelight Grotto | Underground mushroom forests, luminous spores, tangled roots, and enormous fungal shelves. |
| 10 | Prismheart Depths | Vast crystal caverns with gemstone formations, underground rivers, and refracted light. |
| 11 | Ironroot Foundry | A subterranean industrial complex of colossal furnaces, rusted machinery, and ore railways. |
| 12 | Thunderstep Highlands | Windswept grasslands, standing stones, storm-dark skies, and lightning-scarred ridges. |
| 13 | Frostbound Pass | Snow-covered mountain trails, frozen fortifications, avalanche slopes, and ancient watchfires. |
| 14 | The Mirror Expanse | A vast frozen lake with glasslike ice, trapped ruins, and shimmering auroras. |
| 15 | Skyreach Gardens | Mountaintop sanctuaries connected by stone bridges, with hanging gardens above the clouds. |
| 16 | Embermaw Caldera | An active volcanic basin of black basalt, flowing lava, sulfur vents, and scorched temples. |
| 17 | The Bannerless Fields | An enormous ancient battlefield scattered with broken siege engines, fallen standards, and giant armor. |
| 18 | Clockwork Meridian | An abandoned mechanical city of brass towers, exposed gears, canals, and astronomical instruments. |
| 19 | Starfall Basin | A meteor-shattered landscape with glowing celestial fragments, twisted stone, and strange vegetation. |
| 20 | The Eclipse Throne | A dark royal stronghold surrounding an eclipsed sun, with black marble, pale gold, and fractured monuments. |

**First-slice mob respawn rules:**
- Basic mobs: 2-minute respawn cooldown.
- Advanced mobs: 5-minute respawn cooldown.
- Maximum: 3 respawns per day. The cooldown continues while the app is closed.
- Current prototype interpretation: the cap is per spawn point and resets at
  the next UTC calendar day. Cooldowns and counts are saved locally and continue
  while the app is closed. This scope/timezone is an implementation assumption,
  not an owner-approved design decision.
- While a respawnable mob is defeated, show a compact marker above its spawn
  point, positioned from that point's screen projection rather than in a global
  HUD list. The marker shows the mob name, remaining cooldown as `MM:SS`, and an
  elapsed progress bar. Unlabeled filled pips show remaining daily respawns;
  muted outlined pips show respawns already used. Do not add a text count. Hide
  the marker while the mob is alive or its spawn point is outside the usable
  screen area.

**Quests:**  
- Should quests be automatically accepted, explicitly accepted, or either?There should be some entity with exlamation mark which tells the quest

- Which objectives are interesting? (Examples: defeat, explore, collect, talk.) Defeat and collect 
  
- Can optional quests be failed or abandoned? No optional quests now., Just the main story quest
- What should quest rewards/progression be? Stat rewards and unlocking special areas

### Location quest giver and summoned quest mobs

**Owner-confirmed direction, 2026-11-19; documentation only, not implemented.**
The owner clarified that quest-mob counts increase across the **locations
within each map**, not across separate multi-location maps. These encounters
are distinct from the bosses that unlock neighboring locations.

- Each location has a quest-giver NPC who summons a staged sequence of quest
  mobs. Location N has N quest mobs: Mosswatch Hollow has 1, Goldenfield Marches
  has 2, and The Eclipse Throne (location 20) has 20.
- Quest mobs are fought sequentially. Defeating the current stage makes the
  next stage available; it does not summon the next mob automatically.
- Summoning is always player-requested: the hero must visit the location's
  quest giver and explicitly confirm the summon. This applies to the first
  stage, each subsequent stage, and retries.
- If a quest mob kills the hero, that mob disappears. It does not automatically
  respawn when the hero revives. The hero must return to the quest giver and
  confirm another summon to retry the undefeated stage.
- A failed attempt does not advance the sequence or undo completed stages.
- Each quest mob must be unique in some meaningful way, rather than relying
  only on a larger HP pool.
- A quest mob is highly durable/resistant relative to the hero strength
  expected when it becomes available, but has a discoverable weakness.
- Finding and exploiting that weakness must make victory achievable before
  the hero is strong enough to defeat the relevant boss. These encounters
  should reward understanding the weakness, not require out-grinding their
  baseline resistance.
- Defeating any quest mob grants a substantially greater reward than an
  ordinary mob. Reward each completed encounter, not only the end of the
  entire sequence.
- Future quest-stage and reward persistence must follow the existing
  requirement that gameplay progress survives restarting.

**Details deliberately deferred:** NPC identity/placement and whether this
is the existing Warden or another NPC; relationship to the main story and
whether encounters are optional or required; encounter
availability beyond the preceding-stage prerequisite;
mob identities, distinctive traits, weaknesses and discovery clues;
resistance/HP/damage values; which boss is the balance benchmark; reward
types/amounts; replaying already-completed stages and reward-claim rules;
re-summoned mob health/state; leaving an active encounter, quitting/reloading,
or the hero dying to another enemy; simultaneous defeats; and summon limits
or costs, if any.

This adds a future encounter requirement to the earlier main-story-only quest
scope; it does not yet settle whether it belongs to that main quest or is a
separate challenge. It does not replace the approved boss-unlock rules or
change the currently implemented Mosswatch quest.

**Treasures and secrets:**  
- What can a hidden treasure contain?Percentage boost of stats or stats directly (permanent)
- Should secrets be hinted at, discoverable by observation, or very obscure?
  Should discoverable by observation
- Can a treasure/secret be found only once, and should it stay found after
  replaying a section?It should stay there but as looted

**Story:**  
- Desired tone (for example lighthearted, mysterious, heroic): Heroic
- Who is the hero, and what do they want? They want to get stronger, grind the stats and complete main quest
- What is the threat or reason to visit new sections?To get stats, get stronger and complete main quest
- How should story be presented (short dialogue, scenes, environmental clues)?Short dialoque 
  
- Any themes/content to include or avoid? Don't have any in mind, recommend something

**Campaign continuation requirement (owner, 2026-11-19):** Completing all
20 locations on this map unlocks the next map with new locations. The story must
resolve this map's main conflict while leaving room for the hero's continuing
journey.

The proposed campaign, **The Unbroken Road**, is documented in the
[story draft](story.md), with an overarching plot, a beat for every approved
location, a complete first-map ending, and a next-map hook. Its new names,
characters, lore, and plot are proposals awaiting owner review, not approved
canon or implemented gameplay. Existing Mosswatch dialogue is preserved.

## 8. Progress persistence and offline behavior

**Should the first release work without an internet connection?** Yes

**What must survive closing/restarting the app?** (For example: equipment,
section unlocks, quests, treasure discoveries.)  
Everything

**Multiple save slots or one save per device? Should be save newest progres, including player position, stats etc**  
**Should losing/replacing a device be allowed to lose progress in the first
release, or is cloud/account recovery required?\*\* No Clound for now, it will be added later  
**If a save is damaged, what recovery experience would feel acceptable? We will use cloud to =save players progress, but not now.** 


Cloud sync and paid ownership require extra security design. Mark these as
future requirements rather than relying on a local file to prove a purchase.

## 9. Accessibility, language, and presentation

- Launch language(s): English in first release
- Are text size/subtitles important? No
- Color/shape/animation alternatives needed for combat or loot readability?
  Recommend
- Adjustable music, effects, voice, haptics, or screen shake? Yes, in settings player can turn on/off
- Any photosensitivity, motion, or content considerations? Recommend
- Visual references you own or may legally share (do not attach ripped game
  assets): No,i don't, it havce to be designed. You will describe what images/animation etc is needed (exact sopecification of file type, size etc) and i will deliver it, if you will not be able to create it

## 10. Future business decisions (can wait until after the MVP)

- Is the game intended to be free, paid upfront, or undecided? Free-to-play;
  the player must be able to complete the entire game without spending money.
- Are in-app purchases only cosmetic, convenience, expansion content, or
  undecided? Optional progression acceleration only: purchases speed up progress
  rather than make completion possible.
- What must never be sold or randomized for money? [Your answer]
- Should the game ever include ads? (Currently excluded.) [Your answer]
- Is online account/cloud progress a future requirement? [Your answer]

**Owner-confirmed monetization principle, 2026-11-19:** All locations/maps,
story progression, quests, and boss victories must remain achievable without
paying. Do not require a paid item, boost, quest-mob weakness solution, or
content unlock to complete the game. Purchases may reduce the time/effort needed
to progress, not create a paid-only completion path.

Specific products, pricing, acceleration amounts/durations, limits, and economy
details remain undecided. This direction does not authorize implementation now:
no purchase, premium currency, ad integration, analytics SDK, or account system
should be built until the remaining product decisions and security/privacy
requirements are reviewed. Existing future purchase-authority requirements
remain unchanged.

## 11. Acceptance examples

Write player-visible rules in this format:

`Given [starting state], when [player action/event], then [observable outcome].`

1. Given a new player at the first exit, when the boss is undefeated, then
   the exit remains locked.
2. Given the boss is defeated, when the player closes and reopens the game, then
   the boss progress is saved and the exit remains unlocked.
3. Given the player dies, when the 3-second respawn countdown completes, then
   the hero returns to the spawn point with half maximum HP and loses no rewards
   or progress.
4. Given the player loots a treasure, when they revisit the area, then the
   treasure remains marked as looted.
5. Given a Basic HP or Attack subtype is defeated, then its deterministic stat
   reward is shown and increases the corresponding stat.
6. Given any enemy starts an attack, when it starts its next attack, then the
   start-to-start cycle is 1.0 second and its wind-up resolves before damage.
7. Given the hero leaves any enemy's attack range and does not hit it for
   5 seconds, then the enemy, including the boss, returns toward its original
   spawn point and heals at 10% of its maximum HP per second, capped at maximum
   HP; if it arrives before full HP, it remains at spawn and continues healing
   until fully restored unless combat resumes.
8. Given a mob is chasing or returning, when the hero hits it, then the mob
   stops returning/healing and its 5-second no-hit timer resets.
9. Given an enemy kills the hero, when the respawn countdown begins, then that
   enemy retreats toward its spawn and no enemy attacks the hero for 3 seconds.
10. Given the killer is retreating after the hero's death, when the hero hits it
    before the 3-second lock expires, then it resumes normal combat immediately;
    otherwise it resumes normal behavior when the lock expires.
11. Given a respawnable mob is defeated, when its cooldown is pending, then a
    marker follows the spawn point and shows the mob name, remaining time,
    elapsed progress bar, and visual-only pips for remaining daily respawns.
    Used pips are muted, and the marker is hidden when the mob is alive or its
    spawn point is outside the usable screen area. The pips reflect the
    prototype's UTC-day reset behavior.
12. Given an idle enemy is outside twice its attack range, then it remains idle;
    when the hero enters that focus range but remains outside attack range, the
    enemy starts chasing and only engages an attack inside attack range.
13. Given the hero and living mobs are visible, then each has a banner above its
    figure showing its name, current/max HP, and health-bar fill; when a mob is
    defeated, its banner is removed.
14. Given the hero is alive, then automatic healing continues during movement
    and combat, capped at maximum HP; pause and the death countdown do not heal.
15. Given the Warden's quest is accepted and the boss defeated, approaching the
    Warden grants the authored stat reward once; restarting cannot repeat it.
16. Given the boss is defeated, crossing the eastern gate records first-section
    completion; the section stays explorable without pretending a second map
    exists.

### Implemented first-section realization (2026-11-19)

The owner requested completion of the first map section including graphics.
The following are assistant-selected, reversible realization/content values,
not replacements for the owner's answers or final balance approval:

| Content | Current implementation |
| --- | --- |
| Section | Mosswatch Hollow; original low-poly woodland ruins, camp, lantern road, guardian dais, and eastern gate |
| Hero | Armored Vanguard silhouette with the existing axe/head/torso loadout; walking and attack animation |
| Ordinary enemies | Mossbound/HP, Ironbound/Axe Attack, and Ruin Keeper/Advanced silhouettes; existing stable IDs, stats, rewards, and respawn rules preserved |
| Boss | Hollow Guardian; existing ordinary-mob-style behavior with higher HP/damage, no new boss phase or extra drop |
| Main story | “The silent watch”; approach the Warden's `!` at the western camp to accept, defeat the guardian, return for a short conclusion |
| Quest reward | Once-only +2 permanent Health points and +2 Axe Attack points; quest completion is not an extra boss-gate requirement |
| Treasure | Southern grove, hinted by pale stones and an inscription; once-only +3 permanent Health points, visibly open/looted afterward |
| Hero healing | 2 HP/second while alive, including combat; active gameplay time only; capped at max HP, never revives a dead hero |
| Presentation | Fixed-angle following camera, target/wind-up rings, hit feedback, damage numbers, projected HP banners, safe-area HUD, nonmodal dialogue, circular touch joystick |
| Completion | Cross eastward through the opened gate; persist completion and allow continued exploration/grinding in this section |

Healing-point sources/scaling remain unspecified; the base healing rate does
not invent such a source. Ordinary enemy values remain provisional and the
guardian is intended to require movement/timing rather than stationary trading.
No new weapon catalog, inventory, skin progression, second section, audio pack,
monetization, or online system is implied. Art provenance and references are in
the [asset ledger](assets.md); decisions and save compatibility are in the
[decision log](decision-log.md).

## 12. Open questions and sign-off

| Question | Your current answer or "recommend" | Must be decided before |
| --- | --- | --- |
| How do head and torso armor values combine, and when are armor, block, dodge, and critical-hit checks applied? | Recommended in section 6: add matching armor, apply critical damage before armor, dodge avoids all damage, and apply block after armor; confirm before implementation. | Combat implementation |
| How many HP/Axe Attack points does each Basic subtype reward? Are collected points permanent? | The prototype grants 1 permanent point for the matching stat on each Basic defeat. Confirm the amount and permanence. | Reward tests |
| What are enemy wind-up values? What does the Advanced enemy do/drop? | Focus range is now twice attack range for every enemy; the wind-ups remain prototype values to tune in playtests. Advanced currently has no stat reward; its distinctive threat/reward remain open. | Combat prototype |
| Are the reference HP and example armor values in section 6 acceptable, and does the hero start with armor equipped? | The graybox currently equips 2 head + 3 torso Axe armor. This is not approved; test with both 0 and 5 armor and choose the starting loadout. | Combat implementation |
| Does the three-respawn daily limit apply per enemy, spawn point, or area? When does a new day begin? | The prototype assumes per spawn point and resets at the next UTC calendar day. Confirm or change this scope/timezone. | Respawn implementation |
| How does the hero heal, and which sources provide Healing, critical, Dodge, and Block points? | It is auto heal, alway applied, the same for other stats | Progression implementation |
| How are additional weapon slots unlocked? Can shields occupy a slot that otherwise attacks? | Later decision | Before adding multi-weapon progression/shields |
| What are the boss's attacks and can boss rewards be repeated? | Now lets the boss be tha same as mobs but it is stronger with damage and hp | Boss/content implementation |

**Things you want me to research or propose rather than decide on your behalf:**  
Check the best practicies for mobile games, best earning games based on micro purchaces, we chould use same logic.

**Owner approval/date after review:** 19.11.2026

---

Once the first-slice answers are filled, share the document back. I can then
turn them into a concise approved game brief, identify conflicts, and create
testable implementation tasks. Product feel and final balance remain yours to
approve through playtesting.
