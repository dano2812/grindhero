# Testing strategy

## Layered evidence

| Layer | What it proves | Required timing |
| --- | --- | --- |
| Foundation PowerShell checks | Required docs/workflows, local links, readiness/report checker regression behavior | Every PR/push now |
| EditMode rule tests | Damage/stats, equipment, seeded loot, quests, unlocks, migration rules | Each gameplay PR after M1 |
| EditMode content tests | IDs/references, prerequisite graph, loot weights, asset completeness | Each content PR after validators exist |
| PlayMode integration tests | Composition, input/lifecycle, encounters, UI binding, scene transition | Each affected PR after M1 |
| Android/iOS player compilation | Platform APIs, stripping/AOT, SDK/package compatibility | Trusted main builds; release candidates |
| Physical-device smoke/performance | Touch, graphics, suspension, thermal behavior, store integration | Player-facing milestones and releases |
| Backend/store sandbox tests | Verified entitlements, replay/refunds/retries | Before any purchase-enabled release |

Keep Domain/Application tests in Unity EditMode test assemblies with no engine
dependency in the tested code. No separate desktop runtime is necessary initially.
Tests must be isolated, deterministic, and independent of execution order. Use
temporary save locations, explicit test seeds/clocks, and cleanup. Fail on zero
discovered tests in established suites. Do not mask flaky tests with retries;
quarantine only with an issue, owner, and expiry.

## Critical regression cases

The current first-section suites contain 12 EditMode and 11 PlayMode tests.
They cover healing and combat rules, persistence/backup recovery, schema-1
migration, idempotent treasure/quest rewards, section prerequisites, and the
authored scene's quest/treasure/boss/gate loop across a scene reload. The
progression integration test directly defeats the boss; it is not evidence
that a human has balanced or completed the combat encounter.

PlayMode tests load their isolated scene in single-scene mode so the Editor's
currently open scene cannot contribute duplicate actors, cameras, or saves.
The graphics-capable preview test renders 1440x810 opening, dialogue, grove,
and guardian views into ignored `.artifacts/mosswatch-*.png`. These are staged
Editor views with actors held still, not device screenshots. Inspect the
images rather than treating successful PNG output as a visual-quality assertion.
Use a graphics-capable run (omit `-nographics`) for this suite.

- Combat: zero/dead health, damage boundaries, equipment changes, repeated death,
  pause/resume, and invalid stat data.
- Loot/inventory: deterministic boundary draws, zero/invalid weights, quantities,
  full inventory outcome, stable item IDs, and no duplicate reward grants.
- World/quests: prerequisites, boss replay, section transition failure, quest
  ordering, optional secrets, treasure replay, and story key resolution.
- Saves: first run, previous schema fixtures, migration idempotency where intended,
  corrupted/truncated file, interruption during write/replacement, backup recovery,
  unsupported future schema, and missing/retired content IDs.
- Lifecycle: suspend during combat/reward/save/transition, forced process death,
  network loss when future online features exist, and low-memory recovery.
- Platform: IL2CPP/ARM64 smoke, safe areas, aspect ratios, audio focus, touch
  cancellation, and release-only stripping differences.
- Purchases later: pending/cancelled/declined, retry after grant before
  acknowledgement, reinstall/restore, duplicate token, account mismatch,
  refund/revocation, offline cached entitlement expiry, and sandbox failures.

## Required difficulty and progression pacing tool

The owner requires a development tool for calculating/testing each location's
difficulty and farming time to boss readiness. Its authoritative requirements
and illustrative early/late pacing examples are in the
[gameplay specification](gameplay-specification.md#required-difficulty-and-progression-pacing-tool).
This tool is a required development deliverable, not yet implemented.

When developed, use it during location balancing and after changes to combat
stats, stat growth, rewards, respawn allowances, or encounter prerequisites.
Reports must retain the input-data version and scenario assumptions, separate
active-play time from calendar time, and make impossible or blocked progression
explicit. Include known-weakness quest-mob scenarios as their rules become
specified, rather than treating every enemy as a stationary HP comparison.

Verify the tool itself with small, independently calculable reference scenarios,
cooldown/daily-reset boundaries, and unreachable-boss cases. Compare its combat
and progression predictions with the actual game before relying on long-horizon
estimates. Exact acceptance tolerances and CI enforcement are not decided.
Model output complements, but never replaces, human combat/fun review or
physical-device verification.

The required baseline scenario uses no purchases or paid boosts and must show
that all progression remains achievable free-to-play. Apply the illustrative
early/late pacing targets to this baseline. When products are specified,
compare optional paid acceleration separately: it may shorten time/effort, but
must not be necessary for a quest, boss, location/map unlock, or completion.
Flag any paid-only prerequisite or unreachable free progression as a defect.

## Performance budgets (initial targets, not verified claims)

Select named baseline devices and tier in M1. Proposed floor: sustained 30 FPS
(frame time <= 33.3 ms) on the lowest supported tier, with an optional 60 FPS
tier (<= 16.7 ms). Record p95/p99 timings as well as averages during a
15-minute worst-encounter soak. A locked camera does not eliminate overdraw,
lighting, skinning, or UI costs.

Target zero steady-state managed allocations per frame in combat rule/update
paths after warmup; document unavoidable spikes. Set numeric RAM, texture,
draw-call, startup, scene-load, build-size, and battery/thermal budgets after
measuring the M2 slice. These unresolved budgets must be approved before content
expansion, not quietly left as "optimize later".

Profile development players with Unity Profiler/Memory Profiler and platform
tools, then confirm release player behavior. Store device model/OS, commit,
build settings, encounter, duration, and results in the task/PR. Editor FPS and
headless tests cannot certify mobile performance or visual quality.

## Quality gates

Coverage is diagnostic, not a substitute for assertions. Establish a measured
Domain/Application baseline in M2 and prevent regression; do not impose an
arbitrary global percentage on generated/vendor/UI code. Critical state and
migration paths require explicit behavioral tests regardless of coverage.

Before beta: both platforms install, startup succeeds, no missing references,
all critical cases pass, save migration/recovery passes, budgets are met on
baseline devices, and no unresolved release-blocking defects remain.
