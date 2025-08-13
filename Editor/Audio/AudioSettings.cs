using AdriKat.Toolkit.Attributes;
using AdriKat.Toolkit.Settings;
using UnityEngine;

namespace AdriKat.Toolkit.Audio
{
    public class AudioSettings : ScriptableObject, IDefaultInitializable
    {
        [Header("Database")]
        [Tooltip("The folder in which the AudioData objects will be created by default.")]
        public string defaultAudioDataCreationFolder = "Assets/Audio/AudioData";
        
        [Header("Audio IDs")]
        [ButtonAction(true, nameof(RegenerateAudioIDsClass))]
        [Tooltip("The path of the generated AudioIDs class by default.\nIf the file already exists, it will be overwritten in place without changing its current path.")]
        public string audioIDClassPath = "Assets/Audio/AudioIDs.cs";
        [Tooltip("If true, will auto re-generate the AudioIDs class each time Unity recompiles.")]
        public bool autoRegenerateAudioIDs = false;
        [Tooltip("If true, will trigger Unity's recompilation after generating the AudioIDs class.")]
        public bool refreshAssetsAfterGeneration = true;

        private void RegenerateAudioIDsClass()
        {
            AudioIDGenerator.GenerateAudioIDClass();
        }
        
        public void SetDefault()
        {
            
        }
    }
    
    // The following line enables the use of the settings provider for the audio settings.
    public class AudioSettingsProvider : SettingsProvider<AudioSettings> {}
}