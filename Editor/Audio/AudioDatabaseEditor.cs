using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Audio
{
    [CustomEditor(typeof(AudioDatabase), true)]
    public class AudioDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Refresh AudioID constants"))
            {
                AudioIDGenerator.GenerateAudioIDClass();
            }
            
            if (GUILayout.Button("Open Audio Settings"))
            {
                AudioSettingsProvider.OpenSettingsForUser();
            }
        }
    }
}