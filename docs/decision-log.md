# Development decision log

This append-only log records development decisions made by the game owner and
implementation choices made by the assistant. The detailed, current rules
remain in the linked specification and ADRs. Historical entries below were
reconstructed on 2026-10-10 from those documents and session checkpoints; their
original timestamps were not retained.

## Recording rules

- Record every development decision made by the owner or assistant, including
  choices made during implementation. This covers product scope, player
  behavior, UX, tuning, architecture, implementation, tooling, testing,
  persisted data, and repository conventions.
- Identify who made or approved each choice. Mark assistant-selected,
  unvalidated behavior as provisional rather than attributing it to the owner.
- Include context, rationale, consequences, affected docs/code, and validation
  where useful. Use an increasing `DEC` ID.
- Append a superseding entry; do not erase an earlier decision or proposal.
- Keep detailed gameplay rules in the [gameplay specification](gameplay-specification.md).
  Keep unresolved questions there and in the [game design decision gates](game-design.md);
  do not record an open idea as accepted.
- Durable architecture, security, persistence, and trust-boundary choices also
  receive an [ADR](adr/README.md).

## History

### DEC-001 — Unity mobile-game foundation

- **Original date:** Not retained; reconstructed 2026-10-10.
- **Decision maker:** Owner selected Unity/C# and the game direction.
- **Status:** Accepted; see [ADR-0001](adr/0001-unity-mobile-foundation.md).
- **Decision:** Build a fixed-camera 3D action RPG for iOS and Android, using a
  data-driven Unity GameObject architecture. Unity `6000.6.5f1` and URP
  `17.6.0` are the pinned prototype baseline. Purchases remain future scope;
  no backend or purchasing SDK is part of the current slice.

### DEC-002 — First-slice interaction and equipment

- **Original date:** Not retained; reconstructed 2026-10-10.
- **Decision maker:** Owner.
- **Status:** Owner-specified; physical-device feel remains unvalidated.
- **Decision:** Landscape, one-handed movement with a floating one-finger
  joystick, fixed-angle camera that follows the hero, nearest-enemy targeting,
  and automatic attacks in range. The slice starts with an axe and head/torso
  armor, one unlocked weapon slot, and passive dodge/block rather than active
  skills. Future equipment may use up to five active weapon slots, with each
  equipped weapon attacking simultaneously.
- **Rationale and scope:** Keep the first combat loop simple while preserving
  room for data-driven equipment and progression. The full rules and open
  balance questions are in the gameplay specification.

### DEC-003 — Combat, rewards, and enemy respawns

- **Original date:** Not retained; reconstructed 2026-10-10.
- **Decision maker:** Owner specified the first-slice rules; prototype tuning
  remains assistant-selected where the specification labels it provisional.
- **Status:** Mixed: owner rules are specified; numeric balance and some scope
  decisions remain open.
- **Decision:** Matching armor reduces incoming damage; a successful block
  leaves the hero taking 80% of the hit. Basic HP and Basic Attack enemies give
  deterministic points to the corresponding hero stats. Basic mobs use a
  two-minute respawn cooldown, Advanced mobs five minutes, and the stated
  maximum is three respawns per day; cooldowns continue while the app is closed.
- **Important boundary:** The prototype interprets the daily cap as per spawn
  point and uses the next UTC calendar day as its reset. Those are not yet
  owner-approved choices. Advanced rewards, final balance, and other open
  decisions remain in the gameplay specification.

### DEC-004 — Hero death recovery and lethal-enemy retreat

- **Original date:** Not retained; reconstructed 2026-10-10.
- **Decision maker:** Owner requested the behavior; the three-second values
  were explicitly specified.
- **Status:** Implemented; PlayMode validation remains pending.
- **Decision:** After a three-second countdown the hero returns to the spawn
  point at half maximum health, without losing progress or rewards. The enemy
  that delivered the lethal hit retreats and cannot attack for three seconds;
  a hit from the hero cancels that lock. No enemy can damage the hero during
  the hero's countdown.
- **Source:** [Gameplay specification](gameplay-specification.md) and its
  acceptance criteria.

### DEC-005 — Initial cooldown HUD presentation

- **Original date:** Not retained; reconstructed 2026-10-10.
- **Decision maker:** Owner requested a cooldown visualization and a nearby
  visual-only daily respawn count; the initial fixed-panel layout and pip
  encoding were assistant implementation choices.
- **Status:** Superseded for location and pip meaning by DEC-007.
- **Decision:** The first implementation used an `MM:SS` timer, elapsed bar,
  and a list below target status. Filled pips represented respawns already
  used and outlined pips represented the remaining allowance.
- **History note:** Preserve this entry so the change in placement and pip
  meaning is traceable.

### DEC-006 — Temporary respawn-state reset for testing

- **Date:** 2026-10-10.
- **Decision maker:** Owner asked for a cooldown reset and approved also
  resetting daily counts for a clean test.
- **Status:** Completed as a local test-data operation; not a gameplay rule.
- **Decision:** The three regular mob entries were made alive with zero
  cooldown and zero daily usage. Hero progress and defeated-boss progress were
  preserved, and the original local save was backed up.

### DEC-007 — Respawn indicators at mob spawn points

- **Date:** 2026-10-10.
- **Decision maker:** Owner specified the spawn-point location and the need to
  show the remaining spawn count. The screen-space projection and visual
  styling are assistant implementation choices.
- **Status:** Owner requirement implemented and Editor compilation completed;
  PlayMode execution and visual review remain pending.
- **Decision:** Replace the global list with a compact screen-space marker
  projected above each defeated respawnable mob's spawn point. Keep the mob
  name, `MM:SS` remaining time, and elapsed cooldown bar. Show the remaining
  daily respawns as unlabeled pips: filled pips are still available; muted
  outlined pips have been used. Add no textual count. Hide a marker while the
  mob is alive or its spawn point is outside the usable screen area.
- **Rationale:** Keep status attached to the relevant place in the world while
  retaining legible, safe-area-aware mobile UI. This replaces DEC-005's fixed
  panel and used-pip emphasis; it does not resolve the prototype's per-spawn,
  UTC-day cap assumption.
- **Implementation follow-up (2026-10-10):** The PlayMode regression test
  checks spawn-point projection, marker contents, remaining/used pip colors,
  and the prototype UTC-day reset. Unity initially reported an ambiguous
  `Object` reference after the test added `System` date/time types; the cleanup
  call now explicitly uses `UnityEngine.Object.Destroy`. A subsequent Editor
  script recompile completed without compiler errors. The PlayMode test and
  mobile readability have not yet been verified.

### DEC-008 — Mob focus range derived from attack range

- **Date:** 2026-10-10.
- **Decision maker:** Owner.
- **Status:** Accepted and implemented; Unity recompilation and PlayMode
  execution remain pending.
- **Decision:** Every enemy's focus (aggro) range is exactly twice its attack
  range. Derive the focus range from the attack-range value rather than tuning
  it independently. An idle enemy remains idle outside this range, chases when
  the hero enters it, and only engages attacks within its attack range.
- **Rationale:** Reduce the distance at which mobs react to the hero while
  keeping detection consistent with each enemy's attack reach.
- **Current prototype values:** Basic enemies have a 2.1 attack / 4.2 focus
  range; Advanced has 2.25 / 4.5; the boss has 2.5 / 5.0. These focus values
  are derived, not separately serialized.
- **Implementation and validation:** `EnemyDefinition.FocusRange` supplies the
  derived range to idle acquisition and post-kill reacquisition; the separate
  serialized aggro field was removed from the definitions, assets, and scene
  builder. A PlayMode test covers remaining idle outside the range and chasing
  inside it. Repository documentation and readiness checks passed; the open
  Editor has not yet recompiled this update, so Unity compilation and PlayMode
  execution remain unverified.

### DEC-009 — Ordinary mob healing and actor health banners

- **Date:** 2026-10-10.
- **Decision maker:** Owner confirmed that mobs heal to full HP at 10% of
  maximum HP per second. Whether this included the boss was clarified later in
  DEC-010. The banner presentation is an assistant implementation choice.
- **Status:** Regular mobs used the confirmed rate and maximum-HP cap while
  returning, but previously stopped healing on reaching spawn if still
  injured. They now continue healing while idle at spawn until full. Regression
  coverage and actor banners are implemented. Repository and Unity readiness
  checks passed; Unity compilation, PlayMode execution, and mobile readability
  remain pending.
- **Decision:** While returning to spawn, regular mobs regenerate at a rate of
  `maximumHealth * 0.1` per second, capped at maximum health. If one reaches
  spawn before full health, it continues healing while idle there until full;
  combat interrupts recovery. Show a compact
  projected screen-space banner above the hero and each living mob, with actor
  name, current/max HP, and a proportional health bar. Hide banners outside the
  usable screen area and remove a mob's banner when it is defeated. Preserve
  the existing top-level hero stats and spawn-point respawn indicators.
- **Scope note:** At the time of this entry, the boss remained configured not
  to return-and-heal. DEC-010 records the owner's later decision to apply this
  behavior to the boss. Hero passive healing is not implemented or specified.
- **Rationale:** Make health and combat progress readable directly over each
  figure without replacing existing summary HUD information.
- **Implementation and validation:** `EnemyActor.MoveTowardSpawnAndHeal`
  applies the configured `0.1` maximum-health fraction per second and caps
  healing at the definition's maximum health; idle mobs now keep healing at
  spawn until full. The PlayMode suite covers the rate, cap, full recovery,
  projected banners, health text/bar updates, and removal after defeat. The
  existing Unity Editor processes have not rebuilt the edited scripts, so
  compilation and PlayMode execution remain unverified.

### DEC-010 — Boss return-and-heal behavior

- **Date:** 2026-10-10.
- **Decision maker:** Owner selected applying the ordinary mob return-and-heal
  behavior to the boss as well.
- **Status:** Implemented in the boss asset and scene builder; regression
  coverage checks the boss configuration. Unity compilation and PlayMode
  execution remain pending.
- **Decision:** The boss chases for the configured 5-second timeout after the
  hero leaves its attack range, then returns to its spawn point and heals at
  10% of maximum HP per second, capped at maximum HP. If it reaches spawn
  before full health, it continues healing while idle there until full. A
  player hit resets the chase timer and interrupts returning/healing as it does
  for other enemies.
- **Rationale:** Keep enemy recovery consistent across archetypes while
  allowing the boss's attacks and durability to distinguish the encounter.
- **Implementation and validation:** Set `returnAndHeal` to true in
  `Boss.asset` and `FirstSliceSceneBuilder`; removed the unresolved-boss
  tooltip. The PlayMode boot test checks the spawned boss is configured for the
  shared chase timeout and healing rate. Repository and readiness checks pass;
  Unity has not recompiled or run the test since these changes.

### DEC-011 - Provisional slice acceptance and progression clarifications

- **Date:** 2026-11-19.
- **Decision maker:** Owner.
- **Decision:** The current verification/playtest state is acceptable "for now";
  proceed with mobile smoke-build work and a checkpoint, not a claim that mobile
  gates have passed. The owner dated the gameplay worksheet approval and
  specified always-active automatic hero healing and an ordinary-mob-style
  boss distinguished by higher damage and HP.
- **Scope and status:** Preserve the existing slice and the owner's worksheet
  answers. Hero regeneration is still unimplemented; its rate/scaling and
  sources of other stat points remain unspecified. Boss reward repeatability
  is not answered by choosing its attack behavior. Monetization research is
  requested in the worksheet, not approval to add purchases or a backend.
- **Consequence:** Build/test on mobile before expanding equipment, inventory,
  loot, or section content. Physical-device testing and final balance approval
  still require the owner; iOS archive/signing requires a Mac/Xcode host.

### DEC-012 - Reproducible local Android smoke build

- **Date:** 2026-11-19.
- **Decision maker:** Assistant implementation choice under the owner's request
  to continue with Android smoke-build preparation.
- **Decision:** Add an Editor-only build action for the existing first-slice
  scene: development APK, IL2CPP, ARM64, default debug signing, and a temporary
  local-only `com.grindhero.smoke` identifier. Restore overridden build settings
  on success or failure; do not generate or retune gameplay content. Keep
  outputs/logs under ignored `.artifacts`, with no store upload or release keys.
- **Toolchain:** Use the pinned Editor's bundled Android SDK/NDK/OpenJDK.
  Windows Android tools reject the worktree's non-ASCII path; use a temporary
  ASCII drive alias to the same worktree and a process-local ASCII
  `GRADLE_USER_HOME`, then remove the alias and restore the environment.
  Keep the Android native build cache `game/.utmp` ignored. Preserve the
  pinned Editor's generated URP/settings serialization and Input System
  preloaded-asset reference; these changes came from the Editor, not hand edits.
- **Related fixes:** Unity compilation rejected the HP banners' obsolete
  `GetInstanceID`; use typed `EntityId` keys from `GetEntityId` in the HUD and
  its regression test. The respawn-marker test must configure the authoritative
  saved respawn state through the bootstrapper, not a transient view state that
  the normal respawn poll overwrites.
- **Validation:** After the owner refreshed Unity licensing, all 9 EditMode and
  8 PlayMode tests passed. The Android APK built with zero reported warnings;
  its identity, ARM64/IL2CPP content, and signature were inspected. Leftover
  performance-test JSON resources from failed runs were cleaned from the
  worktree before the successful build. Artifact details and the
  device handoff are in the getting-started guide. No device was connected;
  device acceptance is not implied by a successful APK build.

## Open ideas and questions

The current source of truth for unanswered gameplay and product questions is
the [gameplay specification's sign-off section](gameplay-specification.md#12-open-questions-and-sign-off)
and the [game design decision gates](game-design.md#product-decision-gates).
Promote an item to this history only when the owner decides it or an
implementation choice is explicitly made and labeled as such.
