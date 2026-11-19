# Game brief

## Confirmed vision

A readable, simple-looking 3D mobile action RPG with a fixed-angle camera. The
hero kills enemies to earn stats and drops, equips multiple weapon/armor types,
and uses cosmetic skins. Short quests, hidden treasures, easter eggs, and a
lightweight story provide reasons to explore. Defeating a section's boss unlocks
the next, harder section permanently. The approved world direction is one large,
connected map of 20 themed locations, with free travel between unlocked
locations and no loss of access to earlier regions. The authoritative ordered
theme list is in the
[gameplay specification](gameplay-specification.md#approved-world-progression-and-location-themes).

Core loop: enter section -> fight/explore -> earn loot and progress -> improve
equipment -> defeat boss -> unlock next section -> reveal next story beat.

The entire game must be completable free-to-play. Future in-app purchases are
optional progression accelerators, not requirements for content, boss victories,
or completion. Balance against a no-purchase baseline; exact products, pricing,
and acceleration values are undecided. This is confirmed direction, not
authorization to implement purchases in the current slice.

Completing the first map's 20 locations unlocks another map with new locations.
Each map's story should resolve its local conflict while supporting the hero's
continuing journey. See [The Unbroken Road story draft](story.md) for the
proposed first-map campaign and continuation hook; its creative details are not
yet approved canon.

Each location will also have a quest giver who summons unique, sequential
quest-mob encounters: location N has N mobs, from 1 at Mosswatch through 20 at
The Eclipse Throne. The player must visit the NPC and confirm each summon,
including the next stage or a retry. A quest mob disappears if it kills the
hero and must be summoned again; completed stages remain completed.
These mobs are highly resistant at
the expected hero strength, but discovering their weakness should allow victory
before the relevant boss is beatable. Every quest-mob defeat gives a greater
reward than an ordinary mob. This is recorded future scope, not implemented;
details and its relationship to the main story remain open in the
[gameplay specification](gameplay-specification.md#location-quest-giver-and-summoned-quest-mobs).

## First playable slice

One section, Mosswatch Hollow, a hero with one basic axe and head/torso armor, a fixed
camera, one-handed touch movement, and automatic combat. Include Basic enemies
with two reward variants (maximum-HP points or Axe Attack points), one Advanced
enemy, and one boss. Rewards are deterministic; the locked exit opens after the
boss, and that progress persists after restart. Prioritize readable feedback
and enjoyable combat before adding breadth.
The current realization includes original low-poly art, the Warden main quest,
and a persistent grove treasure; the other 19 location themes are not yet built.

MVP extends this with two connected sections, a small equipment catalog, one
quest chain, one hidden treasure, one cosmetic, basic story beats, inventory,
settings, and recoverable local saves. This is a proposed scope cap; do not
interpret "several weapons" as an unlimited content commitment. The approved
20-location world is the longer-term direction, not an automatic expansion
of this proposed MVP delivery scope.

## Product decision gates

Fill in [gameplay specification](gameplay-specification.md) to provide the
player-facing answers for the first slice. The [roadmap](roadmap.md) labels
decisions you own separately from repository work the assistant can perform.

| Decision | When | Evidence |
| --- | --- | --- |
| Portrait/landscape, thumb reach, controls, targeting/auto-attack | M1 | Two device-tested graybox control candidates |
| Camera projection/angle and occlusion behavior | M2 | Readability on smallest target screen |
| Minimum devices/OS and frame-rate/thermal tier | M1 | Representative iOS and Android hardware |
| Stat scaling, loot rarity, inventory limits, respawn/death penalty | M3 | Explicit rules and testable balance examples |
| Equipment visuals, skin rules, art style/asset ownership | M3 | Small art pipeline sample and license review |
| Story tone, first quest, accessibility/localization scope | M4 | Reviewed short content brief |
| Offline/cloud sync/account expectations | Before M5 | Product decision and threat model |
| Paid products, audience/age rating, ads/analytics | Before M6 | Privacy/store policy review and economy design |

Initially exclude multiplayer, PvP, leaderboards, trading, procedural world
generation, live operations, ads, and real-money purchases. Adding any of these
requires a scoped task and an ADR where trust or persistence changes.

## Player experience rules

- Combat feedback must distinguish damage, healing, misses, pickups, and death.
- Touch controls and UI must handle safe areas, pause, app backgrounding, and
  different aspect ratios. Do not rely on mouse-only interactions.
- Story and tutorials should not block repeated play; distinguish essential
  objectives from optional secrets and use stable localization keys.
- Do not hide essential progression behind random drops or a purchase.
- Boss rewards and section unlocks must survive interruptions without duplication.
- Treat color-blind readability, adjustable audio/haptics, and readable text as
  design constraints; validate accessibility scope at its decision gate.
