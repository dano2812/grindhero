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
