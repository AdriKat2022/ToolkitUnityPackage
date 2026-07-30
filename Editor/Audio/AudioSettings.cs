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
        public string AudioDataCreationFolder;
        [Tooltip("The folder in which the AudioClips files.")]
        public string DefaultAudioClipsFolder;
        
        [Header("Audio IDs")]
        [Tooltip("If true, will auto re-generate the AudioIDs class each time Unity recompiles and whenever an AudioClip is added to the database.")]
        public bool RegenerateOnRecompile;
        [Tooltip("If true, will trigger Unity's recompilation after generating the AudioIDs class.")]
        [VarName("Recompile After Generating AudioIDs")]
        public bool RecompileAfterGeneratingAudioIDs;
        [Tooltip("The path of the generated AudioIDs class by default.\nIf the file already exists, it will be overwritten in place without changing its current path.")]
        public string AudioIDClassFolder;
        [ButtonAction(true, nameof(RegenerateAudioIDsClass), customNames = "Regenerate IDs", heightSpacing = 5, nicifyVariableNames = false)]
        [Tooltip("The name of the generated AudioIDs class.\nIf changed while the file already exists, it will be renamed the next time the generation is triggered.")]
        public string AudioIDClassName;

        public void SetDefault()
        {
            DefaultAudioDatabase = null;
            AudioDataCreationFolder = "Assets/Audio/AudioData";
            DefaultAudioClipsFolder = "Assets/Audio/AudioClips";
            
            RegenerateOnRecompile = false;
            RecompileAfterGeneratingAudioIDs = true;
            
            AudioIDClassFolder = "Assets/Audio";
            AudioIDClassName = "AudioIDs";
        }
        
        private void RegenerateAudioIDsClass()
        {
            AudioIDGenerator.GenerateAudioIDClass();
        }
    }
    
    // The following line enables the use of the settings provider for the audio settings.
    // Marked as abstract: it will never be instantiated. A static class would have been better, but it is unfortunately impossible for a static class to extend another class.
    public abstract class AudioSettingsProvider : EditorSettingsProvider<AudioSettings> {}
}