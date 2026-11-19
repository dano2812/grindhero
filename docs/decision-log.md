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

### DEC-013 - Complete the first authored section now

- **Date:** 2026-11-19.
- **Decision maker:** Owner.
- **Decision:** Continue implementation through the whole first map section,
  including attractive graphics informed by other mobile games.
- **Scope:** Realize the existing combat, main-story NPC, permanent-stat treasure,
  and boss-gated exploration in one playable section. This supersedes DEC-011's
  sequencing restriction on first-section content, not the physical-device,
  iOS, licensed-CI, performance, or release gates.
- **Non-goals:** A second map, a broader item/inventory/skin system, new boss
  phases, third-party asset purchases, online services, monetization, or release
  publishing. These still require their own scope and decisions.

### DEC-014 - Original low-poly Mosswatch presentation

- **Date:** 2026-11-19.
- **Decision maker:** Assistant, realization under DEC-013.
- **Context/options:** Replace the graybox with original editor-authored art
  rather than copying reference-game assets or blocking on an external art pack.
- **Decision:** Mosswatch Hollow is a stylized woodland-ruins island with
  faceted pines, a lantern road, a Warden camp, a southern grove, a guardian dais,
  and an eastern gateway. Use a moss/teal/stone/gold palette, shared meshes and
  materials, six segmented character prefabs, and a fixed 55-degree orthographic
  camera (size 8, clamped following focus).
- **Readability:** Walk/axe animation, target highlighting, expanding wind-up
  rings, small damage numbers, brief hit flashes, projected HP banners, and
  edge-anchored safe-area HUD plates. Dialogue is short and nonmodal. Hide both
  joystick pieces when idle; use an original circular disc when dragging.
  Keep the treasure clearing free of foreground trees. Match hero/mob attack
  ring radii to their authored weapon/range, and clear the old target highlight
  when the hero dies.
- **Research:** Archero's official listing and the Mighty Doom designer case
  study informed general silhouette/HUD/readability principles only. Links and
  provenance are in the [asset ledger](assets.md); no external art was copied.
- **Consequences/status:** Unity Editor APIs generate meshes, prefabs, definitions,
  and scenes while preserving existing GUIDs. Authoring is an explicit action,
  not part of every APK build. Scenery uses static renderers, not static
  billboard labels or the moving chest/NPC. Rendered opening, story, grove, and
  guardian previews were inspected and used to correct occlusion/framing.
  Device performance, accessibility/feel, and final art approval remain open.

### DEC-015 - First-section story, discovery, and hero recovery values

- **Date:** 2026-11-19.
- **Decision maker:** Assistant realization/tuning; the owner selected automatic
  healing, an exclamation-mark quest giver, main story, permanent-stat treasure,
  and an ordinary-mob-style stronger boss in the worksheet.
- **Decision:** Approach the Warden to accept “The silent watch”; defeat the
  Hollow Guardian and return for once-only +2 Health and +2 Axe Attack points.
  A pale-stone trail/inscription hints at a southern chest worth +3 permanent
  Health points; it remains visibly open once looted. Cross the opened eastern
  gate to record completion and keep exploring. Boss defeat alone still opens
  the gate; turning in the quest is not an extra unlock prerequisite.
- **IDs:** `section.mosswatch`, `quest.mosswatch.warden`, and
  `treasure.mosswatch.grove`. Existing enemy/spawn IDs and combat/respawn rules
  are preserved. Story/rewards are authored in `Mosswatch.asset`, not save labels.
- **Healing:** Start at 2 HP/second, active while alive including combat, capped
  at max HP, paused with gameplay time and inactive during the death countdown.
  Returning mobs retain their separate 10%-of-max-HP/second rule. This implements
  the owner's healing direction; the numeric base is provisional, with no
  invented Healing-point source or scaling.
- **Consequences:** Quest and treasure claims are idempotent application-layer
  operations; reward totals are computed before assigning state. Dialogue and
  chest state bind to saved IDs. A second section and new equipment mechanics
  are not implied. The guardian still needs human combat-balance review.

### DEC-016 - Save schema 2 and first-section verification

- **Date:** 2026-11-19.
- **Decision maker:** Assistant implementation choice.
- **Decision:** Add accepted/completed quest, looted treasure, and completed
  section ID collections in schema 2. Migrate schema-1 saves on read without
  changing stats, position, boss completion, or enemy cooldowns. Validate IDs,
  duplicates, and completion prerequisites; retain explicit startup/save errors
  and last-known-good backup recovery.
- **Compatibility/recovery:** The next successful save writes schema 2 using the
  existing temporary-file/backup replacement. Old builds cannot read schema 2;
  copy the save directory before downgrading. A pre-upgrade backup is not a
  permanent rollback copy because later saves rotate the normal `.bak`.
  No progress reset is performed.
- **Verification:** Scene authoring and all 12 EditMode/11 PlayMode tests pass,
  including healing/death/pause, migration/replay, and the authored quest/chest/
  boss/gate flow across reload. Tests use single-scene isolation to avoid
  duplicate objects from the Editor's open scene; preview rendering uses actual
  Unity graphics. The progression integration test directly defeats the boss
  and does not certify human combat balance.
- **Android:** The updated IL2CPP/ARM64 development APK built successfully; its
  manifest, native libraries, and debug signature were inspected. Artifact size,
  hash, and run configuration are in [getting started](getting-started.md).
  No Android device is connected. iOS/device/thermal/soak and licensed-CI checks
  remain unverified; none is silently marked complete.

### DEC-017 - Approved connected world and 20 location themes

- **Date:** 2026-11-19.
- **Decision maker:** Owner, accepting the proposed 20-location theme list.
- **Decision:** Use one large, connected world. Begin in Mosswatch Hollow;
  defeating each location's boss permanently unlocks the next, harder
  location. The hero remains free to cross between unlocked locations and
  revisit earlier regions. Persist unlocks across restarts.
- **Content source:** Preserve the approved names, visual themes, and
  progression order in the
  [gameplay specification](gameplay-specification.md#approved-world-progression-and-location-themes),
  from Mosswatch Hollow through The Eclipse Throne. Link to that list rather
  than maintaining competing copies.
- **Status and consequences:** Approved design direction only; Mosswatch is
  the sole implemented location. Detailed geography, connecting paths, later
  encounters, quest/reward content, and difficulty tuning remain future work.
  The owner requested documentation, not implementation of the other 19
  locations. Existing MVP scope proposals and device/performance/release gates
  are unchanged.

### DEC-018 - A complete first-map story with room to continue

- **Date:** 2026-11-19.
- **Decision maker:** Owner for the continuation requirement; assistant for the
  proposed story treatment.
- **Confirmed requirement:** Create a story for the approved map that can
  continue after all 20 locations are finished, when the next map with new
  locations unlocks.
- **Draft, not approved canon:** [The Unbroken Road](story.md) proposes a
  lantern-road network, Regent Veyr, a five-act arc across the approved location
  sequence, a genuine liberation of the first map, and contact with survivors
  beyond its final gate. The first map resolves its own conflict; later maps
  can explore the larger mystery without undoing that victory.
- **Boundaries:** Preserve the existing Mosswatch Warden quest/dialogue, heroic
  tone, short mobile-friendly storytelling, permanent unlocks, and backtracking.
  No new mechanics, collection requirements, scenes, dialogue assets, or save
  changes are implemented. The next map's location count/themes and the larger
  mystery's resolution remain undesigned.

### DEC-019 - Per-map summoned quest mobs and weakness-based challenges

- **Date:** 2026-11-19.
- **Decision maker:** Owner.
- **Clarification:** The original map-versus-location interpretation below and
  the then-open count/summoning questions are superseded by DEC-020. The
  distinctive mobs, discoverable weaknesses, and greater rewards still apply.
- **Decision:** Each map has a quest-giver NPC who summons staged quest mobs.
  Map 1 has one mob and Map 2 has two; a later stage becomes available only
  after defeating the preceding mob. Each mob is distinctive and highly
  resistant relative to expected hero strength, but has a discoverable weakness
  that makes it beatable before the relevant boss. Each quest-mob defeat
  provides a greater reward than an ordinary mob, not just a final-series reward.
- **Source of truth:** The
  [gameplay specification](gameplay-specification.md#location-quest-giver-and-summoned-quest-mobs)
  records the requirements and deferred questions. Maps are multi-location
  worlds; these quest encounters do not replace location-unlocking bosses.
- **Scope/status:** Documentation only, with details to be specified later.
  Do not invent resistance/reward values, particular weaknesses, later-map
  counts, a summoning interaction, replay/claim rules, or an optional/mandatory
  classification. NPC identity and the connection to the main story are open.
  This extends the earlier quest scope without changing the existing Warden
  implementation, approved boss gates, scenes, or saves.

### DEC-020 - Per-location counts, confirmed summons, and defeat retries

- **Date:** 2026-11-19.
- **Decision maker:** Owner.
- **Scope clarification:** When asked whether counts increase across the
  20 locations or separate multi-location maps, the owner selected
  "Across the 20 locations within each map." Correct DEC-019's earlier
  interpretation: each location has its quest giver and location N has N
  sequential quest mobs, from 1 in Mosswatch Hollow to 20 in The Eclipse Throne.
- **Summoning:** The player must go to the quest giver and explicitly confirm
  each summon. Defeating a stage unlocks the next, but never automatically
  summons it. The same confirmation is required for retries.
- **Failure:** A quest mob disappears after it kills the hero. It does not
  automatically respawn; the player must request another summon of the
  undefeated stage. Failure does not advance the sequence or undo completed
  stages.
- **Status:** Documentation only. Preserve the earlier weakness/large-reward
  requirements. Detailed stats, rewards, replay/claim policy, re-summoned mob
  state, and other encounter lifecycle edge cases remain open. No Unity
  gameplay, scenes, or saves are changed.

### DEC-021 - Required location difficulty and farming-time tooling

- **Date:** 2026-11-19.
- **Decision maker:** Owner.
- **Decision:** A tool for calculating/testing each location's difficulty and
  farming time to boss readiness must be part of development. Record it as a
  required deliverable, not optional post-content cleanup.
- **Pacing examples:** The owner suggested roughly 1 hour of farming/play in
  the first location before defeating its boss, increasing to roughly 1-3
  months of farming at the final location. These are illustrative targets;
  exact durations, skill assumptions, daily play frequency, and the distinction
  between calendar months and accumulated active play remain open. No hard
  waiting gate or current tuning change is approved by these examples.
- **Development implications:** Use authored rules/data and expected carried-over
  hero progress; account for combat, rewards, cooldowns, daily limits, and
  future weakness-based quest encounters. Expose scenario assumptions and
  unreachable progression. Verify model predictions against game behavior;
  calculated pacing is not evidence that the game is fun.
- **Status/source:** Documentation only; the tool is not implemented.
  Requirements are in the
  [gameplay specification](gameplay-specification.md#required-difficulty-and-progression-pacing-tool),
  validation expectations in the testing strategy, and implementation task
  S-06 in the roadmap. Existing gameplay values, scenes, and saves are unchanged.

### DEC-022 - Full free-to-play completion and optional paid acceleration

- **Date:** 2026-11-19.
- **Decision maker:** Owner.
- **Decision:** The player must be able to complete the entire game free-to-play.
  Future in-game purchases only speed up progression; they are not required to
  defeat bosses, complete quests, or access locations/maps and the ending.
- **Balancing consequence:** The required difficulty/pacing tool must establish
  a no-purchase baseline. Evaluate DEC-021's illustrative progression durations
  against that baseline; compare paid acceleration separately when defined.
  A paid-only prerequisite or unreachable free progression violates this rule.
- **Open details/status:** Products, prices, acceleration values/durations,
  limits, and economy rules are not yet specified. This records direction only;
  no purchase SDK, economy, backend, scenes, or saves are implemented or changed.
  Existing security/privacy and authoritative purchase-verification gates remain.

### DEC-023 - AI Act alignment as a release-review requirement

- **Date:** 2026-11-19.
- **Decision maker:** Owner.
- **Requirement:** The game should align with applicable AI legislation; the
  owner asked for the most important EU AI Act considerations.
- **Recorded approach:** Keep AI-assisted development distinct from actual
  in-game AI functionality; review intended use and provider/deployer roles,
  literacy, applicable transparency duties, manipulation/vulnerability risks,
  provenance, and related privacy/consumer/IP obligations before release.
  Preliminary guidance and official sources are in
  [AI-assisted development and provenance](ai-usage.md).
- **Status:** Research/planning guidance, not a legal opinion, audit,
  certification, or authorization to add AI features. Release-specific
  applicability remains to be assessed with qualified legal advice.

## Open ideas and questions

The current source of truth for unanswered gameplay and product questions is
the [gameplay specification's sign-off section](gameplay-specification.md#12-open-questions-and-sign-off)
and the [game design decision gates](game-design.md#product-decision-gates).
Promote an item to this history only when the owner decides it or an
implementation choice is explicitly made and labeled as such.
