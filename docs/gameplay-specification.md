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
  chance checks earned through stat points; see section 6. Other actions and
  healing controls remain to be decided.

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

**Enemy attack rules:** Enemies start attacking when the hero comes within their
proximity/aggro range. All enemy types (Basic, Advanced, and boss) use a
1.0-second attack cycle, measured from the start of one attack to the start of
the next. Do not deal damage immediately when the hero first enters aggro range;
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
cooldowns in section 7. Whether the boss uses the same return behavior remains
open.

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
location. It does not have to be stronger than every other large enemy in the
current location. Its specific attacks and telegraphs remain to be designed.
Its attack cycle is 1 second. Whether it uses the mob return-and-heal rule
remains undecided; the current graybox lets it chase indefinitely and does not
heal it while returning.

**Death and respawn:**  
- What happens to the player's location and health? There will be respawn point, where will hero appear, with half of max HP
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
behaviors. Weapon definitions should own damage, cycle, wind-up, and range;
enemy definitions should own health, attack damage/type, cycle, wind-up,
telegraph, and special-attack timing; a shared combat-tuning asset should own
values such as the block multiplier and critical multiplier. Validate that
health and damage are non-negative and attack cycles are greater than zero.
Changing these values should not require changing combat code.

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

The generated graybox uses the following adjustable ScriptableObject values.
They are implementation assumptions to make the loop playable and testable;
they are not approved final balance or a commitment to the starting loadout.

| Actor/content | Current prototype values |
| --- | --- |
| Hero | 100 starting max HP; 15 base Axe damage + 5 axe-weapon damage; 1-second attack cycle with 0.25-second wind-up; starts with 2 head and 3 torso Axe armor equipped; 0% dodge/block; block multiplier 0.8; defeat respawn at 50% max HP |
| Basic — HP | 80 HP; 11 Axe damage; 0.5-second wind-up; 2.1 attack range / 8 aggro range; 3.2 movement speed; drops 1 permanent HP point (+5 max HP); 120-second respawn cooldown |
| Basic — Axe Attack | 80 HP; 13 Axe damage; otherwise shares the Basic HP profile; drops 1 permanent Axe Attack point (+1 base Axe damage); 120-second respawn cooldown |
| Advanced | 180 HP; 18 Axe damage; 0.65-second wind-up; 2.25 attack range / 9 aggro range; 3.4 movement speed; no stat reward yet; 300-second respawn cooldown |
| Boss | 450 HP; 22 Axe damage; 0.6-second wind-up; 2.5 attack range / 12 aggro range; 3.6 movement speed; no reward or respawn yet; currently chases indefinitely instead of returning/healing |

All enemy attack cycles are 1 second, as specified. Ordinary enemies currently
chase for 5 seconds after the hero leaves attack range and heal at 10% of max HP
per second while returning; healing stops once they reach their spawn point.
The three-respawn cap is currently **per spawn point per UTC calendar day**.
These interpretations still need owner review. The first-slice project does
not yet contain the five-slot inventory system, quests, treasures, story
content, or equipment drops.

## 7. Sections, exploration, quests, and story

**Section structure:**  
- Are sections linear, branching, or connected through a hub?Section should be neighboring areas
- Can the player revisit completed sections? Yes, player can return to the location, mobs can respown for a limiterd times, and the counter is resetted on new day
- Do ordinary enemies respawn when the player returns? There is respawn cooldown (also running when the player is not in the game)
- Are there checkpoints or safe areas? No
- Precisely what unlocks the next section?Defeating boss unlocks new section, in first slice there will be only one location

**First-slice mob respawn rules:**
- Basic mobs: 2-minute respawn cooldown.
- Advanced mobs: 5-minute respawn cooldown.
- Maximum: 3 respawns per day. The cooldown continues while the app is closed.
- Current prototype interpretation: the cap is per spawn point and resets at
  the next UTC calendar day. Cooldowns and counts are saved locally and continue
  while the app is closed. This scope/timezone is an implementation assumption,
  not an owner-approved design decision.

**Quests:**  
- Should quests be automatically accepted, explicitly accepted, or either?There should be some entity with exlamation mark which tells the quest

- Which objectives are interesting? (Examples: defeat, explore, collect, talk.) Defeat and collect 
  
- Can optional quests be failed or abandoned? No optional quests now., Just the main story quest
- What should quest rewards/progression be? Stat rewards and unlocking special areas

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

- Is the game intended to be free, paid upfront, or undecided? [Your answer]
- Are in-app purchases only cosmetic, convenience, expansion content, or
  undecided? [Your answer]
- What must never be sold or randomized for money? [Your answer]
- Should the game ever include ads? (Currently excluded.) [Your answer]
- Is online account/cloud progress a future requirement? [Your answer]

No purchase, premium currency, ad integration, analytics SDK, or account system
should be built until these choices and the security/privacy requirements are
reviewed.

## 11. Acceptance examples

Write player-visible rules in this format:

`Given [starting state], when [player action/event], then [observable outcome].`

1. Given a new player at the first exit, when the boss is undefeated, then
   the exit remains locked.
2. Given the boss is defeated, when the player closes and reopens the game, then
   the boss progress is saved and the exit remains unlocked.
3. Given the player dies, when they return to the game, then the hero respawns at
   the spawn point with half maximum HP and loses no rewards or progress.
4. Given the player loots a treasure, when they revisit the area, then the
   treasure remains marked as looted.
5. Given a Basic HP or Attack subtype is defeated, then its deterministic stat
   reward is shown and increases the corresponding stat.
6. Given any enemy starts an attack, when it starts its next attack, then the
   start-to-start cycle is 1.0 second and its wind-up resolves before damage.
7. Given the hero leaves an enemy's attack range and does not hit it for
   5 seconds, then the enemy returns toward its original spawn point and heals
   at 10% of its maximum HP per second, capped at maximum HP.
8. Given a mob is chasing or returning, when the hero hits it, then the mob
   stops returning/healing and its 5-second no-hit timer resets.

## 12. Open questions and sign-off

| Question | Your current answer or "recommend" | Must be decided before |
| --- | --- | --- |
| How do head and torso armor values combine, and when are armor, block, dodge, and critical-hit checks applied? | Recommended in section 6: add matching armor, apply critical damage before armor, dodge avoids all damage, and apply block after armor; confirm before implementation. | Combat implementation |
| How many HP/Axe Attack points does each Basic subtype reward? Are collected points permanent? | The prototype grants 1 permanent point for the matching stat on each Basic defeat. Confirm the amount and permanence. | Reward tests |
| What are each enemy's aggro range and wind-up? What does the Advanced enemy do/drop? | Prototype ranges, wind-ups, and attack damage are listed above; tune them in playtests. Advanced currently has no stat reward; its distinctive threat/reward remain open. | Combat prototype |
| Are the reference HP and example armor values in section 6 acceptable, and does the hero start with armor equipped? | The graybox currently equips 2 head + 3 torso Axe armor. This is not approved; test with both 0 and 5 armor and choose the starting loadout. | Combat implementation |
| Does the boss use the same return-and-heal behavior as other mobs? | The graybox currently lets the boss chase indefinitely and does not return/heal. Confirm whether this exception is intended. | Boss implementation |
| Does the three-respawn daily limit apply per enemy, spawn point, or area? When does a new day begin? | The prototype assumes per spawn point and resets at the next UTC calendar day. Confirm or change this scope/timezone. | Respawn implementation |
| How does the hero heal, and which sources provide Healing, critical, Dodge, and Block points? | [Your answer] | Progression implementation |
| How are additional weapon slots unlocked? Can shields occupy a slot that otherwise attacks? | Later decision | Before adding multi-weapon progression/shields |
| What are the boss's attacks and can boss rewards be repeated? | [Your answer] | Boss/content implementation |

**Things you want me to research or propose rather than decide on your behalf:**  
[Your answer]

**Owner approval/date after review:** [Your answer]

---

Once the first-slice answers are filled, share the document back. I can then
turn them into a concise approved game brief, identify conflicts, and create
testable implementation tasks. Product feel and final balance remain yours to
approve through playtesting.
