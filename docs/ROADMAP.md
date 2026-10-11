# HoloGraffiti Roadmap

HoloGraffiti started as a HoloLens 2 app for drawing 3D tubes in mid-air with a pinch gesture and exporting them as OBJ.

For senior design we're porting it to the Meta Quest 3S and turning it into a spatial tool for making simple game assets and geometries: users build props out of basic shapes, brush strokes and color, then export them to Unity. We'll evaluate it with a user study.

### Fall
We build a thin version that works end to end (draw on the Quest → export → open in Unity) and show it in December.

### Spring
We add functionality such as brushes, color wheels, shapes and performance work in short iterations, then test it with users.

---

## Proposal deliverables and where they're covered

| # | Deliverable (from the proposal) | Covered by | Track |
|---|---|---|---|
| 1 | Migrate from HoloLens 2 to Meta Quest using Unity and the Meta XR SDK | Fall, Phase 1 | Tech lead |
| 2 | Refine hand-gesture drawing for Quest hand tracking | Fall, Phase 2 | A |
| 3 | Export FBX and GLB in addition to OBJ, compatible with Unity and Unreal | Fall Phase 2 | B |

The rest of the proposal (research, user study, iterating on feedback) will be planned later.

---

## Decisions

| Topic | Decision | Why |
|---|---|---|
| Target hardware | Quest 3S only (HoloLens support is dropped.) | HoloLens 2 is discontinued |
| Engine | Unity 6.6 (`6000.6.5f1`) for fall, then Unity 6.7 LTS over winter break | The current Meta XR SDK (v203, June 2026) no longer supports Unity 2022.3. |
| XR stack | Meta XR Core SDK + Meta XR Interaction SDK, running on the Unity OpenXR Plugin | This is Meta's recommended setup. The older Oculus XR Plugin is deprecated. Meta also recommends the Interaction SDK for hand input. |
| MRTK3 | We want remove this it entirely | It's built around HoloLens, the versions here are pre release, and running it next to Meta's input stack can cause issues. |
| Render pipeline | URP | This is what Meta's tooling assumes and recommends. It also keeps the application more performant. |
| Asset scope | Basic shapes + brush strokes + color | Achievable, easy to test in a study, and produces clean low-poly meshes that suit games. |
| Export | OBJ and GLB written on the headset. FBX made on a desktop by a headless Blender script. | Unity's runtime FBX export only works in Windows/macOS/Linux builds, not on Android. |

---

## Team and tracks

| Role | Person | Responsibilities |
|---|---|---|
| Project lead | _TBD_ | Schedule, advisor communication, course deliverables |
| Technical lead | _TBD_ | Architecture, code review, Unity 6 migration, performance (Track B) |
| Track A: Interaction | _TBD_, _TBD_ | Hand input, pinch drawing, menus |
| Track B: Export & Performance | Tech lead + _TBD_ | Mesh generation, OBJ/GLB export, Unity import checks, performance baseline |

---

## Timeline overview

### Fall: Port and export pipeline

| Weeks | Tech lead | Track A: Interaction | Track B: Export | Milestone |
|---|---|---|---|---|
| Early October | Team setup | Team setup | Team setup | Everyone has tools installed |
| Early November | Unity 6 + Meta SDK port on its own branch | Sandbox project: pinch prototype, input interface, editor input | Sandbox project: OBJ fixes, GLB export, tube fixes, tests | M0 (Fri Nov 6): port merged |
| Mid November | Review and integration, performance baseline | Pinch drawing in the real project, hand menu | Export on the headset, Unity import checks | |
| Late November | _Thanksgiving week, light work_ | | | |
| Late November | Code freeze Mon Nov 30, then bug fixes only | Bug fixes, rehearse | Bug fixes, record backup video | Fall showcase (date TBD) |

---

## 5. Fall semester: port and export pipeline

### Phase 0: Team setup (Oct 12–16)

- [ ] Tag the current commit `hololens-v1` and push the tag.
- [ ] Add Git LFS and a `.gitattributes` for binary files (`.png`, `.psd`, `.fbx`, `.glb`, `.wav`, and so on).
- [ ] Each teammate configures Unity Smart Merge (`UnityYAMLMerge`) in their git config.
- [ ] Each teammate installs, via Unity Hub, the **exact** Unity version we use (`6000.6.5f1`), with **Android Build Support** (OpenJDK, SDK & NDK).
- [ ] Enable developer mode on headset and install **Meta Quest Developer Hub** on dev machines.
- [ ] Create the headset sign-up sheet.

**Done when:** everyone has the tools installed and has deployed any sample app to a Quest 3S.

### Phase 1: The port (Oct 19 – Nov 6)

**Owner:** tech lead, on one branch (`quest-migration`). The Unity upgrade rewrites `ProjectSettings/`, scene files and many `.meta` files, so **nobody else edits the main Unity project until it's merged.**

**Tech lead: migration steps**
1. Branch from `hololens-v1`.
2. Clean up the tutorial leftovers: `SampleScene.unity`, `firstunity.unity`, `duplicate1.cs`, the corgi PNG, `FollowCamera.cs` (unused) and `WSATestCertificate.pfx`. Set the product name, company name and Android package ID.
3. Open the project in Unity 6 and let it upgrade.
4. Remove MRTK3 and the Microsoft packages from `Packages/manifest.json` and delete `Packages/MixedReality/` (81 MB).
5. Switch the build target to Android and remove the UWP ("Metro") XR settings.
6. Install the Meta XR Core SDK and Interaction SDK on the Unity OpenXR Plugin. Follow Meta's [Unity project setup guide](https://developers.meta.com/horizon/documentation/unity/unity-project-setup/) and apply the Project Setup Tool's fixes.
7. Switch to URP. Create one lightweight stroke material and assign it in the Inspector, replacing the `Shader.Find` calls.
8. Remove the frame-rate cap and vSync code ([DrawingManager.cs#L57](Assets/Scripts/DrawingManager.cs#L57)). On Quest the headset controls frame timing.
9. Rebuild `DrawingScene` around Meta's camera rig with hand tracking and passthrough.
10. Stub out the drawing input so the project compiles (`DrawingManager` currently depends on MRTK). Phase 2 replaces the stub.

**Everyone else works in a sandbox meanwhile.** Each person uses a throwaway Unity 6 project on their own machine and brings plain C# scripts into the main project after M0.
- **Track A:** go through Meta's Interaction SDK samples and build a pinch-drawing prototype. Write the input interface (pinch state, pinch point, confidence) and an **editor version driven by mouse and keyboard**, so people can work without a headset.
- **Track B:**
  - fix the OBJ exporter bugs (locale-dependent number format, mirrored output, no normals, always overwrites the same file)
  - get **GLB export** working with the glTFast package's runtime export
  - fix tube twisting with parallel transport
  - add end caps
  - write edit-mode tests for mesh generation and export

**Done when:** passthrough and hand tracking run on a Quest 3S, the APK builds from every team machine, and the branch is merged to `main`.

### Phase 2: Drawing on Quest + export to Unity (Nov 9 – Dec 4)

**Track A: Input and UI**
- [ ] Bring the input interface and editor input into the project, and connect `DrawingManager` to it.
- [ ] Write the Meta version using the Interaction SDK's pinch strength:
  - separate start and release thresholds so strokes don't break when the fingers hover near the cutoff
  - draw from the **pinch point** (between thumb and index finger), not the index tip
  - end the stroke cleanly if hand tracking is lost
- [ ] Remove the 500-point stroke cap. Split long strokes into chunks instead.
- [ ] Replace the MRTK hand menu and button with Interaction SDK poke buttons (**Export**, **Clear**).

**Track B: Export pipeline to Unity**
- [ ] Bring the fixed OBJ exporter, GLB export and tube fixes into the project.
- [ ] Export from the headset: one tap writes a timestamped `.glb` and `.obj` to the app's storage.
- [ ] Get files off the headset with `adb pull` or Meta Quest Developer Hub's file browser, and document the steps.
- [ ] Write an **import checklist** and test both formats in Unity 6: right scale (1 unit = 1 meter), upright, not mirrored, no missing faces.

**Tech lead: performance baseline (measure only, no optimizing yet)**
- [ ] Build a **benchmark scene** that spawns N strokes. Record frame time with the **OVR Metrics Tool** and the **Unity Profiler** connected to the headset.
- [ ] Write down the numbers. They go in the December presentation as "where we are," and later work gets compared against them.

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

**Not in the December build:** brushes, color, shapes, FBX, Unreal and performance optimization. Those come later.

**Done when (M1):** everything in [Targets](#targets-and-acceptance-criteria) passes, the backup video is recorded, and the presentation is ready.

---

## Targets and acceptance criteria

These are proposed numbers for the fall showcase.

| Area | Target | How it's measured |
|---|---|---|
| Frame rate | Baseline measured and reported | Benchmark scene + OVR Metrics Tool |
| Pinch reliability | No unintended breaks during a 30-second continuous stroke | Scripted test sessions |
| Export | OBJ and GLB open in Unity 6 at the right scale, upright, not mirrored, no missing faces | Import checklist |
| Build | APK builds and installs from every team machine | Checked at each milestone |

---

## References

- [Meta: Unity project setup](https://developers.meta.com/horizon/documentation/unity/unity-project-setup/)
- [Meta: Unity and OpenXR compatibility](https://developers.meta.com/horizon/documentation/unity/unity-and-openxr-compatibility)
- [Meta: Hand tracking interactions in Unity](https://developers.meta.com/horizon/documentation/unity/unity-handtracking-interactions/)
- [Meta XR Core SDK downloads](https://developers.meta.com/horizon/downloads/package/meta-xr-core-sdk/)
- [Unity FBX Exporter: Developer's Guide (runtime platform limits)](https://docs.unity3d.com/Packages/com.unity.formats.fbx@5.1/api/index.html)
- [Blender command-line usage](https://docs.blender.org/manual/en/latest/advanced/command_line/index.html)
