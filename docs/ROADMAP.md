# HoloGraffiti Roadmap

HoloGraffiti started as a HoloLens 2 app for drawing 3D tubes in mid-air with a pinch gesture and exporting them as OBJ.

For senior design we're porting it to the Meta Quest 3S and turning it into a spatial tool for making simple game assets and geometries: users build props out of basic shapes, brush strokes and color, then export them to Unity. We'll evaluate it with a user study.

### Fall
We build a thin version that works end to end (draw on the Quest → export → open in Unity) and show it in December.

### Spring
We add functionality such as brushes, color wheels, shapes and performance work in short iterations.

---

## Proposal deliverables and where they're covered

The proposal is a guideline, not a contract. Scope can change as we learn what's feasible.

| # | Deliverable (from the proposal) | Covered by | Owner |
|---|---|---|---|
| 1 | Migrate from HoloLens 2 to Meta Quest using Unity and the Meta XR SDK | Fall, Phase 1 | Lead + Performance |
| 2 | Refine hand-gesture drawing for Quest hand tracking | Fall, Phase 2, refined in spring | Interactions + Geometry |
| 3 | Export FBX and GLB in addition to OBJ, compatible with Unity and Unreal | OBJ and GLB: fall, Phase 2. FBX: spring. Unreal: deferred. | Exports |

---

## Decisions

| Topic | Decision | Why |
|---|---|---|
| Target hardware | Quest 3S only (HoloLens support is dropped.) | HoloLens 2 is discontinued |
| Engine | Unity 6.6 (`6000.6.5f1`) | The current Meta XR SDK (v203, June 2026) no longer supports Unity 2022.3. |
| XR stack | Meta XR Core SDK + Meta XR Interaction SDK, running on the Unity OpenXR Plugin | This is Meta's recommended setup. The older Oculus XR Plugin is deprecated. Meta also recommends the Interaction SDK for hand input. |
| Project | A fresh Unity project at the repo root. The HoloLens project moved to `legacy/` for reference. | This is a rewrite. Starting clean avoids upgrading HoloLens-only settings and packages. |
| MRTK3 | Not used | It's built around HoloLens, the HoloLens project used pre-release versions, and running it next to Meta's input stack can cause issues. |
| Render pipeline | URP | This is what Meta's tooling assumes and recommends. It also keeps the application more performant. |
| Asset scope | Basic shapes + brush strokes + color | Achievable, easy to test in a study, and produces clean low-poly meshes that suit games. |
| Export | OBJ and GLB written on the headset. FBX made on a desktop by a headless Blender script. | Unity's runtime FBX export only works in Windows/macOS/Linux builds, not on Android. |
| Unreal export | Deferred | We don't know yet how hard it is or whether we'll have time. We decide during winter-break planning. |
| CI | GitHub Actions with GameCI. Every pull request runs a `.meta` file check, a formatting check and the EditMode tests. Merges to `main` also build the Android APK. | Broken assets and compile errors are caught before they reach `main`, and every merge produces an APK anyone can install. |
| Code style | CSharpier, checked in CI | Keeps project formatting standardized |

---

## Team and tracks

| Role | Person | Responsibilities |
|---|---|---|
| Lead + Port | **Ibrahim** | Quest port, Management, Comms, Deliverables, Code Review, Architecture |
| Performance + Port | **Nick** | Quest port, Benchmarking, Performance testing, Platform integration |
| Geometry | **Nithin** | Mesh generation, Brushes, Shapes, Vertex colors |
| Interactions | **Nathan** | Hand inputs, Motion, pinch drawing |
| Exports | **Eddy** | OBJ/GLB/FBX export, Pipeline, Unity import checks, Menus |

After the port, Lead and Performance help wherever work is stuck: Lead with features and integration, Performance with optimizations.

---

## How we work

- **Issues, not schedules.** Each role's work is a list of GitHub issues. Pick them up when you can. The milestones below are the only fixed dates.
- **Pull requests.** Branch from `main` and keep pull requests small. A pull request merges once CI passes and Lead has reviewed it.
- **Ownership.** Each role owns its code. When you change someone else's code, they review the change too. Performance changes include before-and-after numbers from the benchmark scene.
- **Rebalancing.** We re-split the work at winter break, based on what's left.

---

## Timeline overview

### Fall: port and export pipeline

| Weeks | Lead | Performance | Geometry | Interactions | Exports | Milestone |
|---|---|---|---|---|---|---|
| Mid October | Team setup | Team setup | Team setup | Team setup | Team setup | Everyone has tools installed |
| Late October – early November | Quest port: SDK, settings, scene | Quest port: CI, rendering settings, scripts | Sandbox: tube fixes, long strokes, tests | Sandbox: pinch prototype, input interface, editor input | Sandbox: OBJ fixes, GLB export, hand menu prototype, tests | M0 (Fri Nov 6): port merged |
| Mid November | Integration, code review | Performance baseline, CI and builds | Tube fixes in the real project | Pinch drawing in the real project | Export on the headset, hand menu, import checks | |
| Late November | _Thanksgiving week, light work_ | | | | | |
| End of November | Code freeze Mon Nov 30, then bug fixes only | Bug fixes | Bug fixes | Bug fixes, rehearse | Bug fixes, record backup video | Fall showcase (date TBD) |

### Winter break and spring

| When | Work |
|---|---|
| Winter break | Upgrade to Unity 6.7 LTS. Plan spring: re-split the work and decide on Unreal export. |
| Spring | Short iterations on brushes, shapes, color, menus, FBX export and optimizations |
| May | User study |

---

## Fall semester: port and export pipeline

### Phase 0: Team setup (Oct 12–16)

- [x] Create a fresh Unity `6000.6.5f1` URP project at the repo root and move the HoloLens project to `legacy/`.
- [x] Set up CI, Git LFS and Smart Merge rules (`.gitattributes`), assembly definitions and the CSharpier formatter.
- [ ] Tag `6b02e1d` (the last commit with the HoloLens project at the root) as `legacy`. Tag the finished setup as `phase-1-start`. Push both tags.
- [ ] Protect `main` on GitHub: require pull requests and passing CI checks.
- [ ] Create GitHub issues for each role's Phase 1 work.
- [ ] Each teammate follows [SETUP.md](SETUP.md): Unity `6000.6.5f1` with Android Build Support, Git LFS, Smart Merge, the formatter, developer mode and Meta Quest Developer Hub.
- [ ] Create the headset sign-up sheet.

**Done when:** everyone has finished [SETUP.md](SETUP.md) and has deployed any sample app to a Quest 3S.

### Phase 1: The port (Oct 19 – Nov 6)

**Owners:** Lead and Performance, on one branch (`quest-migration`) created from `phase-1-start`. Switching to Android and installing the Meta SDK rewrite `ProjectSettings/` and add many files, so **nobody else edits the main Unity project until it's merged.**

The repo root is a fresh Unity 6.6 URP project. The old HoloLens project stays in `legacy/` for reference.

Unity can't merge two people's changes to `ProjectSettings/` or a scene reliably, so the port is split by files. Only Lead edits `ProjectSettings/`, `Packages/` and `DrawingScene`. Performance works in other files.

**Lead**
1. Add the Unity license secrets ([GameCI's guide](https://game.ci/docs/github/activation)) and set the `UNITY_CI` repository variable to `true`, so CI tests and builds the branch.
2. Set the product name, company name and Android package ID. Remove the template packages we don't use (`com.unity.collab-proxy`, `com.unity.visualscripting`).
3. Switch the build target to Android.
4. Install the Meta XR Core SDK and Interaction SDK on the Unity OpenXR Plugin. Follow Meta's [Unity project setup guide](https://developers.meta.com/horizon/documentation/unity/unity-project-setup/) and apply the Project Setup Tool's fixes.
5. Build `DrawingScene` in `Assets/HoloGraffiti/Scenes` around Meta's camera rig with hand tracking and passthrough.

**Performance**
1. Get CI passing on the branch once the Meta SDK is in, including the Android APK build. If the project uses Unity's Meta Quest build profile, set `buildProfile` in the CI build step to its asset path.
2. Set up the URP asset the Quest build uses (`Assets/Settings`) with Meta's recommended rendering settings.
3. Create one lightweight URP stroke material.
4. Copy `DrawingManager`, `TubeMeshGenerator` and `ObjExporter` from `legacy/Assets/Scripts/` into `Assets/HoloGraffiti/Scripts/Runtime`. Leave `FollowCamera.cs` (unused) and `duplicate1.cs` behind. Then:
   - Stub out the drawing input so the project compiles (`DrawingManager` depends on MRTK). Phase 2 replaces the stub.
   - Replace the `Shader.Find` calls with the stroke material, assigned in the Inspector.
   - Remove the frame-rate cap and vSync code ([DrawingManager.cs#L57](../legacy/Assets/Scripts/DrawingManager.cs#L57)). On Quest the headset controls frame timing.

   Geometry's fixed `TubeMeshGenerator` and Exports' fixed `ObjExporter` replace these copies in Phase 2.

**Everyone else works in a sandbox meanwhile.** Each person uses a throwaway Unity `6000.6.5f1` project on their own machine and brings plain C# scripts into the main project after M0.
- **Geometry:**
  - fix tube twisting with parallel transport
  - add end caps
  - build long strokes out of several mesh chunks, so strokes don't need a point limit
  - write EditMode tests for mesh generation
- **Interactions:** go through Meta's Interaction SDK samples and build a pinch-drawing prototype. Write the input interface (pinch state, pinch point, confidence) and an **editor version driven by mouse and keyboard**, so people can work without a headset.
- **Exports:**
  - fix the OBJ exporter bugs (locale-dependent number format, mirrored output, no normals, always overwrites the same file)
  - get **GLB export** working with the glTFast package's runtime export
  - write EditMode tests for export
  - prototype the hand menu with the Interaction SDK's poke button and hand menu samples

**Done when:** passthrough and hand tracking run on a Quest 3S, the APK builds on every team machine and in CI, and the branch is merged to `main`.

### Phase 2: Drawing on Quest + export to Unity (Nov 9 – Dec 4)

**Lead:** bring everyone's sandbox work into the project, review pull requests, and help wherever work is stuck.

**Performance: baseline and builds (measure only, no optimizing yet)**
- [ ] Build a **benchmark scene** that spawns N strokes. Record frame time with the **OVR Metrics Tool** and the **Unity Profiler** connected to the headset.
- [ ] Write down the numbers. They go in the December presentation as "where we are," and later work gets compared against them.
- [ ] Keep CI and the APK build working as everyone's work comes in.

**Geometry: strokes**
- [ ] Bring the tube fixes and their tests into the project (`Scripts/Runtime` and `Scripts/Tests`).
- [ ] Remove the 500-point stroke cap and use the chunked long strokes instead.

**Interactions: pinch drawing**
- [ ] Bring the input interface and editor input into the project, and connect `DrawingManager` to it.
- [ ] Write the Meta version using the Interaction SDK's pinch strength:
  - separate start and release thresholds so strokes don't break when the fingers hover near the cutoff
  - draw from the **pinch point** (between thumb and index finger), not the index tip
  - end the stroke cleanly if hand tracking is lost

**Exports: export pipeline and menu**
- [ ] Bring the fixed OBJ exporter, GLB export and their tests into the project (`Scripts/Runtime` and `Scripts/Tests`). Add the glTFast package and reference it from `HoloGraffiti.Runtime`.
- [ ] Build the hand menu with Interaction SDK poke buttons (**Export**, **Clear**).
- [ ] Export from the headset: one tap writes a timestamped `.glb` and `.obj` to the app's storage.
- [ ] Get files off the headset with `adb pull` or Meta Quest Developer Hub's file browser, and document the steps.
- [ ] Write an **import checklist** and test both formats in Unity 6: right scale (1 unit = 1 meter), upright, not mirrored, no missing faces.

**Code freeze Monday Nov 30.** After that, bug fixes and demo prep only.

### Fall showcase (December deliverable)

**What we show:** the full loop working on real hardware. Draw in the air on a Quest 3S, tap Export, and open the result in Unity looking the same.

**Demo script**
1. Cast the headset view to the projector (Meta Quest casting or Meta Quest Developer Hub).
2. Put on the headset: passthrough on, hands tracked.
3. Pinch and draw a few strokes, for example a simple chair or the team initials.
4. Tap **Export** on the hand menu.
5. Pull the file to a laptop and drag it into a Unity scene. It appears at the same size and orientation.

**Have a backup:** record the whole demo as a video in the week of Nov 30, in case the live demo fails (tracking, Wi-Fi, casting).

**Presentation contents**
- Problem and motivation
- Architecture: what changed from HoloLens to Quest, and why
- Live demo
- Performance baseline numbers
- What's next

**Not in the December build:** brushes, color, shapes, FBX and performance optimization. Those come in spring.

**Done when (M1):** everything in [Targets](#targets-and-acceptance-criteria) passes, the backup video is recorded, and the presentation is ready.

---

## Winter break

**Unity 6.7 LTS upgrade**
1. Pick a 6.7 LTS patch that has a GameCI image (`unityci/editor:ubuntu-<version>-android-3`).
2. Merge or close every open branch first. The upgrade rewrites project files, so any branch left open will conflict.
3. One person opens the project in 6.7 on a branch, lets Unity upgrade it, gets CI passing and merges.
4. Everyone installs that exact version the same day and updates the Smart Merge path from [SETUP.md](SETUP.md).

**Spring planning**
- Re-split the work based on what's left from fall.
- Decide whether Unreal export is in scope.
- Turn the spring features into GitHub issues.

---

## Spring semester (draft)

Planned in detail over winter break. The rough split:

| Role | Spring work |
|---|---|
| Geometry | Brushes, basic shapes, vertex colors |
| Interactions | Hand input for the new tools, such as placing and resizing shapes |
| Exports | FBX through a headless Blender script, color wheel and tool menus |
| Performance | Optimizations, measured against the fall baseline |
| Lead | Integration, code review, and help where work is stuck |

**May:** the user study, with the whole team.

---

## Targets and acceptance criteria

These are proposed numbers for the fall showcase.

| Area | Target | How it's measured |
|---|---|---|
| Frame rate | Baseline measured and reported | Benchmark scene + OVR Metrics Tool |
| Pinch reliability | No unintended breaks during a 30-second continuous stroke | Scripted test sessions |
| Export | OBJ and GLB open in Unity 6 at the right scale, upright, not mirrored, no missing faces | Import checklist |
| Build | APK builds on every team machine and in CI, and installs on a Quest 3S | Checked at each milestone |

---

## References

- [Meta: Unity project setup](https://developers.meta.com/horizon/documentation/unity/unity-project-setup/)
- [Meta: Unity and OpenXR compatibility](https://developers.meta.com/horizon/documentation/unity/unity-and-openxr-compatibility)
- [Meta: Hand tracking interactions in Unity](https://developers.meta.com/horizon/documentation/unity/unity-handtracking-interactions/)
- [Meta XR Core SDK downloads](https://developers.meta.com/horizon/downloads/package/meta-xr-core-sdk/)
- [Unity FBX Exporter: Developer's Guide (runtime platform limits)](https://docs.unity3d.com/Packages/com.unity.formats.fbx@5.1/api/index.html)
- [Blender command-line usage](https://docs.blender.org/manual/en/latest/advanced/command_line/index.html)
- [GameCI: Unity license activation](https://game.ci/docs/github/activation)
