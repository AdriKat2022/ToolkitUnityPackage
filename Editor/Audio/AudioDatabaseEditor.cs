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

            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Re-generate AudioIDs Class"))
            {
                AudioIDGenerator.GenerateAudioIDClass();
            }
            
            if (GUILayout.Button("Open Audio Settings"))
            {
                AudioSettingsProvider.OpenSettingsForUser();
            }
            
            GUILayout.EndHorizontal();
        }
    }
}