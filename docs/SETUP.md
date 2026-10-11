# Setup

Do this once on each machine you'll work on.

## 1. Unity

1. Install [Unity Hub](https://unity.com/download).
2. Install **exactly `6000.6.5f1`**. Paste `unityhub://6000.6.5f1/3ff58d469c8a` into your browser's address bar to open that version in Unity Hub.
3. When Hub asks for modules, add **Android Build Support** with **OpenJDK** and **Android SDK & NDK Tools**.
4. If Hub or Unity offers to upgrade the project to a newer version, say no. The whole team switches versions together.

## 2. Git

1. Install Git LFS (`brew install git-lfs` on macOS; it comes with Git for Windows), then turn it on once:
   ```bash
   git lfs install
   ```
2. Clone the repo:
   ```bash
   git clone https://github.com/ibbyml/HoloGraffiti.git
   ```
3. Set up **Unity Smart Merge**, which merges scenes and prefabs object by object instead of line by line.

   macOS:
   ```bash
   git config --global merge.unityyamlmerge.name "Unity SmartMerge"
   git config --global merge.unityyamlmerge.driver "'/Applications/Unity/Hub/Editor/6000.6.5f1/Unity.app/Contents/Helpers/UnityYAMLMerge' merge -h -p --force --fallback none %O %B %A %A"
   git config --global merge.unityyamlmerge.recursive binary
   ```

   Windows:
   ```powershell
   git config --global merge.unityyamlmerge.name "Unity SmartMerge"
   git config --global merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -h -p --force --fallback none %O %B %A %A"
   git config --global merge.unityyamlmerge.recursive binary
   ```

   The path contains the Unity version, so run the `driver` line again with the new path whenever we upgrade.

**If git still reports a conflict in a scene or prefab,** Smart Merge merged everything it could. Where both branches changed the same value, the file now holds the incoming branch's value, and there are no conflict markers to warn you. Open it in Unity, set those values the way you want, then `git add` the file. The best way to avoid this is for only one person to edit a scene at a time and to keep your work in prefabs.

## 3. Code editor and formatter

1. Use **VS Code** with the **Unity** extension. Select it in Unity under Settings → External Tools → External Script Editor.
2. Install the **.NET SDK**, version 8 or newer (`brew install --cask dotnet-sdk` on macOS, or the installer from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)).
3. In the repo folder, install our code formatter, CSharpier, at the version the repo uses:
   ```bash
   dotnet tool restore
   ```
4. Install the **CSharpier** extension for your editor and turn on format on save.

## 4. Open the project

In Unity Hub, choose **Add → Add project from disk** and pick the repo folder. The first open imports everything and takes a few minutes.

Our code and assets live under `Assets/HoloGraffiti/`. Scripts go in `Scripts/Runtime` (the app), `Scripts/Editor` (editor-only tools) or `Scripts/Tests` (EditMode tests). `legacy/` is the old HoloLens project, kept for reference only.

## 5. Quest

1. Turn on developer mode and connect the headset by USB: [Set up your headset for development](https://developers.meta.com/horizon/documentation/unity/unity-env-device-setup/).
2. Install [Meta Quest Developer Hub](https://developers.meta.com/horizon/documentation/unity/ts-mqdh/).

**Done when:** you can install any sample app on a Quest 3S.

## Before you push

CI runs these checks on every pull request. Run them first to catch problems sooner:

```bash
bash .github/scripts/check-meta-files.sh
dotnet csharpier check Assets/HoloGraffiti
```

The first makes sure every asset has its `.meta` file. If the second fails, fix the formatting with `dotnet csharpier format Assets/HoloGraffiti`.
