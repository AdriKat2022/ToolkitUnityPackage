using AdriKat.Toolkit.Settings;
using AdriKat.Toolkit.Utils;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Audio
{
    [CustomPropertyDrawer(typeof(AudioData), true)]

    public class AudioDataPropertyDrawer : PropertyDrawer
    {
        private bool foldout = true;
        private const float BUTTON_WIDTH = 100;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + 4; // For the object reference field

            if (property.objectReferenceValue == null)
                return height;

            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (!foldout)
                return height;

            SerializedObject so = new SerializedObject(property.objectReferenceValue);

            height += EditorGUI.GetPropertyHeight(so.FindProperty(nameof(AudioData.clip))) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(so.FindProperty(nameof(AudioData.volumeMultiplier))) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(so.FindProperty(nameof(AudioData.id))) + EditorGUIUtility.standardVerticalSpacing;

            return height + 4; // Padding
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Draw the object reference field first
            Rect refRect = new Rect(position.x, position.y, position.width - (property.objectReferenceValue == null ? BUTTON_WIDTH : 0), EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(refRect, property, label);


            if (property.objectReferenceValue == null)
            {
                // Handle Drag & Drop of an AudioClip (only if the reference is null).
                HandleAudioClipDragAndDrop(property, refRect);

                // Make a create button.
                if (GUI.Button(new Rect(refRect.x + refRect.width, refRect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight), "Create"))
                {
                    AudioData newAudioData = ScriptableObject.CreateInstance<AudioData>();
                    newAudioData.id = property.name;
                    
                    string creationFolder = AudioSettingsProvider.GetOrCreateSettings().defaultAudioDataCreationFolder;
                    EditorUtils.CreateFoldersRecursively(creationFolder);
                    AssetDatabase.CreateAsset(newAudioData, $"{creationFolder}/{property.name}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    
                    property.objectReferenceValue = newAudioData;
                }

                return;
            }

            // Foldout below the reference field
            Rect foldoutRect = new Rect(position.x + 15, refRect.yMax + 2, position.width - 15, EditorGUIUtility.singleLineHeight);
            foldout = EditorGUI.Foldout(foldoutRect, foldout, "Details", true);

            if (!foldout) return;

            EditorGUI.indentLevel++;

            AudioData option = property.objectReferenceValue as AudioData;
            SerializedObject so = new SerializedObject(option);
            so.Update();

            float y = foldoutRect.yMax + 4;
            float fieldHeight;

            y = DrawProp(so, nameof(AudioData.clip), position, y, out fieldHeight);
            y = DrawProp(so, nameof(AudioData.volumeMultiplier), position, y, out fieldHeight);
            y = DrawProp(so, nameof(AudioData.id), position, y, out fieldHeight);

            so.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }

        private static void HandleAudioClipDragAndDrop(SerializedProperty property, Rect refRect)
        {
            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (refRect.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object dragged in DragAndDrop.objectReferences)
                        {
                            if (dragged is AudioClip clip)
                            {
                                // Create a new SoundEvent
                                AudioData newAudioData = ScriptableObject.CreateInstance<AudioData>();
                                newAudioData.clip = clip;
                                newAudioData.id = clip.name;
                                newAudioData.volumeMultiplier = 1f;

                                string assetPath = $"{AudioSettingsProvider.GetOrCreateSettings().defaultAudioDataCreationFolder}/{clip.name}.asset";
                                AssetDatabase.CreateAsset(newAudioData, assetPath);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();

                                property.objectReferenceValue = newAudioData;
                                GUI.changed = true;
                                break;
                            }
                        }
                    }

                    evt.Use();
                }
            }
        }

        private float DrawProp(SerializedObject so, string propName, Rect position, float y, out float height, bool includeChildren = false)
        {
            SerializedProperty prop = so.FindProperty(propName);
            height = EditorGUI.GetPropertyHeight(prop, includeChildren);
            Rect rect = new Rect(position.x, y, position.width, height);
            EditorGUI.PropertyField(rect, prop, includeChildren);
            return y + height + 2;
        }
    }
}
