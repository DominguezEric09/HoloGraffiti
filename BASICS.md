## Pieces

```
Our scripts (DrawingManager, TubeMeshGenerator, ObjExporter)
        ↓ use
Meta XR SDK: ready-made hand input, buttons, passthrough      ← replaces MRTK3
        ↓ runs inside
Unity 6: the game engine, drawing graphics with URP
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
| **Hand tracking** | Headset cameras follow your hands' joints in 3D, with no controllers. HoloGraffiti relies on it. |
| **Pinch** | Thumb and index finger touching. It's the "mouse click" of hand tracking. Quest also reports **pinch strength**, from 0 (apart) to 1 (touching). |

## Hardware

| Term | What it means |
|---|---|
| **HoloLens 2** | Microsoft's MR headset with see-through lenses, running Windows. Discontinued in 2024. The original app was built for it. |
| **Meta Quest 3S** | Meta's standalone headset (no PC needed). Internally it runs a version of Android, so Quest apps are Android apps. It has a phone-class chip, which is why performance matters so much. |
| **Refresh rate (72/90/120Hz)** | How many images per second the headset displays. In VR, missing that rate causes stutter and can make people nauseous, so "it runs at 72Hz" is a hard requirement, not a nice-to-have. |

## Unity

| Term | What it means |
|---|---|
| **Unity** | The game engine the app is built in. You arrange objects in a visual editor and write behavior in C#. |
| **Unity 2022 LTS / Unity 6** | Engine versions. LTS means "long-term support." Unity 6 is the newer generation, with version numbers like `6000.0.x`. Meta's current SDK requires it. |
| **Scene** | A level file (`.unity`) holding everything in one space. Ours is `DrawingScene`. |
| **GameObject / component** | Everything in a scene is a GameObject. You add behavior by attaching components; our C# scripts are components (`MonoBehaviour`). |
| **Prefab** | A saved, reusable object template. Teams use them so people aren't all editing the same scene file at once. |
| **Package / `manifest.json`** | Unity's add-on system, similar to npm or pip. `Packages/manifest.json` lists the add-ons the project uses. |
| **`.meta` files** | Unity creates one next to every asset to give it a permanent ID. They must be committed, or links between assets break. |
| **Build target** | The platform you compile for. Ours is currently UWP (HoloLens) and needs to become Android (Quest). |
| **UWP / "Metro"** | Universal Windows Platform, the Windows app format HoloLens uses. "Metro" is an old internal name you'll still see in settings files. |
| **APK** | The Android app file. Building the project produces an APK that gets installed on the Quest. |
| **Build And Run** | A Unity button that builds the APK and installs it on a headset plugged in over USB. |

## Graphics

| Term | What it means |
|---|---|
| **Mesh** | A 3D shape made of **vertices** (points) connected into **triangles**. Our strokes become tube meshes. |
| **Normals** | Arrows saying which way each surface faces. Lighting uses them; get them wrong and surfaces look black or inside-out. |
| **Vertex colors** | A color stored on each vertex of a mesh. It's the cheap way to give every stroke its own color. |
| **Shader** | A small program on the graphics chip that decides how a surface looks: color, shine, lighting. Fancy shaders are slow on a mobile chip. |
| **Material** | A shader plus its settings (for example "Standard shader, white"). Objects use materials. |
| **Draw call / batching** | Each "please draw this" request from CPU to GPU is a draw call, and too many is slow. Objects sharing one material can be drawn together (batched). The current code gives every stroke its own material, so nothing batches. That's likely part of the frame-rate trouble he mentioned. |
| **Render pipeline: Built-in vs. URP** | How Unity draws a frame. Built-in is the old default. **URP** (Universal Render Pipeline) is newer and lighter, and it's what Meta builds around. |
| **End caps / closed mesh** | Our tubes are open at the ends like a hose. Closing them makes the mesh "watertight," which 3D printing and some tools need. |
| **Parallel transport** | A math technique for keeping the tube's rings from twisting as the stroke curves. It's the fix for the twisting bug. |

## XR

| Term | What it means |
|---|---|
| **OpenXR** | An industry standard so apps talk to any headset the same way, roughly like USB for headsets. |
| **Unity OpenXR Plugin** | Unity's implementation of OpenXR. It's the layer between Unity and the Quest. |
| **Oculus XR Plugin** | Meta's older connector (Oculus was the old brand name). Deprecated, so we don't use it. |
| **MRTK3** (Mixed Reality Toolkit 3) | An open-source toolkit Microsoft started for HoloLens-style apps: hand input, buttons, menus. The current code uses it to read finger positions and for the hand menu. It's HoloLens-oriented, and this project has pre-release versions, so we're replacing it. |
| **Meta XR SDK** | Meta's set of Unity packages for Quest:<br>• **Core SDK:** camera setup, passthrough, basic hand tracking (you'll see names starting with `OVR`, from Oculus VR)<br>• **Interaction SDK:** ready-made pinch, poke buttons, grabbing and hand menus. It replaces our hand-written pinch detection.<br>• **MR Utility Kit:** understanding the room (walls, tables). Not in our plan. |
| **glTFast** | A Unity package that reads and writes glTF/GLB files. It's already installed, and we'll use it for GLB export. |

## Quest developer tools

| Term | What it means |
|---|---|
| **Developer mode** | A headset setting that allows installing your own apps. Required. |
| **Meta Quest Developer Hub (MQDH)** | A desktop app for managing the headset: install APKs, browse its files, view logs, mirror its screen. |
| **adb** (Android Debug Bridge) | A command-line tool for Android devices. Since the Quest is Android, `adb pull` copies exported files off it. |
| **Quest Link** | Runs the app on your PC and streams it to the headset, so you can test without building an APK every time. **Windows only**, so on your Mac you'll use Build And Run instead. |
| **Meta XR Simulator** | A fake headset on your computer for testing without real hardware. |
| **OVR Metrics Tool** | An overlay inside the headset that shows the real frame rate. It's how we'll measure performance. |
| **Casting** | Mirroring the headset view to a screen or projector. You'll want it for the December demo. |

## 3D file formats

| Format | What it is | Our use |
|---|---|---|
| **OBJ** | Old, simple, universally supported. Shape only; colors need a separate **MTL** file. | What the app exports today |
| **STL** | Triangles only, the standard for 3D printing | From the original internship writeup; not needed now |
| **FBX** | Autodesk's format, the long-time game-industry standard. Holds meshes, materials and animation. It's proprietary and complicated, and Unity can't write it on Android. | Promised in the proposal, so we make it with Blender |
| **glTF / GLB** | A modern open format, nicknamed "the JPEG of 3D": meshes, materials and colors. GLB is the single-file version. | Exported from the headset |
| **Blender** | Free, open-source 3D modeling software that can run scripts with no window open | Converts GLB → FBX on a computer |
| **Unreal Engine** | The other major game engine, made by Epic | The proposal promises exports work there too |

**Axes and handedness:** engines disagree about which direction is "up" and which way the axes point. Unity uses Y-up, "left-handed" coordinates; OBJ uses right-handed ones. Exports that skip the conversion come out **mirrored** or lying on their side. That's one of the bugs in the current OBJ exporter. Units differ too: Unity uses meters, Unreal uses centimeters.

## Unity team workflow

| Term | What it means |
|---|---|
| **Git LFS** | A git add-on that stores big binary files (images, models) outside the normal history so the repo stays fast. |
| **Smart Merge (UnityYAMLMerge)** | Unity's tool for merging scene files. Normal git merging often mangles them. |
