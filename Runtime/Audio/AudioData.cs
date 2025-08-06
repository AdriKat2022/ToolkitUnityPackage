using UnityEngine;

namespace AdriKat.Toolkit.Audio
{
    [CreateAssetMenu(menuName = "Audio/Sound Event")]

    public class AudioData : ScriptableObject
    {
        public string id;
        public AudioClip clip;
        public float volumeMultiplier = 1f;
    }
}