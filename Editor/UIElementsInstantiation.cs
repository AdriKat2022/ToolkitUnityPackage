using UnityEditor;
using UnityEngine;

namespace AdriKat.Utils.UIElements
{
    public class InstantiateUIElements
    {
        [MenuItem("GameObject/UI/SimpleTimer", false, 10)]
        private static void InstantiateSimpleTimer()
        {
            InstantiatePrefabWithGUID("3791d9554aeb5de4a9bf1ab6ae6e35f5");
        }

        [MenuItem("GameObject/UI/HoldAction", false, 10)]
        private static void InstantiateHoldAction()
        {
            InstantiatePrefabWithGUID("cd0d81277799fbd4687d8eb9a4ec80c8");
        }

        private static GameObject InstantiatePrefabWithGUID(string prefabGUID)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);

            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("Prefab not found! Check the GUID.");
                return null;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("Failed to load prefab from GUID.");
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
