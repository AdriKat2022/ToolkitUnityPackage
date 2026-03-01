using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AdriKat.Toolkit.Audio
{
    [CustomEditor(typeof(AudioDatabase), true)]
    public class AudioDatabaseEditor : Editor
    {
        private ReorderableList list;
        private SerializedProperty allSoundsProp;

        private string searchFilter = "";
        private string audioClipsFolder = AudioSettingsProvider.GetOrCreateSettings().DefaultAudioClipsFolder;
        private bool enableDatabaseSynchronisation;
        private bool deleteLinkedAudioData;
        
        private void OnEnable()
        {
            allSoundsProp = serializedObject.FindProperty("allSounds");

            list = new ReorderableList(serializedObject, allSoundsProp, true, true, true, true);

            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Audio Database");
            };

            list.drawElementCallback = DrawElement;

            list.elementHeightCallback = index =>
            {
                var element = allSoundsProp.GetArrayElementAtIndex(index);
                if (element.objectReferenceValue == null)
                    return EditorGUIUtility.singleLineHeight + 6;

                return EditorGUIUtility.singleLineHeight * 5 + 12;
            };

            list.onAddCallback = OnAdd;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawToolbar();
            EditorGUILayout.Space(5);
            DrawDragAndDropArea();
            EditorGUILayout.Space(5);
            list.DoLayoutList();
            DrawButtons();
            ValidateDuplicates();
            EditorGUILayout.Space(10);
            DrawSynchroniser();
            
            serializedObject.ApplyModifiedProperties();
        }

        // ----------------------------------------------------
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            searchFilter = GUILayout.TextField(searchFilter, GUI.skin.FindStyle("ToolbarSearchTextField"));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
            {
                searchFilter = "";
                GUI.FocusControl(null);
            }

            if (GUILayout.Button("Create New AudioData", EditorStyles.toolbarButton))
            {
                CreateNewAudioData();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawDragAndDropArea(float height = 50f)
        {
            Rect dropArea = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));

            GUI.Box(dropArea, "Drag & Drop AudioData Here", EditorStyles.helpBox);
            
            EditorDrawUtils.SetDragAndDropCallback<AudioClip>(dropArea, objects =>
            {
                foreach (Object o in objects)
                {
                    var clip = o as AudioClip;
                    if (clip == null) continue;
                    AddAudioDataToList(AudioDataPropertyDrawer.CreateAudioDataAssetFromAudioClip(clip, false));
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return true;
            });
        }

        private void DrawButtons()
        {
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button(new GUIContent("Re-generate AudioIDs Class")))
            {
                AudioIDGenerator.GenerateAudioIDClass();
            }
            
            if (GUILayout.Button("Open Audio Settings"))
            {
                OpenAudioSettings();
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = allSoundsProp.GetArrayElementAtIndex(index);
            rect.y += 2;

            if (element.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(rect, element, GUIContent.none);
                return;
            }

            var audioData = element.objectReferenceValue as AudioData;

            if (!string.IsNullOrEmpty(searchFilter) &&
                !audioData.id.ToLower().Contains(searchFilter.ToLower()))
                return;

            SerializedObject audioSO = new SerializedObject(audioData);

            var idProp = audioSO.FindProperty("id");
            var clipProp = audioSO.FindProperty("clip");
            var volumeProp = audioSO.FindProperty("volumeMultiplier");

            float line = EditorGUIUtility.singleLineHeight;

            Rect r = new Rect(rect.x, rect.y, rect.width, line);
            EditorGUI.PropertyField(r, idProp);

            r.y += line + 2;
            EditorGUI.PropertyField(r, clipProp);

            r.y += line + 2;
            EditorGUI.Slider(r, volumeProp, 0f, 2f);

            r.y += line + 2;
            EditorGUI.BeginDisabledGroup(clipProp.objectReferenceValue == null);
            if (GUI.Button(new Rect(r.x, r.y, 60, line), "Play"))
            {
                PlayClip((AudioClip)clipProp.objectReferenceValue);
            }
            EditorGUI.EndDisabledGroup();
            if (GUI.Button(new Rect(r.x + 70, r.y, 60, line), "Stop"))
            {
                StopAllClips();
            }

            audioSO.ApplyModifiedProperties();
        }

        private void DrawSynchroniser()
        {
            EditorGUILayout.HelpBox("Synchronising your database with the selected folder below will empty the database and recreate an entry for each AudioClip in the specified folder. It will also wipe all AudioData assets in the folder where the new ones will be created.", MessageType.Info, true);

            enableDatabaseSynchronisation = EditorGUILayout.Toggle($"Enable Database Synchronisation", enableDatabaseSynchronisation);

            if (!enableDatabaseSynchronisation) return;
            
            deleteLinkedAudioData = EditorGUILayout.Toggle($"Delete All Linked AudioDatas", deleteLinkedAudioData);
            
            audioClipsFolder = EditorGUILayout.TextField("Audio Clips Folder", audioClipsFolder);

            // Check if audioClipsFolder exists.
            bool isValid = Directory.Exists(audioClipsFolder);

            if (!isValid)
            {
                EditorGUILayout.HelpBox("Specified folder does not exists.", MessageType.Warning, true);
            }
            else
            {
                string audioDataCreationFolder = AudioSettingsProvider.GetOrCreateSettings().AudioDataCreationFolder;
                int audioDataAssetsToCreateCount = AssetDatabase.FindAssets($"t:{nameof(AudioClip)}", new[] { audioClipsFolder }).Length;
                int audioDataAssetsToDeleteCount = AssetDatabase.FindAssets($"t:{nameof(AudioData)}", new[] { audioDataCreationFolder }).Length;
                if (deleteLinkedAudioData)
                {
                    EditorGUILayout.HelpBox($"Will delete all AudioData assets listed in this database.", MessageType.Info, false);
                }
                EditorGUILayout.HelpBox($"Will empty this database.", MessageType.Info, false);
                EditorGUILayout.HelpBox($"Will delete {audioDataAssetsToDeleteCount} AudioData assets in '{audioDataCreationFolder}'.", MessageType.Info, false);
                EditorGUILayout.HelpBox($"Will re-create {audioDataAssetsToCreateCount} AudioData assets in '{audioDataCreationFolder}'.", MessageType.Info, false);
                EditorGUILayout.HelpBox($"Will re-assign all created AudioData assets to this Database.", MessageType.Info, false);
            }
            
            EditorGUI.BeginDisabledGroup(!isValid);
            if (GUILayout.Button("Synchronise Database", new GUIStyle(GUI.skin.button)))
            {
                SynchroniseClipsWithDatabase();
            }
            EditorGUI.EndDisabledGroup();
        }
        
        private void OnAdd(ReorderableList list)
        {
            AddAudioDataToList(null);
        }

        private void AddAudioDataToList(AudioData audioData)
        {
            list.serializedProperty.arraySize++;
            list.index = list.serializedProperty.arraySize - 1;
            list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue = audioData;
        }
        
        private void CreateNewAudioData()
        {
            AudioData asset = CreateInstance<AudioData>();

            string path = EditorUtility.SaveFilePanelInProject(
                "Create AudioData",
                "NewAudioData",
                "asset",
                "Select save location");

            if (string.IsNullOrEmpty(path))
                return;

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            allSoundsProp.arraySize++;
            allSoundsProp
                .GetArrayElementAtIndex(allSoundsProp.arraySize - 1)
                .objectReferenceValue = asset;

            serializedObject.ApplyModifiedProperties();
        }

        private void ValidateDuplicates()
        {
            var ids = allSoundsProp
                .ToEnumerable()
                .Where(p => p.objectReferenceValue != null)
                .Select(p => ((AudioData)p.objectReferenceValue).id)
                .Where(id => !string.IsNullOrEmpty(id));

            var duplicates = ids.GroupBy(i => i)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    "Duplicate Audio IDs detected: " +
                    string.Join(", ", duplicates),
                    MessageType.Error);
            }
        }

        #region Audio Preview
        
        // ----------------------------------------------------
        // Audio Preview (Editor Only)
        // ----------------------------------------------------
        
        private void PlayClip(AudioClip clip)
        {
            if (clip == null) return;

            var audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

            var method = audioUtil.GetMethod(
                "PlayPreviewClip",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                null,
                new[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null);

            method.Invoke(null, new object[] { clip, 0, false });
        }

        private void StopAllClips()
        {
            var audioUtil = typeof(AudioImporter).Assembly
                .GetType("UnityEditor.AudioUtil");

            var method = audioUtil.GetMethod(
                "StopAllPreviewClips",
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public);

            method.Invoke(null, null);
        }

        #endregion*

        private void SynchroniseClipsWithDatabase()
        {
            if (deleteLinkedAudioData)
            {
                // Delete all assets linked to this database.
                AudioData[] allSounds = allSoundsProp.ExtractArray<AudioData>();
                foreach (var audioData in allSounds)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(audioData));
                }

                Debug.Log($"Deleted {allSounds.Length} linked AudioData assets.");
            }
            
            Debug.Log($"Reset database ({allSoundsProp.arraySize} entries).");
            allSoundsProp.arraySize = 0;
            
            // Wipe the folder.
            string audioDataFolder = AudioSettingsProvider.GetOrCreateSettings().AudioDataCreationFolder;
            string[] allAudioDatas = AssetDatabase.FindAssets($"t:{nameof(AudioData)}", new[] { audioDataFolder }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            AssetDatabase.DeleteAssets(allAudioDatas, new List<string>());
            
            Debug.Log($"Deleted {allAudioDatas.Length} AudioData assets in the destination folder.");
            
            // Add back all sounds in the folder.
            IEnumerable<AudioClip> allAudioClips = AssetDatabase.FindAssets($"t:{nameof(AudioClip)}", new[] { audioClipsFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AudioClip>);
            
            foreach (var audioClip in allAudioClips)
            {
                AddAudioDataToList(AudioDataPropertyDrawer.CreateAudioDataAssetFromAudioClip(audioClip, false));
            }
            
            Debug.Log($"Created {allAudioDatas.Length} AudioData assets.");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            enableDatabaseSynchronisation = false;
        }
            
        [MenuItem("Toolkit/Audio/Audio Settings")]
        private static void OpenAudioSettings()
        {
            AudioSettingsProvider.OpenSettingsForUser();
        }
    }
}