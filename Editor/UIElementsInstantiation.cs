using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.UIElements
{
    public class InstantiateUIElements
    {
        [MenuItem("GameObject/UI/Draggable Window", false, 10)]
        private static void InstantiateDraggableWindow()
        {
            InstantiatePrefabWithGUID("802f77e30de68254ca2e9c6fe453d03a");
        }
        
        [MenuItem("GameObject/UI/Slide Selector", false, 10)]
        private static void InstantiateSlideSelector()
        {
            InstantiatePrefabWithGUID("3ce48dc3862df4c4b8efec1b4d302805");
        }
        
        [MenuItem("GameObject/UI/Simple Timer", false, 10)]
        private static void InstantiateSimpleTimer()
        {
            InstantiatePrefabWithGUID("3791d9554aeb5de4a9bf1ab6ae6e35f5");
        }

        [MenuItem("GameObject/UI/Hold Action", false, 10)]
        private static void InstantiateHoldAction()
        {
            InstantiatePrefabWithGUID("cd0d81277799fbd4687d8eb9a4ec80c8");
        }

        [MenuItem("GameObject/Debugging/Live Logger", false, 10)]
        private static void InstantiateLiveLogger()
        {
            InstantiatePrefabWithGUID("eafa3d82aa67114488f702b184b89bb9");
        }

        private static GameObject InstantiatePrefabWithGUID(string prefabGUID)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);

            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError($"Prefab with GUID \"{prefabGUID}\" not found!");
                return null;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Failed to load prefab of GUID \"{prefabGUID}\" at path \"{prefabPath}\".");
                return null;
            }

            // Instantiate as a Prefab instance first (required for unpacking)
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (instance == null)
            {
                Debug.LogError("Failed to instantiate prefab.");
                return null;
            }
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            instance.transform.SetParent(Selection.activeTransform, false);
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Unpacked Timer Prefab");
            Selection.activeGameObject = instance;

            return instance;
        }
    }
}
