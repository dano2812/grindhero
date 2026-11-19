# Asset and dependency ledger

No third-party game-art or audio pack is used. Unity/URP and other engine
packages are installed; `game/Packages/manifest.json` and
`game/Packages/packages-lock.json` are the package/version authority.
Record each acquisition before use; an asset-store download does not establish
the right to redistribute its source.

| Asset/package | Origin/vendor | Exact version or revision | License/seat restrictions | Attribution | Owner/review date |
| --- | --- | --- | --- | --- | --- |
| Mosswatch meshes, palette, and character prefabs | Original, AI-assisted project source in `FirstSectionArt.cs`; assets under `game/Assets/_GrindHero/Art` | First-section realization, 2026-11-19 | No external art acquisition; review applicable AI/contributor terms before redistribution | Project AI-use notice | Repository owner / 2026-11-19 |
| Animation, damage numbers, UI, and joystick disc | Original project C# presentation code | First-section realization, 2026-11-19 | No external image, rig, animation, or font download | Project AI-use notice | Repository owner / 2026-11-19 |
| LegacyRuntime font and primitive geometry | Unity Editor built-in resources | Editor `6000.6.5f1` | Unity terms apply; not a separately acquired asset pack | As required by Unity terms | Repository owner / 2026-11-19 |

## Original first-section art

The Editor authoring tools create faceted meshes, shared URP materials, six
segmented character prefabs, a woodland island, pines, ruins, lanterns, a hinged
chest, and an eastern gateway. Runtime code adds displacement-driven walking,
axe wind-up/swing poses, restrained hit flashes, target/attack rings, and damage
numbers. No copied characters, screenshots, music, or downloaded game models
are embedded. Rebuilding through **GrindHero > Build First Slice** preserves
existing asset GUIDs but intentionally replaces generated scenes and resets
builder-owned tuning; review local scene/content edits first.

Shared materials and meshes, static scenery, a fixed-angle orthographic camera,
one directional light, and no new rendering dependency keep the starting art
pipeline simple. These choices do not establish device performance; profiling,
supported-device budgets, and final art/game-feel approval remain open.

## Presentation research, not asset sources

Reviewed on 2026-11-19:

- [Archero official listing](https://apps.apple.com/mr/app/archero/id1453651052):
  readable stylized silhouettes and a clear separation between actors and floor.
- [Mighty Doom designer case study](https://vincentchandesigns.com/portfolio-items/mighty-doom/):
  HUD hierarchy, readable combat states, concise feedback, and keeping the action
  area unobstructed.

Applied as general principles: a restrained woodland/teal/gold palette, chunky
actors, edge-anchored status/objective plates, visible attack wind-ups, and short
nonmodal dialogue. The graphics and layout are original; no claim about revenue
or monetization is inferred from these references.

For original art/audio, record the author, source location, and ownership or
commission agreement. For packages, also record mobile/AOT compatibility,
maintenance risk, and why the dependency is necessary. The package manifest and
lockfile are the installed-version authority once Unity is bootstrapped.

Do not import ripped assets, copied copyrighted characters/music, or assets with
unclear rights. Confirm contributor and AI-tool licensing terms where relevant.
Choose the repository's code license explicitly before any public distribution.
