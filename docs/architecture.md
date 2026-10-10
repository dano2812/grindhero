# Architecture

## Baseline

Use Unity/C#, Universal Render Pipeline, and GameObjects/MonoBehaviours for
presentation. Prefer ordinary C# gameplay rules over engine-bound logic.
Start as a modular single-player client, not microservices or an ECS framework.
Scale primarily by authoring content and maintaining clear dependency boundaries.

## Assembly dependency direction

An arrow below means "references"; enforce it with `.asmdef` files in M1/M2.

```text
Domain          -> no project assembly; no UnityEngine
Application     -> Domain
Infrastructure  -> Application, Domain
Presentation    -> Application, Domain
Bootstrap       -> Application, Infrastructure, Presentation, Domain
Editor          -> runtime assemblies only as needed; Editor platform only
EditMode tests  -> tested assemblies; test assembly only
PlayMode tests  -> tested assemblies; test assembly only
```

Set `noEngineReferences: true` for Domain and Application. Put Unity-dependent
serialization/assets and SDK integrations in adapters. Bootstrap is the only
runtime layer that selects concrete adapters; presentation cannot directly read
files, call stores, or create backend clients. Test assemblies must not enter
player builds. Editor tooling must not be referenced by runtime assemblies.

Create only useful boundaries, not one assembly per class. Within Domain and
Application, organize by feature: Combat, Inventory, Progression, Quests, World.
Avoid cycles and general-purpose "Managers" that own unrelated state.

| Layer | Owns | Does not own |
| --- | --- | --- |
| Domain | Stats, damage, loot rules, equipment rules, quest/section state | Frames, GameObjects, disk, networking |
| Application | Commands/use cases, transaction orchestration, adapter ports | Render/input details, store SDK types |
| Infrastructure | Versioned save I/O, clocks/random adapters, future store/backend adapters | Deciding paid entitlement validity |
| Presentation | Touch input, animation, camera, audio, UI, Unity object lifetimes | Authoritative progression/economy rules |
| Bootstrap | Composition root, startup/load order, shutdown | Global service locator |
| Editor | Content validation, authoring tools, build-time checks | Code that ships in player builds |

Use explicit constructor dependencies for plain C# classes and serialized
references for scene components. Introduce interfaces at real I/O or substitution
boundaries, not for every class. No DI framework, global mutable singleton bus,
or speculative plugin registry is needed.

## Content model

Use ScriptableObjects as editor-authored definitions. Map them to immutable
runtime values at the boundary; never mutate definition assets as player state.
Saved references use stable IDs, not Unity instance IDs, array indices, asset
paths, localized names, or scene names.

- Items: stable item ID, equipment category/slot, stat modifiers, rarity, visuals.
- Enemies: stable enemy ID, stats, behavior preset, loot table, visuals.
- Loot: table ID, weighted entries, quantities, explicit fallback/no-drop rules.
  Inject randomness into rules so tests use a deterministic source.
- Quests: quest ID, prerequisite IDs, typed objectives, reward IDs, story/text keys.
- Sections: section ID, scene/asset reference, prerequisite section/boss IDs,
  spawn definitions, quest hooks, optional treasure IDs, and exit definitions.
- Bosses: stable encounter ID. Completion records reward claim and unlock state
  as one progression operation; replaying completion cannot grant twice.

Validate duplicate/missing IDs, invalid weights/quantities, incompatible equipment,
missing references, prerequisite cycles, unreachable required sections, and
missing localization keys in editor tooling and CI. Reject invalid catalogs
explicitly; do not silently replace corrupt content with defaults.

## Adding a map section

1. Create a section-owned content folder, scene/prefabs, and definition in Unity.
2. Assign stable section, encounter, treasure, and quest IDs; never reuse retired IDs.
3. Wire prerequisites, boss completion, exits, spawn data, and story/localization keys.
4. Validate the catalog and test locked/unlocked, boss replay, treasure replay,
   save/reload, and old-save compatibility.
5. Capture device performance evidence for the section's worst supported encounter.

Existing combat code must not need a `switch` on section name. Keep only the
active section loaded initially; load the next section asynchronously and expose
loading/failure feedback. Add Addressables only when measured loading/memory or
remote content requirements justify its lifecycle and catalog complexity.

## Runtime and persistence

Separate frame/time-driven presentation from explicit gameplay state transitions.
Define pause/background behavior so enemies, damage, and rewards cannot advance
unexpectedly while the app is suspended. Pool frequently spawned entities and
effects when profiling shows allocation pressure, not every object by default.

Saves have a schema version independent of app version, stable content IDs, and
ordered tested migrations. Persist a consistent snapshot, write to a temporary
file in the same directory, then use platform-verified atomic replacement with
a last-known-good backup. Test interruption on each target platform. Startup
distinguishes first run, corrupt save, migration failure, and unsupported future
schema. Recovery must show the user what happened; do not silently erase progress.
Do not assume arbitrary file replacement APIs behave identically across platforms.

Offline saves are untrusted even if checksummed, encrypted, or signed with a
client-held key. Future paid entitlements and paid currency need server-owned
records separate from local combat progress. Do not implement a backend until
that milestone, but do not couple save DTOs to store SDK receipts.

## Extension rule

New content uses existing definitions. A new behavior gets a focused use case,
rule, or component and tests. Extract a shared abstraction only after a real
second consumer or a clear external boundary. Remote configuration, ECS,
networking, procedural generation, and broad event frameworks require a new ADR.
