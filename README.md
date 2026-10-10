# GrindHero

A mobile, fixed-camera 3D action RPG for iOS and Android, built with Unity and C#.
The player fights enemies, gains stats and loot, completes quests, discovers
secrets, and defeats bosses to unlock connected map sections.

**Status: first-slice graybox implemented and Editor-tested; not yet validated
on mobile devices.** The Unity project is in `game/`, pinned to Editor
`6000.6.5f1` and URP `17.6.0`. Its generated scene passes 9 EditMode tests and
1 PlayMode smoke test, and the repository readiness precheck passes. Android
and iOS player builds, physical-device testing, and licensed CI remain open.
No backend or purchasing SDK is installed.

## Start here

1. Open the existing Unity project in `game/` using the pinned Editor; follow
   [getting started](docs/getting-started.md) for setup and validation.
2. Review the open prototype assumptions in the
   [gameplay specification](docs/gameplay-specification.md), then follow the
   ownership-marked [next steps](docs/roadmap.md).
3. Read the [architecture](docs/architecture.md) before adding game code.
4. Use the [development workflow](docs/development.md), [testing strategy](docs/testing.md),
   and [security requirements](docs/security.md) throughout implementation.

| Path | Responsibility |
| --- | --- |
| `game/` | Unity project and first-slice graybox prototype |
| `docs/` | Design, architecture, decisions, roadmap, and operational guidance |
| `scripts/` | Dependency-free PowerShell repository and Unity-readiness checks |
| `.github/` | CI workflows, issue/PR templates, and Copilot app instructions |

Repository conventions are in [CONTRIBUTING.md](CONTRIBUTING.md);
release policy is in [versioning and delivery](docs/delivery.md).
Architecture decisions are recorded in [ADRs](docs/adr/README.md).

## Checks available now

On Windows, from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-Repository.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-Repository.Tests.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-UnityChecks.Tests.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-UnityProjectReady.ps1
```

The Windows commands use a process-local policy for these reviewed repository
scripts; they do not change the machine's execution policy. On managed hosts,
follow your organization's policy. On macOS/Linux with PowerShell 7 installed,
use `pwsh` and omit `-ExecutionPolicy Bypass`. The Unity readiness check validates
project files, package pins, metadata, assemblies, test presence, and the build
scene; it does not compile or run the game. Editor test commands and current
results are in [getting started](docs/getting-started.md). Those tests cover
rules, persistence, and scene boot/pause, not full device behavior or final
combat balance.

## AI-assisted development

AI tools assisted with parts of this repository's design, code, tests, and
documentation. See [AI-assisted development and provenance](docs/ai-usage.md).
This notice does not mean every file was generated entirely by AI and is not,
by itself, a claim of legal or regulatory compliance.

## Scope and decisions

Confirmed: Unity/C#, iOS/Android, simple 3D visuals, fixed camera, equipment and
skins, quests and secrets, lightweight story, boss-gated sections, extensibility,
and future in-app purchases.

Starting architecture: single-player offline gameplay, Universal Render Pipeline
(URP), GameObjects rather than ECS, and data-driven content. Offline-first is a
starting assumption, not a promise of cloud sync, multiplayer, or a trusted
offline paid economy. The graybox currently uses landscape orientation,
one-finger virtual-joystick movement, and automatic nearby targeting; control
feel and the supported device floor still require owner review and physical
testing. Business-model decisions remain open. The project Editor is pinned to
Unity `6000.6.5f1` (Unity 6.6 Update) in ADR-0001.

The `game/` directory contains the Unity project and its pinned package
manifest/lock. I can manage Unity installed in this session's Windows workspace,
but cannot sign in to or activate your personal Unity license, test on your
physical devices, access your Apple/Google developer accounts, accept
third-party terms, or handle signing secrets. You own the creative/game-feel
decisions; I can turn your answers into proposals, implementation, and testable
tasks. See [ADR-0001](docs/adr/0001-unity-mobile-foundation.md) and
[getting started](docs/getting-started.md).

This repository is not assigned an open-source license. Until the owner chooses
one, it does not grant permission to redistribute the code or assets.
