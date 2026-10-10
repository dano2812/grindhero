# Contributing

Read [development](docs/development.md), [testing](docs/testing.md), and
[security](docs/security.md) before changing implementation.

## Definition of ready

A task has a clear player/developer outcome, in/out scope, dependencies, measurable
acceptance criteria, validation approach, and any unanswered decisions. Resolve
architecture or product blockers before implementation; do not disguise guesses
as approved requirements.

## Definition of done

- Acceptance criteria demonstrated, with automated tests at the appropriate layer.
- Existing behavior preserved or intentional changes documented.
- Assembly boundaries respected; no new package without rationale and review.
- Related design/ADRs/setup documentation updated.
- No secrets, personal data, generated Unity caches, or unlicensed assets committed.
- Unity assets include their original `.meta` files; no missing script references.
- Applicable editor tests and builds pass; player-facing changes exercised on a
  representative device, with performance evidence for affected hot paths.
- Known limitations and migration/rollback implications recorded in the PR.

For foundation-only changes, Unity/device checks are not applicable. State that
explicitly rather than reporting tests that did not run.

## Collaboration

Use short-lived branches and small PRs against `main`; one coherent task per PR.
Use the issue templates and link the task in the PR. Prefer Conventional Commits
(`feat:`, `fix:`, `docs:`, `test:`, `build:`, `ci:`, `refactor:`, `chore:`).
Require review when another developer is available. While solo, use the same
checklist and a deliberate self-review; do not create unfulfillable approval rules.

Do not modify scenes or prefabs concurrently without coordinating ownership.
Use Unity Smart Merge where supported; never resolve serialized conflicts by
blindly accepting one side. Preserve GUIDs when moving assets.
