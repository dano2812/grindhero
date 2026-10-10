# Roadmap and task structure

M0 is delivered. The project now also has an Editor-validated first-slice
graybox and local rule/persistence/scene tests. M1 is **not complete** until
licensed platform builds and physical-device verification pass. The entries
below are a local planning backlog, not already-created GitHub issues or
delivery-date promises.

## Recommended next steps and ownership

**You** means a decision or real-world action only you can provide. **I can**
means repository documentation, code, test, and workflow work I can do in this
session after its prerequisites exist. **Together** means I implement a
testable version and you judge its design/behavior. I can manage Unity
installations and run the Editor in this session's Windows workspace where
permissions allow. I cannot sign in to or activate your personal Unity license,
use physical devices, access your Apple/Google developer accounts, accept store
terms, or handle signing secrets. These external actions need you; the
associated repository work can still be done here.

1. **You — Review the open decisions in [the gameplay specification](gameplay-specification.md).**
   The first-slice behavior is recorded and implemented as a prototype. Confirm
   the starting armor, reward permanence/amounts, respawn-limit scope and reset,
   Advanced-enemy reward, boss chase behavior, and combat balance. **I can:**
   explain trade-offs and update code/tests/data. **You decide:** game feel,
   scope, and final balance.
2. **Together — Confirm licensed builds and test devices.** Unity Hub and
   Unity `6000.6.5f1` Update are installed in this session. The owner selected
   this Update release to proceed after the `6000.3.26f1` LTS candidate failed
   to compile in its bundled URP. Android/iOS module availability and license
   eligibility still need verification through actual platform builds.
   Identify the actual iOS and Android devices available for testing. iOS builds
   require macOS and Xcode for local archive/signing; if your main development
   computer is Windows, arrange a Mac or approved Mac build host. **I can:**
   retest a future LTS patch and its toolchain. **You must:** accept applicable
   Unity terms and confirm license eligibility and test devices.
3. **I can — Bootstrap the Universal 3D project at `game` (completed).** The
   project uses Editor `6000.6.5f1`, URP `17.6.0`, and a resolved package lock.
   The first-slice scenes, data assets, and assemblies have been generated;
   **9 EditMode and 1 PlayMode test pass** in the Editor. **You must:** accept
   applicable Unity terms and verify platform builds on physical devices.
4. **Together — Review the first-slice interaction choices.** Landscape,
   one-finger virtual-joystick movement, a following fixed-angle camera, and
   automatic nearby targeting are in the graybox. **I can:** refine the
   prototype and tests. **You must:** choose the feel after trying it on actual
   screens.
5. **I can — Implement M1's reusable foundation and first slice (initial implementation complete).**
   The graybox includes hero movement, axe auto-attacks, head/torso armor,
   Basic HP/Attack rewards, an Advanced enemy, a boss-gated exit, pause/resume,
   versioned local saves, and a test-only scene. Damage/respawn/boss values are
   tunable prototype assumptions, not final decisions. **You must:** review
   these assumptions and report desired changes.
6. **Together — Play and tune the first playable section.** **I can:** adjust
   content values, behaviors, feedback, and automated coverage. **You must:**
   run the graybox on iOS and Android devices and report where controls,
   difficulty, camera, or feedback do not feel right; approve final design and
   commissioned/licensed art.
7. **You — Enable licensed CI and device build access when ready.** Configure a
   protected `unity-ci` environment and licensing secrets without sharing them
   in chat or committing them. Arrange an iOS/macOS build host and target-device
   smoke tests. **I can:** maintain workflows, add build/test validation, and
   debug sanitized logs. **You must:** provide licenses, account access,
   environment approvals, and real-device verification.
8. **Together — Expand only after the vertical slice passes review.** Approve
   stats, equipment/loot, quests, secrets, story, and a second section in the
   worksheet; then we can implement them in small tasks. **I can:** build
   data-driven systems/content validators and tests. **You must:** approve
   creative direction, provide/commission licensed assets, and accept the MVP
   scope.
9. **You — Make release and monetization decisions later.** Before beta,
   arrange developer accounts, privacy/store disclosures, signing, support
   contact, and release permissions. Purchases are a separate optional
   milestone after product, privacy, and server-authority decisions. **I can:**
   prepare/review code and secure workflows. **You must:** hold the accounts,
   credentials, legal approvals, and final publication authority.

**Current immediate action:** review and playtest the generated graybox, then
confirm the open decisions in the gameplay specification. Keep the current
enemy damage, reward amounts, starting armor, daily-cap interpretation, and boss
behavior provisional until you approve them after playtesting.

## Milestones and exit gates

| Milestone | Depends on | Outcome / exit gate |
| --- | --- | --- |
| M0 Foundation | None | Documented structure/rules/decisions, templates, working foundation CI |
| M1 Bootstrap and mobile proof | M0 | Pinned Unity `6000.6.5f1` Update/URP project with clean import/compile, useful assemblies/tests, both physical-device builds, licensed CI |
| M2 Vertical slice | M1 | Fun one-section combat/boss/unlock loop, restart persistence, measured device budgets |
| M3 Systems | M2 | Data-driven loot/equipment/stats, versioned recoverable saves, repeatable balance tests |
| M4 Content pipeline and MVP | M3 | Second section added without section-specific code; quest/story/secret/skin authored and validated |
| M5 Hardening and beta | M4 | Device/support matrix, accessibility/privacy/store readiness, signed beta distribution |
| M6 Purchases (optional, future) | M5 + approved business/trust decisions | Authoritative server ledger, sandbox lifecycle tests, approved purchase-enabled beta |

Do not implement future milestones merely to fill folders. No multiplayer or live
operations milestone is implied.

**Current evidence:** Editor scene generation and the local EditMode/PlayMode
test suites pass. The repository readiness precheck also passes; it verifies
project structure and metadata, not compilation, tests, licensing, or platform
builds. No Android/iOS player build, physical-device test, licensed CI run, or
performance soak has been completed.

## Dependency-ready task backlog

| ID | Task | Prerequisites | Acceptance / validation |
| --- | --- | --- | --- |
| F-01 | Establish repository foundation | None | M0 files/checks/templates available; current delivery |
| B-01 | Bootstrap Unity 6.6 Update/URP | F-01 | Exact Editor/modules/package lock/meta verified; clean project import and compile |
| B-02 | Establish assemblies and rule/scene tests | B-01 | Dependencies enforce architecture; nonzero EditMode/PlayMode suites pass |
| B-03 | Compare touch/orientation/device floor | B-01 | Two control candidates on named iOS/Android devices; decision recorded |
| B-04 | Produce Android and iOS smoke builds | B-02, B-03 | Both physical installs; pause/safe-area/input evidence; pinned toolchains |
| B-05 | Enable trusted engine CI/protection | B-02 | Licensed clean tests pass; fork trust/approval documented; required checks configured |
| S-01 | Hero movement, fixed camera, graybox section | B-03, B-04 | Touch movement and occlusion readable on baseline devices |
| S-02 | Combat rules and enemy lifecycle | B-02, S-01 | Health/damage/death, 1-second enemy attack cycles, 5-second chase/return with 10%-of-max-HP-per-second healing and player-hit interruption/reset, Basic HP/Attack variants, Advanced enemy, deterministic rewards, respawn and pause tests |
| S-03 | Boss progression and locked exit | S-02 | Boss completion unlocks exactly once; interrupted/replayed completion tested |
| S-04 | Minimal save snapshot and recovery | S-03 | Restart retains unlock; corrupt/interrupted writes recover explicitly |
| S-05 | Slice performance/fun review | S-04 | 15-minute device soak; approved numeric budgets and combat direction |
| Y-01 | Item definitions, stats, equipment | S-05 | Stable IDs, slot rules, stat examples/tests, first visual equipment |
| Y-02 | Loot and inventory | Y-01 | Seeded rolls, full-inventory behavior, valid tables, no duplicate rewards |
| Y-03 | Save schema and migration fixtures | Y-02, S-04 | Old slice saves migrate; inventory/quest extension documented; recovery tests |
| C-01 | Section/quest catalog and validators | Y-03 | Duplicate/reference/cycle failures explicit; prerequisites tested |
| C-02 | Second section and story quest | C-01 | New section authored without name-based branches; progression/reload tests |
| C-03 | Treasure, easter egg, cosmetic sample | C-01, Y-01 | Optional discovery/replay rules and licensed visual sample |
| H-01 | Device/accessibility/quality pass | C-02, C-03 | Supported matrix/budgets met; critical regressions pass |
| H-02 | Privacy, identifiers, signing, beta CI | B-04, B-05 | Store/legal/contact decisions; protected secrets; build-number allocation implemented |
| H-03 | Internal beta and recovery rehearsal | H-01, H-02 | Signed installs, prior-save migration, forward-fix plan demonstrated |
| P-01 | Monetization and backend ADR | H-03 | Product catalog/account/offline policy and authority reviewed |
| P-02 | Server verification/entitlement ledger | P-01 | Unique transactions, atomic grants, auth, audit, refunds/reconciliation tests |
| P-03 | Client IAP adapter and sandbox flows | P-02 | Pending/retry/restore/revoke/account-mismatch tests on both stores |
| P-04 | Purchase readiness review | P-03 | Security/privacy/store lifecycle gates pass before enabling paid products |

Implement a milestone as multiple small PRs. When creating issues, carry over the
prerequisites and expand acceptance criteria; row ordering is not a dependency.

## Task example: boss unlock

Outcome: defeating the first boss opens the next exit and survives restart.
Scope: one encounter/unlock; no new quest UI or backend.
Dependencies: S-02 and approved boss/exit IDs.
Acceptance: exit starts locked; boss death records completion/reward/unlock in
one state transition; replay gives no second reward; save/reload remains unlocked;
failure/interruption cannot create duplicate rewards or silently reset progress.
Validation: domain transition tests, PlayMode exit interaction, interrupted save
fixtures, physical-device suspend/resume. Evidence belongs in the PR.

## Estimation and decision discipline

Estimate only the next milestone after its decisions are resolved. Engine
learning, store enrollment, iOS access, and CI licensing are real prerequisites,
not hidden "setup" time. Use short spikes when uncertainty is high.
Record accepted design changes in ADRs; keep product choices in the game brief
and task. Revisit MVP scope based on the slice instead of scaling unproven combat.
