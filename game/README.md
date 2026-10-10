# Unity project

This is the Unity project root, created from the official **Universal 3D**
template. The pinned Editor is `6000.6.5f1` (Unity 6.6 Update); the package lock
pins URP `17.6.0`. See [getting started](../docs/getting-started.md) for setup
and [ADR-0001](../docs/adr/0001-unity-mobile-foundation.md) for the version
decision.

## Project files

The tracked Unity project lives here inside the repository. Keep `Assets`
including `.meta` files, `Packages/manifest.json`,
`Packages/packages-lock.json`, and shared `ProjectSettings`. Do not commit
`Library`, `Temp`, `Logs`, `UserSettings`, local IDE files, build output, or
signing credentials. Preserve this README alongside the Unity project.

Target structure after M1 (create folders in Unity so metadata is generated):

```text
game/
  Assets/
    _GrindHero/
      Code/
        Domain/           # GrindHero.Domain; no UnityEngine dependency
        Application/      # GrindHero.Application; use cases and ports
        Infrastructure/   # GrindHero.Infrastructure; save/platform adapters
        Presentation/     # GrindHero.Presentation; MonoBehaviours, views, input
        Bootstrap/        # GrindHero.Bootstrap; explicit composition
        Editor/           # GrindHero.Editor; content validation/build tools
      Content/
        Definitions/      # Item, enemy, quest, loot, and section definitions
        Sections/         # One owned directory per map section
        Shared/           # Shared art/audio/prefabs, not gameplay code
      Scenes/             # Bootstrap and first playable scene
      Settings/           # Render/input configuration
      Tests/
        EditMode/
        PlayMode/
    ThirdParty/           # Isolated licensed vendor assets
  Packages/
    manifest.json
    packages-lock.json
  ProjectSettings/
    ProjectVersion.txt
```

Folders and assembly definitions should appear as needed during M1/M2, not as a
large empty architecture. Do not put reusable gameplay code in vendor folders.
Authoring and runtime boundaries are detailed in the [architecture](../docs/architecture.md).
