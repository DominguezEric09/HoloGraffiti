using HoloGraffiti.Interactions;
using Oculus.Interaction.Input;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HoloGraffiti.Editor
{
    public static class DrawingSceneSetup
    {
        const string ScenePath = "Assets/HoloGraffiti/Scenes/DrawingScene.unity";
        const string MaterialPath = "Assets/HoloGraffiti/Materials/Stroke.mat";
        const string StrokeShader = "Universal Render Pipeline/Simple Lit";
        const string RootName = "HoloGraffiti";
        const int MaxSourceChainLength = 8;
        const string RightControllerAnchorPath =
            "TrackingSpace/RightHandAnchor/RightControllerAnchor";
        const string LeftControllerAnchorPath = "TrackingSpace/LeftHandAnchor/LeftControllerAnchor";

        [MenuItem("HoloGraffiti/Set Up Drawing Scene")]
        public static void Apply()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Material strokeMaterial = GetOrCreateStrokeMaterial();

            Hand leftHand = FindHand(scene, Handedness.Left);
            Hand rightHand = FindHand(scene, Handedness.Right);
            Transform rig = FindCameraRig(scene);

            GameObject root = GetOrCreateRoot(scene);
            AddHandDrawing(root, "LeftHandDrawing", leftHand, strokeMaterial);
            AddHandDrawing(root, "RightHandDrawing", rightHand, strokeMaterial);
            AddControllerDrawing(
                root,
                "LeftControllerDrawing",
                OVRInput.Controller.LTouch,
                FindAnchor(rig, LeftControllerAnchorPath),
                strokeMaterial
            );
            AddControllerDrawing(
                root,
                "RightControllerDrawing",
                OVRInput.Controller.RTouch,
                FindAnchor(rig, RightControllerAnchorPath),
                strokeMaterial
            );

            GameObject exporters = GetOrCreateChild(root, "Exporters");
            GetOrAddComponent<ObjExporter>(exporters);
            GetOrAddComponent<GlbExporter>(exporters);

            GetOrAddComponent<StrokeClearer>(GetOrCreateChild(root, "StrokeClearer"));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[DrawingSceneSetup] Set up {ScenePath}.");
        }

        static Material GetOrCreateStrokeMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find(StrokeShader);
            if (shader == null)
            {
                throw new System.InvalidOperationException($"Shader not found: {StrokeShader}");
            }

            material = new Material(shader) { enableInstancing = true };
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        // The rig holds several Hand components per side, chained together: each one
        // reads from the one before it. We want the first Hand in the chain, the one
        // closest to the tracking data (FromOVRHandDataSource).
        static Hand FindHand(Scene scene, Handedness handedness)
        {
            Hand closest = null;
            int closestDepth = int.MaxValue;

            foreach (GameObject sceneRoot in scene.GetRootGameObjects())
            {
                foreach (Hand hand in sceneRoot.GetComponentsInChildren<Hand>(true))
                {
                    if (hand.GetType() != typeof(Hand))
                    {
                        continue;
                    }

                    int depth = DepthFromTrackingData(hand, handedness);
                    if (depth > 0 && depth < closestDepth)
                    {
                        closest = hand;
                        closestDepth = depth;
                    }
                }
            }

            if (closest == null)
            {
                throw new System.InvalidOperationException(
                    $"No {handedness} Interaction SDK Hand found in {ScenePath}. Add the OVRComprehensiveInteractionRig first."
                );
            }

            return closest;
        }

        // Counts the steps from a Hand back to the tracking data for the given side.
        // Returns -1 if the chain ends anywhere else.
        static int DepthFromTrackingData(Object modifier, Handedness handedness)
        {
            for (int depth = 1; depth <= MaxSourceChainLength; depth++)
            {
                Object source = new SerializedObject(modifier)
                    .FindProperty("_iModifyDataFromSourceMono")
                    ?.objectReferenceValue;

                if (source is FromOVRHandDataSource)
                {
                    int sourceHandedness = new SerializedObject(source)
                        .FindProperty("_handedness")
                        .intValue;
                    return sourceHandedness == (int)handedness ? depth : -1;
                }

                if (source == null)
                {
                    return -1;
                }

                modifier = source;
            }

            return -1;
        }

        static Transform FindCameraRig(Scene scene)
        {
            foreach (GameObject sceneRoot in scene.GetRootGameObjects())
            {
                OVRCameraRig rig = sceneRoot.GetComponentInChildren<OVRCameraRig>(true);
                if (rig != null)
                {
                    return rig.transform;
                }
            }

            throw new System.InvalidOperationException($"No OVRCameraRig found in {ScenePath}.");
        }

        static Transform FindAnchor(Transform rig, string path)
        {
            Transform anchor = rig.Find(path);
            if (anchor == null)
            {
                throw new System.InvalidOperationException($"OVRCameraRig has no child at {path}.");
            }

            return anchor;
        }

        static GameObject GetOrCreateRoot(Scene scene)
        {
            foreach (GameObject sceneRoot in scene.GetRootGameObjects())
            {
                if (sceneRoot.name == RootName)
                {
                    return sceneRoot;
                }
            }

            var root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        static GameObject GetOrCreateChild(GameObject parent, string name)
        {
            Transform existing = parent.transform.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            return child;
        }

        static T GetOrAddComponent<T>(GameObject gameObject)
            where T : Component
        {
            return gameObject.TryGetComponent(out T component)
                ? component
                : gameObject.AddComponent<T>();
        }

        static void AddHandDrawing(GameObject root, string name, Hand hand, Material strokeMaterial)
        {
            GameObject gameObject = GetOrCreateChild(root, name);

            var pinchDetector = GetOrAddComponent<PinchDetector>(gameObject);
            var serializedDetector = new SerializedObject(pinchDetector);
            serializedDetector.FindProperty("targetHand").objectReferenceValue = hand;
            serializedDetector.ApplyModifiedPropertiesWithoutUndo();

            AddDrawingManager(gameObject, pinchDetector, strokeMaterial);
        }

        static void AddControllerDrawing(
            GameObject root,
            string name,
            OVRInput.Controller controller,
            Transform anchor,
            Material strokeMaterial
        )
        {
            GameObject gameObject = GetOrCreateChild(root, name);

            var provider = GetOrAddComponent<ControllerInputProvider>(gameObject);
            var serializedProvider = new SerializedObject(provider);
            serializedProvider.FindProperty("controller").intValue = (int)controller;
            serializedProvider.FindProperty("controllerAnchor").objectReferenceValue = anchor;
            serializedProvider.ApplyModifiedPropertiesWithoutUndo();

            AddDrawingManager(gameObject, provider, strokeMaterial);
        }

        static void AddDrawingManager(
            GameObject gameObject,
            MonoBehaviour inputProvider,
            Material strokeMaterial
        )
        {
            var drawingManager = GetOrAddComponent<DrawingManager>(gameObject);
            var serializedManager = new SerializedObject(drawingManager);
            serializedManager.FindProperty("inputProviderComponent").objectReferenceValue =
                inputProvider;
            serializedManager.FindProperty("strokeMaterial").objectReferenceValue = strokeMaterial;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
