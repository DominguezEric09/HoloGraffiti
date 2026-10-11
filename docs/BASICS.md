# Basics

Terms you'll run into on this project, grouped by topic.

## Pieces

```
Our C# scripts: drawing, tube meshes, export
        ↓ use
Meta XR SDK: hand input, buttons, passthrough
        ↓ runs inside
Unity: the game engine, drawing graphics with URP
        ↓ talks to the headset through
Unity OpenXR Plugin (OpenXR = the industry-standard "language")
        ↓
Quest 3S: cameras, displays, hand tracking, Android-based OS
```

Almost every term below fits into one layer of that stack.

## VR

| Term | What it means |
|---|---|
| **VR** (virtual reality) | You're fully inside a digital world and can't see your surroundings. |
| **AR** (augmented reality) | Digital objects layered over the real world, like Pokémon Go on a phone. |
| **MR** (mixed reality) | AR where digital objects understand the room: they sit on your real table or hide behind your real couch. Microsoft's preferred term for HoloLens. |
| **XR** (extended reality) | An umbrella term for VR, AR and MR together. Tools use it in their names because they cover all three. |
| **Passthrough** | The Quest's outward cameras film the room and show it on the displays, so you see your surroundings while wearing a VR headset. That's how the Quest does MR. The HoloLens has see-through glass instead. |
| **Hand tracking** | Headset cameras follow your hands' joints in 3D, with no controllers. |
| **Pinch** | Thumb and index finger touching. It's the "mouse click" of hand tracking. Quest also reports **pinch strength**, from 0 (apart) to 1 (touching). |

## Hardware

| Term | What it means |
|---|---|
| **HoloLens 2** | Microsoft's MR headset with see-through lenses, running Windows. Discontinued in 2024. |
| **Meta Quest 3S** | Meta's standalone headset (no PC needed). Internally it runs a version of Android, so Quest apps are Android apps. It has a phone-class chip, so apps get a much smaller performance budget than on a PC. |
| **Refresh rate (72/90/120Hz)** | How many images per second the headset displays. An app has to finish every frame in time. Missing frames causes stutter, and in VR stutter can make people nauseous. |

## Unity

| Term | What it means |
|---|---|
| **Unity** | The game engine the app is built in. You arrange objects in a visual editor and write behavior in C#. |
| **Unity versions** | Unity 6 versions are numbered `6000.x.y`. For example, `6000.6.5f1` is Unity 6.6, patch 5 (`f` means a final release). **LTS** (long-term support) releases get fixes for two years. The releases between them get fixes only until the next one comes out. |
| **Project folders** | `Assets/` holds everything you make. `Packages/` lists the installed packages. `ProjectSettings/` holds project-wide settings, including `ProjectVersion.txt`, the exact Unity version the project uses. `Library/` is Unity's local cache of imported assets. It's never committed, and Unity rebuilds it if you delete it. |
| **Scene** | A level file (`.unity`) holding everything in one space. |
| **GameObject / component** | Everything in a scene is a GameObject. You add behavior by attaching components. A C# script that inherits from `MonoBehaviour` is a component. |
| **Prefab** | A saved, reusable object template. Teams use them so people aren't all editing the same scene file at once. |
| **Package / `manifest.json`** | Unity's add-on system, similar to npm or pip. `Packages/manifest.json` lists the packages a project uses, and `packages-lock.json` records the exact versions installed. |
| **`.meta` files** | Unity creates one next to every asset and folder to give it a permanent ID (a GUID). They must be committed, or links between assets break. |
| **Assembly definition (`.asmdef`)** | A file that compiles the scripts in its folder, and the folders below it, into a separate assembly (a separate compiled unit). It makes compiling faster and lets tests use the code. An assembly can only use code from the assemblies it lists as references, and that includes packages. |
| **Play Mode / domain reload** | Pressing Play runs the scene inside the editor. A **domain reload** restarts all C# code first, which resets static variables. With domain reload turned off (Fast Enter Play Mode), Play starts faster, but static variables keep their values from the previous run unless the code resets them. |
| **Unity Test Framework** | Unity's testing tool, built on NUnit. **EditMode tests** run in the editor without pressing Play, which suits plain logic like mesh generation. **PlayMode tests** run inside a playing scene. |
| **Build target** | The platform you compile for, such as Android (Quest) or UWP (HoloLens). |
| **Build profile** | A saved set of build settings for a platform. Newer Unity 6 versions include one for Meta Quest. |
| **UWP / "Metro"** | Universal Windows Platform, the Windows app format HoloLens uses. "Metro" is an old internal name you'll still see in settings files. |
| **IL2CPP** | A way of building C# that converts it to C++ first, which produces faster code. 64-bit Android builds, including Quest builds, need it. |
| **APK** | The Android app file. Building the project produces an APK that gets installed on the Quest. |
| **Build And Run** | A Unity button that builds the APK and installs it on a headset plugged in over USB. |

## Graphics

| Term | What it means |
|---|---|
| **Mesh** | A 3D shape made of **vertices** (points) connected into **triangles**. |
| **Normals** | Arrows saying which way each surface faces. Lighting uses them; get them wrong and surfaces look black or inside-out. |
| **Vertex colors** | A color stored on each vertex of a mesh. A cheap way to color parts of a mesh differently without separate materials. |
| **Shader** | A small program on the graphics chip that decides how a surface looks: color, shine, lighting. Complex shaders are slow on a mobile chip. |
| **Material** | A shader plus its settings (for example "URP Lit, white"). Objects use materials. |
| **Draw call / batching** | Each "please draw this" request from the CPU to the GPU is a draw call, and too many slow down every frame. Objects that share a material can be drawn together (batched), so giving every object its own material prevents batching. |
| **Render pipeline** | How Unity draws a frame. **Built-in** is the original pipeline. **URP** (Universal Render Pipeline) is lighter and runs on everything from phones and standalone headsets to PCs. **HDRP** targets high-end PCs and consoles. |
| **URP Asset** | The file holding URP's quality settings (named `*_RPAsset`). A project can have several, for example one for PC and one for mobile. |
| **End caps / closed mesh** | End caps close off the ends of a tube mesh. A closed ("watertight") mesh has no holes, which 3D printing and some tools need. |
| **Parallel transport** | A math technique that keeps the rings of a tube mesh from twisting as its path curves. |

## XR

| Term | What it means |
|---|---|
| **OpenXR** | An industry standard so apps talk to any headset the same way, roughly like USB for headsets. |
| **Unity OpenXR Plugin** | Unity's implementation of OpenXR: the layer between Unity and the headset. |
| **Oculus XR Plugin** | Meta's older connector between Unity and Quest (Oculus was Meta's old brand name). Deprecated in favor of OpenXR. |
| **MRTK3** (Mixed Reality Toolkit 3) | An open-source toolkit, started by Microsoft, for HoloLens-style apps: hand input, buttons, menus. |
| **Meta XR SDK** | Meta's set of Unity packages for Quest:<br>• **Core SDK:** camera setup, passthrough, basic hand tracking. You'll see names starting with `OVR`, from Oculus VR. Includes the **Project Setup Tool**, which checks project settings against Quest requirements and fixes them.<br>• **Interaction SDK:** ready-made pinch, poke buttons, grabbing and hand menus.<br>• **MR Utility Kit:** understanding the room (walls, tables). |
| **glTFast** | A Unity package that reads and writes glTF/GLB files, including from inside a running app. |

## Quest developer tools

| Term | What it means |
|---|---|
| **Developer mode** | A headset setting that allows installing apps that aren't from the store. |
| **Meta Quest Developer Hub (MQDH)** | A desktop app for managing the headset: install APKs, browse its files, view logs, mirror its screen. |
| **adb** (Android Debug Bridge) | A command-line tool for Android devices. `adb install` installs an APK, and `adb pull` copies files off the device. |
| **Quest Link** | Runs the app on a PC and streams it to the headset, so you can test without building an APK each time. Windows only. |
| **Meta XR Simulator** | A fake headset on your computer for testing without real hardware. |
| **OVR Metrics Tool** | An overlay inside the headset that shows the real frame rate and other performance stats. |
| **Casting** | Mirroring the headset view to a phone, computer or TV. |

## 3D file formats

| Format | What it is |
|---|---|
| **OBJ** | Old, simple, universally supported. Shape only; colors need a separate **MTL** file. |
| **STL** | Triangles only. The standard for 3D printing. |
| **FBX** | Autodesk's format, the long-time game-industry standard. Holds meshes, materials and animation. It's proprietary and complicated, and Unity can't write it on Android. |
| **glTF / GLB** | A modern open format, nicknamed "the JPEG of 3D": meshes, materials and colors. GLB is the single-file version. |
| **Blender** | Free, open-source 3D modeling software. It can run scripts with no window open. |
| **Unreal Engine** | The other major game engine, made by Epic. |

**Axes and handedness:** tools disagree about which direction is "up" and which way the axes point. Unity is Y-up and left-handed; OBJ and glTF are right-handed, and Blender is Z-up. Exports that skip the conversion come out **mirrored** or lying on their side. Units differ too: Unity uses meters, Unreal uses centimeters.

## Team workflow

| Term | What it means |
|---|---|
| **Git LFS** | A git add-on that stores big binary files (images, models, audio) outside the normal history so the repo stays fast. |
| **`.gitattributes`** | A file that tells git how to treat each file type: its line endings, which merge tool to use, and whether it goes to LFS. |
| **Smart Merge (UnityYAMLMerge)** | Unity's tool for merging scene and prefab files object by object. Plain git merges them line by line, which often breaks them. |
| **Tag** | A permanent name for one commit, used to mark milestones. |
| **CI** (continuous integration) | Automatic checks that run on a server for every push or pull request, such as running the tests and building the app. |
| **GitHub Actions** | GitHub's CI service. Each workflow is a file in `.github/workflows/`. |
| **GameCI** | Open-source GitHub Actions that run Unity in CI to test and build projects. They need a Unity license, stored as a GitHub secret. |
| **Branch protection** | A GitHub setting that blocks merging into a branch, such as `main`, until required CI checks pass. |
| **Formatter (CSharpier)** | A formatter rewrites code into one consistent style, so reviews focus on what the code does. CSharpier is a formatter for C#. |
| **Dotnet tools** | Command-line tools for .NET. `.config/dotnet-tools.json` pins them to exact versions, and `dotnet tool restore` installs them. |
