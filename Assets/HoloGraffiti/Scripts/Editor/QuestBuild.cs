using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HoloGraffiti.Editor
{
    public static class QuestBuild
    {
        const string OutputPath = "build/Android/HoloGraffiti.apk";

        [MenuItem("HoloGraffiti/Build Quest APK")]
        public static void BuildApk()
        {
            var scenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }

            EditorUserBuildSettings.buildAppBundle = false;

            BuildReport report = BuildPipeline.BuildPlayer(
                new BuildPlayerOptions
                {
                    scenes = scenes.ToArray(),
                    locationPathName = OutputPath,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = BuildOptions.None,
                }
            );

            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new System.InvalidOperationException(
                    $"Quest APK build {summary.result} with {summary.totalErrors} error(s)."
                );
            }

            Debug.Log(
                $"[QuestBuild] Built {summary.outputPath} ({summary.totalSize / (1024 * 1024)} MB)."
            );
        }
    }
}
