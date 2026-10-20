# HoloGraffiti

A spatial drawing app (pinch and draw 3D tubes in mid-air, export them as game assets), being ported from HoloLens 2 to the Meta Quest 3S. It's a year-long UConn senior design project for a team of five.

- `docs/ROADMAP.md`: the plan, decisions, owners and milestones. It's the source of truth, so read it before planning work.
- `docs/SETUP.md`: machine setup and the checks to run before pushing.
- `docs/BASICS.md`: glossary of the XR, Unity and graphics terms used here.

## Who does what

- Ibrahim (Lead, GitHub `ibbyml`) owns the whole Quest port. As the instructor directed, Claude does most of the port work: C# scripts, editor scripts, packages and CI. Ibrahim does the Unity Editor clicks and headset testing, and reviews every pull request.
- The other four teammates work on features (geometry, interactions, exports) in their own sandbox Unity projects, and bring plain C# scripts in after the port merges.
- The team is new to XR. Explain XR, Unity and 3D-graphics terms in plain language the first time they come up, or point to `docs/BASICS.md`.

## Layout

- The repo root is the new Unity `6000.6.5f1` URP project. Our code and assets live under `Assets/HoloGraffiti/`:
  - `Scripts/Runtime`: the `HoloGraffiti.Runtime` assembly, namespace `HoloGraffiti`
  - `Scripts/Editor`: `HoloGraffiti.Editor` (editor-only tools)
  - `Scripts/Tests`: `HoloGraffiti.Tests` (EditMode tests)

  When our code uses a new package, also add that package's assembly to the right `.asmdef`'s `references`.
- `legacy/` is the old HoloLens project (Unity 2022.3, MRTK3), tagged `legacy` at `6b02e1d`. It's for reference only: Unity doesn't import it, and the LFS and merge rules are switched off there. Don't edit it. The scripts being ported are in `legacy/Assets/Scripts/`.

## Rules

- **Unity is pinned to exactly `6000.6.5f1`.** Never let Unity upgrade the project. The whole team switches versions together; Unity 6.7 LTS is planned for winter break.
- **Quest 3S only.** Don't add HoloLens, MRTK, UWP or Windows Mixed Reality code or packages.
- **XR stack:** Meta XR Core SDK and Interaction SDK on the Unity OpenXR Plugin. Don't use the deprecated Oculus XR Plugin.
- **URP only.**
- **Exports:** the headset writes OBJ and GLB. Unity can't write FBX on Android, so FBX comes from a headless Blender script on a desktop.
- **One writer at a time for `ProjectSettings/`, `Packages/` and scenes.** Unity can't merge them reliably. Until the port merges, only the `quest-migration` branch touches them.
- **`.meta` files:** every file and folder under `Assets/` needs its `.meta` committed, and CI fails without it. If you create an asset outside Unity, open the project so Unity generates the `.meta`, or write one with a new random 32-hex-digit GUID. Never reuse a GUID.
- **Unity YAML:** assets are saved as text, so scenes, prefabs and settings show up in diffs as YAML. Prefer changing them in the Editor or with an editor script over hand-editing the YAML.
- **`docs/ROADMAP.md`:** Ibrahim owns its wording. Only edit it when asked for that specific change; otherwise suggest wording in chat.
- **Pull requests:** `main` has no branch protection (GitHub's free plan doesn't offer it on private repos), so the rule is enforced by review. Branch from `main`, keep pull requests small, and merge only after Ibrahim reviews.

## Performance

The Quest 3S has a phone-class GPU, and frame rate was the original app's main problem. Treat performance as a requirement:
- Don't allocate every frame (`ToArray()`, LINQ, new lists).
- Share materials so strokes can be batched.
- Assign materials in the Inspector. Never call `Shader.Find` at runtime.
- Don't set `Application.targetFrameRate` or vSync. The headset controls frame timing.

## Commands

Before pushing (CI runs the same checks):

```bash
bash .github/scripts/check-meta-files.sh
dotnet tool restore
dotnet csharpier check Assets/HoloGraffiti
```

Fix formatting with `dotnet csharpier format Assets/HoloGraffiti`.

To run Unity from the command line, close the Editor first, because Unity can't open the same project twice. `$UNITY` is `/Applications/Unity/Hub/Editor/6000.6.5f1/Unity.app/Contents/MacOS/Unity` on macOS and `C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.exe` on Windows.

```bash
"$UNITY" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode-results.xml -logFile -
```

```bash
"$UNITY" -batchmode -quit -projectPath . -executeMethod HoloGraffiti.Editor.SomeClass.SomeMethod -logFile -
```

CI (`.github/workflows/CI.yml`) runs the `.meta` check and the formatting check on every push and pull request. The EditMode tests and the Android APK build run only when the repository variable `UNITY_CI` is `true`, which also needs the Unity license secrets. The APK build is skipped on pull requests.

## Port notes

- Install the Meta XR SDKs from Meta's package registry, `https://npm.developer.oculus.com`. Add it as a scoped registry in `Packages/manifest.json` rather than using the Asset Store, so CI and teammates can download the packages without signing in. The packages are `com.meta.xr.sdk.core` and `com.meta.xr.sdk.interaction.ovr`. Version 207.0.1 was the latest on 2026-10-16; Core needs Unity 6000.0 or newer.
- Open question: the Interaction SDK depends on `com.unity.textmeshpro` 3.0.6, which Unity 6 merged into `ugui`. Check how the Package Manager resolves it.
- The port branch `quest-migration` starts from the tag `phase-1-start`.
