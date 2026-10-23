using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.Management;

namespace HoloGraffiti.Editor
{
    public static class QuestProjectSettings
    {
        const string ProductName = "HoloGraffiti";
        const string CompanyName = "HoloGraffiti";
        const string AndroidPackageId = "com.holograffiti.app";
        const string OpenXRLoaderType = "UnityEngine.XR.OpenXR.OpenXRLoader";
        const string MetaXRFeatureSetId = "com.meta.openxr.featureset.metaxr";
        const string XRSettingsPath = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";

        [MenuItem("HoloGraffiti/Apply Quest Project Settings")]
        public static void Apply()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android,
                    BuildTarget.Android
                );
            }

            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidPackageId);

            var config = OVRProjectConfig.CachedProjectConfig;
            config.targetDeviceTypes = new List<OVRProjectConfig.DeviceType>
            {
                OVRProjectConfig.DeviceType.Quest3S,
            };
            config.handTrackingSupport = OVRProjectConfig.HandTrackingSupport.ControllersAndHands;
            config.insightPassthroughSupport = OVRProjectConfig.FeatureSupport.Supported;
            OVRProjectConfig.CommitProjectConfig(config);

            var manager = GetOrCreateManagerSettings(BuildTargetGroup.Android);
            XRPackageMetadataStore.AssignLoader(
                manager,
                OpenXRLoaderType,
                BuildTargetGroup.Android
            );

            var metaFeatureSet =
                OpenXRFeatureSetManager.GetFeatureSetWithId(
                    BuildTargetGroup.Android,
                    MetaXRFeatureSetId
                ) ?? throw new InvalidOperationException("Meta XR OpenXR feature set not found.");
            metaFeatureSet.isEnabled = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Android);

            AssetDatabase.SaveAssets();
        }

        static XRManagerSettings GetOrCreateManagerSettings(BuildTargetGroup group)
        {
            if (
                !EditorBuildSettings.TryGetConfigObject(
                    XRGeneralSettings.settingsKey,
                    out XRGeneralSettingsPerBuildTarget perTarget
                )
            )
            {
                perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(perTarget, XRSettingsPath);
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey, perTarget, true);
            }

            if (!perTarget.HasManagerSettingsForBuildTarget(group))
            {
                perTarget.CreateDefaultManagerSettingsForBuildTarget(group);
            }

            return perTarget.ManagerSettingsForBuildTarget(group);
        }
    }
}
