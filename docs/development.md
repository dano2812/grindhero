# Development workflow

## Working in increments

Keep `main` releasable once M1 establishes a build. Use short-lived branches and
small reviewed PRs; avoid permanent development/platform branches. Drive work
through issues with one testable outcome, not a large "implement RPG" ticket.

Use milestones from the [roadmap](roadmap.md). A GitHub Project can track
Backlog -> Ready -> In progress -> Review -> Done, with Blocked as an explicit
status. Suggested fields: milestone, area, priority, dependencies, and device
evidence. Templates exist, but remote issues/project boards are not created by
this foundation. Add labels only after agreeing on a small useful taxonomy.

Priority: P0 data loss/security/build blocker; P1 current milestone critical path;
P2 improvement. Limit work in progress to one implementation task per developer.
Time-box research spikes and conclude with evidence and a decision, not unused code.

## Planning format

Use a task issue for scope/acceptance; use its PR for implementation evidence.
A larger feature plan should live in the issue or an explicitly requested design
document and contain: outcome, current behavior, options/decision, affected
assemblies/content/saves, incremental tasks with real prerequisites, risks,
validation, rollout, and rollback. Do not accumulate transient planning files in
the repository. Record lasting architectural decisions as ADRs.

## C# and Unity rules

- Follow Unity-supported C#/.NET APIs for the pinned editor. Do not upgrade
  language targets independently of Unity or assume desktop APIs work on mobile.
- Use PascalCase for public types/members and camelCase for locals/parameters.
  Use `_camelCase` private fields, explicit access modifiers, and feature namespaces.
- Prefer small plain C# classes for rules. `MonoBehaviour` coordinates lifecycle
  and engine objects; it is not the domain model.
- Validate external/content input at boundaries. Surface actionable errors;
  do not broadly catch exceptions and return success/default progress.
- Keep allocations, LINQ, scene searches, and disk/network calls out of hot
  per-frame paths unless profiling demonstrates acceptable cost.
- Cancel/clean up async work, event subscriptions, input actions, and pooled
  objects on disable/unload. Do not keep hidden state across play sessions.
- Inject clock/randomness/I/O when determinism or substitution is needed.
- Do not store progress in ScriptableObjects or treat `PlayerPrefs` as a save
  database, secure token store, or source of paid ownership.
- Compile all platform branches; editor-only paths can conceal Android/iOS bugs.
  Exercise AOT/IL2CPP serialization and stripping in player builds.

## Asset ownership and merge strategy

Create/move assets through Unity so `.meta` GUIDs follow them. Commit folders'
metadata as well as files'. Do not hand-author scenes or replace GUIDs to fix
missing references. Prefer prefab composition and section-owned directories to
one scene edited by everyone.

Configure Unity Smart Merge (`UnityYAMLMerge`) locally with the installed editor;
do not commit machine-specific executable paths. Verify any resolved scene
conflict in the editor. Store proprietary source art in the repository only when
ownership/license permits; track origin, license, and attribution in the asset
ledger before importing third-party content.

Git LFS is intentionally not enabled yet. Before large art/audio imports, assess
hosting quota and workflow support; if selected, add targeted binary patterns,
install LFS for all collaborators/CI checkouts, and migrate in a coordinated PR.
Do not put Unity text scene/prefab/meta files in LFS or turn it on without CI support.

## Review and maintainability

Each review checks gameplay correctness, state transitions, lifecycle cleanup,
save compatibility, content references, platform support, errors, and evidence.
Treat assembly boundaries and tests as enforceable architecture, not just prose.
Add a package only if Unity built-ins or a small implementation do not meet the
need; record purpose, version, license, maintenance, mobile/AOT compatibility,
and removal risk. Commit dependency locks; no floating Git package references.

Introduce Roslyn/static analysis supported by the pinned Unity version in M1,
and make player compilation a gate. Formatting alone is not analysis. Keep
builds free of new warnings; baseline existing warnings explicitly during import.
