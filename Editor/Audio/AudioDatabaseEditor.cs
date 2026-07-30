using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;

namespace AdriKat.Toolkit.Audio
{
    [CustomEditor(typeof(AudioDatabase), true)]
    public class AudioDatabaseEditor : Editor
    {
        private SerializedProperty allSoundsProp;

        private string _searchFilter = "";
        private string _audioClipsFolder;
        private bool _enableDatabaseSynchronisation;
        private bool _deleteLinkedAudioData;
        
        #region IMGUI
        
        private ReorderableList _reorderableList;
        
        private void OnEnable()
        {
            _audioClipsFolder = AudioSettingsProvider.GetOrCreateSettings().DefaultAudioClipsFolder;
            allSoundsProp = serializedObject.FindProperty("allSounds");

            _reorderableList = new ReorderableList(serializedObject, allSoundsProp, true, true, true, true);

            _reorderableList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Audio Database");
            };

            _reorderableList.drawElementCallback = DrawElement;

            _reorderableList.elementHeightCallback = index =>
            {
                var element = allSoundsProp.GetArrayElementAtIndex(index);
                if (element.objectReferenceValue == null)
                    return EditorGUIUtility.singleLineHeight + 6;

                return EditorGUIUtility.singleLineHeight * 5 + 12;
            };

            _reorderableList.onAddCallback = OnAdd;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawToolbar();
            EditorGUILayout.Space(5);
            DrawDragAndDropArea();
            EditorGUILayout.Space(5);
            _reorderableList.DoLayoutList();
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

            _searchFilter = GUILayout.TextField(_searchFilter, GUI.skin.FindStyle("ToolbarSearchTextField"));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
            {
                _searchFilter = "";
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
            
            if (GUILayout.Button("Set This As Default Database"))
            {
                AudioSettings audioSettings = AudioSettingsProvider.GetOrCreateSettings();
                audioSettings.DefaultAudioDatabase = (AudioDatabase)target;
                EditorUtility.SetDirty(audioSettings);
            }
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

            if (!string.IsNullOrEmpty(_searchFilter) &&
                !audioData.id.ToLower().Contains(_searchFilter.ToLower()))
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

            _enableDatabaseSynchronisation = EditorGUILayout.Toggle($"Enable Database Synchronisation", _enableDatabaseSynchronisation);

            if (!_enableDatabaseSynchronisation) return;
            
            _deleteLinkedAudioData = EditorGUILayout.Toggle($"Delete All Linked AudioDatas", _deleteLinkedAudioData);
            
            _audioClipsFolder = EditorGUILayout.TextField("Audio Clips Folder", _audioClipsFolder);

            // Check if audioClipsFolder exists.
            bool isValid = Directory.Exists(_audioClipsFolder);

            if (!isValid)
            {
                EditorGUILayout.HelpBox("Specified folder does not exists.", MessageType.Warning, true);
            }
            else
            {
                string audioDataCreationFolder = AudioSettingsProvider.GetOrCreateSettings().AudioDataCreationFolder;
                int audioDataAssetsToCreateCount = AssetDatabase.FindAssets($"t:{nameof(AudioClip)}", new[] { _audioClipsFolder }).Length;
                int audioDataAssetsToDeleteCount = AssetDatabase.FindAssets($"t:{nameof(AudioData)}", new[] { audioDataCreationFolder }).Length;
                if (_deleteLinkedAudioData)
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
            _reorderableList.serializedProperty.arraySize++;
            _reorderableList.index = _reorderableList.serializedProperty.arraySize - 1;
            _reorderableList.serializedProperty.GetArrayElementAtIndex(_reorderableList.index).objectReferenceValue = audioData;
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

        #endregion
        
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
            if (_deleteLinkedAudioData)
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
            IEnumerable<AudioClip> allAudioClips = AssetDatabase.FindAssets($"t:{nameof(AudioClip)}", new[] { _audioClipsFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AudioClip>);
            
            foreach (var audioClip in allAudioClips)
            {
                AddAudioDataToList(AudioDataPropertyDrawer.CreateAudioDataAssetFromAudioClip(audioClip, false));
            }
            
            Debug.Log($"Created {allAudioDatas.Length} AudioData assets.");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _enableDatabaseSynchronisation = false;
        }
            
        [MenuItem("Toolkit/Audio/Audio Settings")]
        private static void OpenAudioSettings()
        {
            AudioSettingsProvider.OpenSettingsForUser();
        }

        [MenuItem("Toolkit/Audio/Initialise Audio System")]
        private static void InitAudio()
        {
            AudioSettings audioSettings = AudioSettingsProvider.GetSettings();
            bool didSettingsExist = audioSettings != null;
            
            if (!didSettingsExist)
            {
                audioSettings = AudioSettingsProvider.CreateSettings();
            }
            
            int choice = EditorUtility.DisplayDialogComplex("Toolkit Audio Setup", "The Audio Ecosystem Toolkit will be initialized with :\n" +
                                                                      "- 1 AudioDatabase scriptable object (Audio folder);\n" +
                                                                      "- 1 AudioSettings scriptable object (Settings folder);\n" +
                                                                      "\nWould you like to go with defaults or customize the initialization?", "Use Defaults", "Cancel", "Customize");
            if (choice == 1) return;
            
            
            // AUDIO SETTINGS RESET
            bool resetSettings = true;
            if (didSettingsExist)
            {
                // Warn about default initializing: it will overwrite everything.
                resetSettings = EditorUtility.DisplayDialog("Audio Settings Initialization", "AudioSettings already exists. Do you want to overwrite it with default values? This will reset all your settings.", "Overwrite", "Keep Existing");
            }

            if (resetSettings)
            {
                // Default initialize everything.
                audioSettings.SetDefault();
                EditorUtility.SetDirty(audioSettings);
                AssetDatabase.SaveAssets();
                Debug.Log("AudioSettings has been reset to default values.", audioSettings);
            }
            
            
            // AUDIO DATABASE CREATION
            if (audioSettings.DefaultAudioDatabase == null || choice == 2)
            {
                string dbPath = AudioIDGenerator.AUDIO_DATABASE_DEFAULT_PATH;

                bool wouldOverwrite = File.Exists(dbPath);
                bool promptUserForPath = choice == 2 || wouldOverwrite;
                bool createNewDb = true;
                bool skipPrompt = false;

                if (promptUserForPath)
                {
                    if (wouldOverwrite)
                    {
                        skipPrompt = !EditorUtility.DisplayDialog("Audio Database Initialization", "An AudioDatabase asset already exists at the default path. Set the existing database as default and move on?", "Confirm path and overwrite existing", "Set as default and skip");

                        if (skipPrompt)
                        {
                            // Set existing as default.
                            createNewDb = false;
                            var existingAudioDb = AssetDatabase.LoadAssetAtPath<AudioDatabase>(dbPath);
                            if (existingAudioDb != null)
                            {
                                audioSettings.DefaultAudioDatabase = existingAudioDb;
                                EditorUtility.SetDirty(audioSettings);
                                AssetDatabase.SaveAssets();
                                Debug.Log($"Existing AudioDatabase at {dbPath} set as default.", existingAudioDb);
                            }
                            else
                            {
                                Debug.LogError($"Failed to load existing AudioDatabase at {dbPath}. Initialization cancelled.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        bool ok = EditorUtility.DisplayDialog("Audio Database Initialization", "Choose a path for the new AudioDatabase asset.", "Ok", "Cancel");
                        if (!ok)
                        {
                            Debug.Log("Initialization Cancelled.");
                            return;
                        }
                    }
                }
                
                if (promptUserForPath && !skipPrompt)
                {
                    string directory = Path.GetDirectoryName(dbPath)?.Replace("\\", "/");
                    
                    // Customize.
                    dbPath = EditorUtility.SaveFilePanelInProject(
                        "Create AudioDatabase",
                        "AudioDatabase",
                        "asset", "Choose where the AudioDatabase asset will be saved. This asset is where all your audio files will be referenced.", directory);
                }

                if (dbPath == null)
                {
                    Debug.Log("Initialization Cancelled");
                    return;
                }

                if (createNewDb)
                {
                    // Create a new AudioDatabase asset.
                    AudioDatabase newDatabase = CreateInstance<AudioDatabase>();
                    AssetDatabase.CreateAsset(newDatabase, dbPath);
                    audioSettings.DefaultAudioDatabase = newDatabase;
                    EditorUtility.SetDirty(audioSettings);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"Created new AudioDatabase at {dbPath}", newDatabase);
                }
            }
            else
            {
                Debug.Log($"Audio Database is already initialized at {AssetDatabase.GetAssetPath(audioSettings.DefaultAudioDatabase)}.", audioSettings.DefaultAudioDatabase);
            }
            
            bool openSettings = EditorUtility.DisplayDialog("Audio System Initialization", "Audio System has been initialized successfully.", "Open AudioSettings", "Close");

            if (openSettings)
            {
                OpenAudioSettings();
            }
            
            Debug.Log("Initialization finished.");
        }
    }
}