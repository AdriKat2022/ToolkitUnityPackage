using AdriKat.Toolkit.Attributes;
using AdriKat.Toolkit.Settings;
using UnityEngine;

namespace AdriKat.Toolkit.Audio
{
    public class AudioSettings : ScriptableObject, IDefaultInitializable
    {
        [Header("Database")]
        [Tooltip("The reference to the audio database.")]
        public AudioDatabase DefaultAudioDatabase;
        [Tooltip("The folder in which the AudioData objects will be created by default.")]
        public string AudioDataCreationFolder = "Assets/Audio/AudioData";
        
        [Header("Audio IDs")]
        [Tooltip("If true, will auto re-generate the AudioIDs class each time Unity recompiles.")]
        public bool AutoRegenerateAudioIDs = false;
        [Tooltip("If true, will trigger Unity's recompilation after generating the AudioIDs class.")]
        public bool RefreshAssetsAfterGeneration = true;
        [ButtonAction(true, nameof(RegenerateAudioIDsClass), heightSpacing = 5)]
        [Tooltip("The path of the generated AudioIDs class by default.\nIf the file already exists, it will be overwritten in place without changing its current path.")]
        public string AudioIDClassFolder = "Assets/Audio";

        [Header("Others")]
        public string DefaultAudioClipsFolder = "Assets/Audio/AudioClips";
        
        private void RegenerateAudioIDsClass()
        {
            AudioIDGenerator.GenerateAudioIDClass();
        }
        
        public void SetDefault()
        {
            
        }
    }
    
    // The following line enables the use of the settings provider for the audio settings.
    public abstract class AudioSettingsProvider : SettingsProvider<AudioSettings> {}
}