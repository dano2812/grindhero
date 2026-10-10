# Getting started

## Prerequisites and platform realities

- Git, Unity Hub, and a C# IDE with Unity integration.
- Unity Editor `6000.6.5f1` (Unity 6.6 Update), pinned in ADR-0001. This project
  deliberately uses the Update channel to unblock development; keep the exact
  patch and package lock pinned and review future upgrades.
- Android Build Support, Android SDK/NDK, and OpenJDK installed through that editor.
- For signed iOS builds: macOS, the supported Xcode/toolchain, Apple developer
  access, and signing/provisioning. Windows can implement gameplay and produce
  Android builds; it cannot locally archive/sign an iOS app through Xcode.
- At least one physical Android device and one physical iPhone for M1 verification.
- Review current Unity license eligibility and CI activation terms. Editor
  installation does not automatically authorize a headless CI seat.

## Current bootstrap status

The repository owner selected Unity `6000.6.5f1` (Unity 6.6 Update) so work
could proceed. The Universal 3D project is now in `game`; its package lock pins
URP `17.6.0`, and a clean batch import completed without compiler errors. The
previous Unity 6.3 LTS candidate `6000.3.26f1` failed in Unity-supplied URP
`17.3.0` editor code because `MaterialReferenceChanger` was missing; it was
not selected. Do not patch Unity package source or downgrade this project.
Consider a future LTS only through a reviewed migration after a clean import,
compile, tests, and platform builds succeed.

The initial graybox slice is now generated at
`game\Assets\_GrindHero\Scenes\FirstSlice.unity`, with data assets, separate
assemblies, a disabled test-scene entry, and EditMode/PlayMode test assemblies.
The pinned Editor has passed the first-slice scene build, **9 EditMode tests**,
and **1 PlayMode scene/pause smoke test**. This verifies Editor-side compilation
and the prototype boot path only; Android/iOS builds, physical-device controls,
and licensed CI have not been validated.

## Detailed bootstrap: Windows and this existing repository

The Unity project belongs in the existing repository's `game` directory. The
Git repository is its parent directory; Unity Hub should open `game`, not the
repository root. Do not run `git init`, create a second GitHub repository, or
connect this project to Unity Version Control. Git already tracks the parent
repository, and the existing GitHub remote is used to share its branch.

### 1. Install Unity Hub

1. Download Unity Hub from the [official Unity download page](https://unity.com/download).
   Unity's [Windows installation guide](https://docs.unity.com/en-us/hub/install-hub-win-mac)
   has the current installer steps.
2. Run the Windows installer, open Unity Hub, and sign in or create a Unity
   account. Activate a Unity license appropriate for your use.
3. Do not download an Editor installer from a third-party site. Use Hub to
   install and manage the Editor and platform modules.

### 2. Install the pinned Editor and Android modules

1. In Hub, open **Installs** and choose **Install Editor**.
2. Install the project-pinned **Unity 6.6 Update Editor `6000.6.5f1`** recorded
   in [ADR-0001](adr/0001-unity-mobile-foundation.md). For future upgrades, use Unity's
   [Unity 6 support page](https://unity.com/releases/unity-6/support) and
   [Editor archive](https://unity.com/releases/editor/archive), and review the
   change before replacing the pin. Do not use a floating "latest" label.
3. Install **Android Build Support**, including **Android SDK & NDK Tools** and
   **OpenJDK**. If the Editor is already installed, use its **Add modules**
   action to add these components.
4. Record the complete Editor version shown by Hub. Unity also records it after
   project creation in `game\ProjectSettings\ProjectVersion.txt`.

Windows is sufficient for Unity development and Android builds. An iOS build
that can be archived and signed requires macOS and Xcode; plan to use a Mac or
approved Mac build host later.

### 3. Project location and template

The initial Universal 3D project has been created in `game`. The following
steps document the original bootstrap path for a fresh clone; do not create a
second project inside the existing project directory.

1. In Hub, choose **Projects > New project**, select the installed Editor
   version, and choose the **Universal 3D** template. This configures URP for a
   3D project; see Unity's
   [URP project creation guide](https://docs.unity3d.com/Manual/urp/creating-a-new-project-with-urp.html).
2. Name the project `GrindHero`. Set its location so the resulting Unity
   project root is exactly `<repository-root>\game`. Before creating it, check
   the full path shown by Hub: it must not produce
   `<repository-root>\game\GrindHero` or put `Assets` directly in the repository
   root.
3. The current `game` folder contains a `README.md`, so Hub may refuse to create
   a project there. If it does, create a temporary project in a sibling folder
   such as `<repository-root>\_unity-bootstrap-temp`. Close Unity Editor after
   the template is generated. In File Explorer, copy only that temporary
   project's `Assets`, `Packages`, and `ProjectSettings` folders into the
   existing `game` folder, alongside `game\README.md`. Do not copy `Library`,
   `Temp`, `Obj`, `Logs`, `UserSettings`, or build output.
4. In Hub, use **Add project from disk** (or the equivalent **Add** action) and
   select the `game` folder. If you used the temporary-folder fallback, open
   `game`, not the temporary project. Wait for the first import to finish and
   resolve any package or compilation errors before continuing.
5. In Unity Editor, open **Edit > Project Settings > Editor**. Set **Version
   Control Mode** to **Visible Meta Files** and **Asset Serialization Mode** to
   **Force Text**. These settings make Unity asset references and serialized
   settings suitable for Git review.
6. Confirm that `game` contains `Assets`, `Packages\manifest.json`,
   `Packages\packages-lock.json`, and `ProjectSettings\ProjectVersion.txt`.
   Keep `game\README.md`; it explains the purpose of the project directory.

### 4. Verify that Unity is using this repository

Unity does not need a separate GitHub sign-in or a special "connect to Git"
setting. Its project files live under `game`; the Git root is the parent
repository. Open a terminal at the repository root (the directory containing
`.git`, `docs`, and `game`) and run:

```powershell
git status --short --branch
```

Unity-generated source assets and settings should appear under `game`. The
repository's `.gitignore` excludes Unity caches and local build output. Keep
source assets and their `.meta` files, the package manifest and lockfile, and
shared project settings; do not add `Library`, `Temp`, `Obj`, `Logs`,
`UserSettings`, IDE-generated files, local builds, or signing credentials.

If Unity is opened from a separate clone or another checkout, its changes will
not appear in this worktree automatically. Use the current worktree's `game`
folder if available; otherwise bring your changes onto the same Git branch
before asking for repository scaffolding. Do not copy the temporary project
into the repository root.

## Bootstrap checklist (M1)

1. Open the existing `game` project with Editor `6000.6.5f1` and verify a clean
   import. The exact Editor patch and package lock are recorded in ADR-0001 and
   `ProjectSettings/ProjectVersion.txt`.
2. Keep only necessary template packages. Add/pin Unity Input System and Unity
   Test Framework if not present. URP remains the rendering baseline. Review
   package compatibility through Unity Package Manager and commit its lockfile.
3. **Completed locally:** assembly definitions, the generated first-slice
   graybox, data assets, and EditMode/PlayMode test assemblies exist. The
   PlayMode setup temporarily enables `FirstSliceTestScene.unity` for the test
   run and restores it to disabled afterward, so it is not included in normal
   player builds. Rebuild generated content from **GrindHero > Build First
   Slice** after changing the scene builder.
4. Confirm the owner-controlled Android application ID and iOS bundle ID before
   release builds; document them without committing credentials. Set
   IL2CPP/ARM64 for release targets, confirm platform architecture/API support,
   and disable development flags for release configurations.
5. Landscape orientation, virtual-joystick movement, and automatic targeting
   are the current prototype choices. Confirm the minimum OS/device floor and
   control feel on real devices; record actual devices rather than inventing
   support claims.
6. Create local Android and iOS development builds, install them, and verify
   touch input, suspend/resume, safe areas, and startup. Windows can produce
   Android builds when its modules are installed; signed iOS builds require
   macOS and Xcode. Record build evidence.
7. **Passed locally:** `.\scripts\Test-UnityProjectReady.ps1` verifies required
   project files, pinned package versions, Unity metadata, assembly boundaries,
   test-assembly presence, and the enabled build scene. It does not compile the
   project, run tests, validate licensing, or build a player. Configure licensed
   Unity CI, run the Unity test workflow, and record the first successful
   platform build and test results.
8. Enable automated engine checks using the trust model in
   [delivery](delivery.md), protect `main`, and update README status.

Use Unity Hub to open `game`, never the repository root. Teammates install the
version from `ProjectSettings/ProjectVersion.txt`, not a floating "latest".
Engine/package upgrades happen in dedicated PRs with platform builds and
save/content regression evidence.

## Local validation after bootstrap

Run tests through **Window > General > Test Runner**, or invoke the pinned editor.
Set `$Unity` to your editor executable's full path; do not assume a machine-wide
Unity command exists:

```powershell
$Unity = 'C:\Program Files\Unity\Hub\Editor\<pinned-version>\Editor\Unity.exe'
New-Item -ItemType Directory -Force .\.artifacts | Out-Null
& $Unity -batchmode -nographics -projectPath "$PWD\game" -runTests -testPlatform EditMode -testResults "$PWD\.artifacts\editmode.xml" -logFile "$PWD\.artifacts\editmode.log"
if ($LASTEXITCODE -ne 0) { throw 'EditMode tests failed; inspect the log and XML.' }
& $Unity -batchmode -nographics -projectPath "$PWD\game" -runTests -testPlatform PlayMode -testResults "$PWD\.artifacts\playmode.xml" -logFile "$PWD\.artifacts\playmode.log"
if ($LASTEXITCODE -ne 0) { throw 'PlayMode tests failed; inspect the log and XML.' }
```

Close the interactive editor before batch testing the same project. Do not add
`-quit` to these test commands: let the test runner manage process exit. Inspect
results for a nonzero test count; a successful process with no discovered tests
does not establish coverage. Rendering-dependent tests require a graphics-capable
runner rather than treating headless tests as visual evidence.

## Sources

- [Download Unity Hub](https://unity.com/download)
- [Install Unity Hub on Windows](https://docs.unity.com/en-us/hub/install-hub-win-mac)
- [Unity releases and support](https://unity.com/releases/unity-6/support)
- [Unity Editor archive](https://unity.com/releases/editor/archive)
- [Creating a URP project](https://docs.unity3d.com/Manual/urp/creating-a-new-project-with-urp.html)
- [iOS setup](https://docs.unity3d.com/Manual/iphone-GettingStarted.html)
- [GameCI testing](https://game.ci/docs/github/test-runner/)
- [GameCI licensing](https://game.ci/docs/github/activation/)

Reviewed for this foundation on 2026-10-10. Recheck current store/toolchain
requirements before each release.

## Unity concepts for a software developer

| Unity concept | Practical mental model |
| --- | --- |
| Scene | Serialized object graph/environment; runtime load/unload owns lifetimes |
| GameObject/component | Composition rather than a deep entity inheritance tree |
| MonoBehaviour | Engine-managed lifecycle adapter, not a service/domain base class |
| `Awake` / `OnEnable` / `Start` | Initialization/activation hooks; avoid cross-object ordering assumptions |
| `Update` / `FixedUpdate` | Per-frame work / fixed-step physics; neither is a business-rule architecture |
| Prefab | Reusable serialized object composition; variants share a controlled base |
| ScriptableObject | Shared authored definition asset; not mutable per-player progress |
| `.meta` GUID | Stable asset identity; losing it can break references across the project |
| Assembly definition | Explicit compilation/dependency boundary |
| EditMode / PlayMode / player | Rules/editor tests / engine lifecycle tests / actual device behavior |

Start by implementing one explicit state transition and its test, then bind it
to a scene component. Keep startup wiring explicit, and profile a device player
early. Successful C# compilation does not prove scene serialization, Unity
object lifetimes, rendering, touch input, or IL2CPP behavior.
