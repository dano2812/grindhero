# ADR-0001: Unity mobile client foundation

Date: 2026-10-10

Status: Accepted. Unity 6.6 Update `6000.6.5f1` is the pinned project Editor.

## Context

GrindHero targets a simple-looking fixed-camera 3D action RPG on iOS and Android.
The developer has senior software experience but is new to mobile games.
Content must grow without rewriting core progression. Purchases are future scope.

## Decision

- Unity and C#, explicitly selected by the repository owner.
- Unity `6000.6.5f1` (Unity 6.6 Update) with URP `17.6.0` is the pinned project
  baseline, selected by the repository owner to proceed with the first slice.
  Keep the exact Editor patch and package lock; review upgrades in dedicated
  changes and follow Unity's Update release cadence.
- Unity 6.3 LTS candidate `6000.3.26f1` was rejected after a clean Universal 3D
  template failed to compile in Unity-supplied URP `17.3.0` editor code because
  `MaterialReferenceChanger` was missing. Do not patch the vendor package. A
  future LTS migration may be evaluated separately after clean-import, test,
  and platform-build evidence.
- Universal Render Pipeline with GameObjects and ordinary C# rules.
- One repository with a nested `game` client and docs/tooling outside Assets.
- Assembly-enforced domain/application/adapter/presentation boundaries.
- Data-driven content with stable IDs and independent versioned player state.
- Offline single-player starting assumption, with paid authority deferred to a
  server boundary before purchases. No backend or purchasing SDK now.

## Alternatives

Godot/GDScript offers an open-source engine but was not selected. Unreal adds
unnecessary complexity for this starting scope. Unity ECS, custom engines,
microservices, and a generic plugin framework add learning/operational overhead
without evidence of need. A root-level Unity project is possible, but nesting
keeps docs/tooling outside Unity import and leaves future services independent.

## Consequences

Unity Update releases require a deliberate upgrade cadence. Licensing and CI
activation need explicit setup. iOS signing/archive needs macOS/Xcode and
appropriate developer access. Text assets/meta ownership and content validation
become collaboration requirements. Offline saves cannot secure a real-money
economy. No app-store compliance or physical-device performance is established
by this foundation.

## Validation and revisit

M1 verifies a clean import and compile on the pinned Editor, then proves package
compatibility, tests, and both device builds. M2 proves the combat slice and
budgets. Revisit the Editor pin through a reviewed migration if the engine fails
supported-platform requirements, measured scene scale needs different
loading/ECS, or multiplayer, cloud sync, remote content, or purchases change
the trust model.
