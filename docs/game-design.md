# Game brief

## Confirmed vision

A readable, simple-looking 3D mobile action RPG with a fixed-angle camera. The
hero kills enemies to earn stats and drops, equips multiple weapon/armor types,
and uses cosmetic skins. Short quests, hidden treasures, easter eggs, and a
lightweight story provide reasons to explore. Defeating a section's boss unlocks
the next section.

Core loop: enter section -> fight/explore -> earn loot and progress -> improve
equipment -> defeat boss -> unlock next section -> reveal next story beat.

## First playable slice

One graybox section, a hero with one basic axe and head/torso armor, a fixed
camera, one-handed touch movement, and automatic combat. Include Basic enemies
with two reward variants (maximum-HP points or Axe Attack points), one Advanced
enemy, and one boss. Rewards are deterministic; the locked exit opens after the
boss, and that progress persists after restart. Prioritize readable feedback
and enjoyable combat before adding breadth.

MVP extends this with two connected sections, a small equipment catalog, one
quest chain, one hidden treasure, one cosmetic, basic story beats, inventory,
settings, and recoverable local saves. This is a proposed scope cap; do not
interpret "several weapons" as an unlimited content commitment.

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
