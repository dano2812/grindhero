# Versioning, CI, and delivery

## Independent version axes

| Version | Policy |
| --- | --- |
| App | SemVer: `0.y.z` before stable release, then `1.0.0`; document breaking changes |
| Store build | Monotonically increasing Android `versionCode` and valid increasing iOS `CFBundleVersion` per store rules |
| Save schema | Explicit integer and tested ordered migrations; independent of app SemVer |
| Content schema/catalog | Explicit format version and stable IDs; record compatibility when changed |
| Unity/packages | Exact editor patch and package lock committed; upgrades in dedicated PRs |
| Backend API later | Explicit compatibility policy and independent deploy version |

Do not use a branch name as a version or assume store build numbers can decrease
on rollback. Allocate build numbers centrally before parallel release jobs
(a reserved counter or timestamp-based scheme validated against platform limits).
Choose and implement the allocator in the first distribution task; no allocator
is present yet. Unity bundle version, Android code, and iOS build version must
match the release manifest.

Use annotated `vX.Y.Z` tags for approved releases; pre-release tags can be
`v0.1.0-alpha.1`. Generate a changelog from reviewed commits, retaining human
notes for save changes, known issues, and player-visible changes. Do not publish
an empty "release" for this documentation foundation.

## What is wired now

- **Repository foundation:** PR/push/manual workflow, Windows and Linux,
  dependency-free PowerShell checks plus link/readiness/report regression tests;
  Linux also validates workflow syntax with a checksum-pinned actionlint binary.
  No secrets are used.
- **Unity tests:** manual workflow only, with a readiness gate that fails clearly
  until the project exists and includes package locks/tests/scene setup.
  Licensed execution uses a protected `unity-ci` environment. Missing licensing
  fails; it is not silently reported as a passing game test.
- Actions are pinned to immutable commits; Dependabot proposes reviewed updates.
  The GameCI CLI is also explicitly pinned and needs deliberate updates; its
  Unity container must be verified and digest-pinned during M1 toolchain setup.
- No signing, mobile build, distribution, release, backend, or store upload
  workflow exists yet. Their gates below must be implemented and exercised.

Configure `unity-ci` environment restrictions/reviewers and Unity license secrets
before dispatch. The supplied workflow uses a GameCI-supported license file plus
Unity email/password; verify the current activation flow for your license.
For a license-server/serial setup, change the workflow in a reviewed PR.
Do not weaken account passwords to work around tooling; fix escaping or select
a supported activation approach.

## Target pipeline introduced by milestone

| Stage | Trigger/trust | Evidence and output |
| --- | --- | --- |
| Foundation | Every PR/push, hosted ephemeral runner, no secrets | Docs/link/checker checks |
| Unity compile and tests (M1) | Trusted main pushes; manual approved contributor revisions | Clean import, EditMode/PlayMode reports, player compilation |
| Android development build (M1/M2) | Trusted main/manual, least-privilege credentials | IL2CPP/ARM64 development APK for device smoke |
| iOS export/archive smoke (M1/M2) | Trusted main/manual, macOS with pinned editor/Xcode | Xcode export and device-installable development build |
| Beta (M5) | Approved release candidate/protected environment | Signed Android AAB/internal track; iOS archive/TestFlight |
| Production (after gates) | Approved immutable tag and environment | Store upload, manifest, changelog, retained artifacts |
| Paid services (M6) | Independent staged backend pipeline | Sandbox contracts, ledger migrations, reconciliation evidence |

Do not supply Unity account secrets to arbitrary fork PRs. Once M1 needs PR
engine validation, choose a supported trust arrangement: secret-free licensed
ephemeral infrastructure, or a maintainer-approved immutable revision copied to
a controlled branch/environment. Approval is for the exact revision; changing
it requires reapproval. Trusted push branches must be permission-controlled.
Foundation checks alone must not be called a complete gameplay merge gate.

Cache Unity `Library` only as an acceleration, keyed by OS/editor/platform and
package/content/settings hash; clean import must work without cache. Never cache
signing material or sensitive save/token data. Use finite job timeouts and artifact
retention. Archive test XML/logs on failure and sanitize before sharing externally.
Download/copying an artifact is not proof it installs on a device.

## Branch protection setup

Enable protection for `main`, require the foundation matrix checks, prevent force
pushes/deletion, and resolve review conversations. Add engine checks after their
first verified run. Require another approval once there is another maintainer;
keep the solo process executable. Enable secret scanning/push protection where
the repository plan permits. These settings are instructions, not remotely
configured by this foundation.

## Release checklist

- Exact source commit/tag, editor/packages, toolchain, content catalog, and build
  numbers recorded in a release manifest.
- Required tests pass, real device matrix passes, performance budgets met.
- Previous production saves migrate; unrecoverable changes block rollout.
- Release backend/stripping/architecture/settings confirmed; no development
  console, debug credentials, sandbox endpoints, or unnecessary permissions.
- Signing, entitlement, privacy disclosures, age rating, asset attribution,
  screenshots, support/contact, and store review requirements checked.
- Beta evidence and gradual rollout plan approved; monitor crashes/ANRs, save
  failures, and later purchase failures with privacy-compliant diagnostics.

Prefer forward fixes. Store rollback means redistributing a compatible version
with a **higher build number**, not rolling users back to an older save schema.
Keep recovery builds compatible with already-migrated saves. Remote content
rollback is a separate concern and is out of scope until remote content exists.
