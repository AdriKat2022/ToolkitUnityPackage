using UnityEngine;

namespace AdriKat.Toolkit.Audio
{
    [CreateAssetMenu(menuName = "Audio/Audio Database", fileName = "AudioDatabase")]

    public class AudioDatabase : ScriptableObject
    {
        public AudioData[] allSounds;
    }
}