using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using AdriKat.Toolkit.CodePatterns;
using AdriKat.Toolkit.Utility;

namespace AdriKat.Toolkit.Audio
{
    [AddComponentMenu("TK/Audio/Audio Manager")]
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Database")]
        public AudioDatabase database;
        [SerializeField]
        protected bool loadAudioDatabaseOnAwake = true;

        [Header("Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        private Dictionary<string, AudioData> audioDictionary;
        
        #region Initialization

        protected override void Awake()
        {
            base.Awake();
            
            if (loadAudioDatabaseOnAwake)
            {
                LoadSoundClipDataDatabase();
            }
        }

        private void LoadSoundClipDataDatabase()
        {
            audioDictionary = new Dictionary<string, AudioData>();
            foreach (var sound in database.allSounds)
            {
                if (sound != null && !string.IsNullOrEmpty(sound.id))
                    audioDictionary[sound.id] = sound;
            }
        }

        #endregion

        #region Control Methods (Play/Stop)

        public void StopMusic(float fadeOutDuration = 0.3f)
        {
            StartCoroutine(FadeOutMusicCoroutine(musicSource, fadeOutDuration, musicSource.volume));
        }

        public void PlayMusic(string id, bool loop = true, float volume = 1f)
        {
            if (!TryGetSoundClipData(id, out AudioData sound)) return;

            musicSource.loop = loop;
            musicSource.clip = sound.clip;
            musicSource.volume = volume * sound.volumeMultiplier;
            musicSource.Play();
        }

        public void PlaySFX(string id, float volume = 1f)
        {
            if (!TryGetSoundClipData(id, out var sound)) return;

            sfxSource.PlayOneShot(sound.clip, volume * sound.volumeMultiplier);
        }

        #endregion

        protected bool TryGetSoundClipData(string id, out AudioData sound)
        {
            if (id.IsNullOrEmpty())
            {
                Debug.LogError($"Null or empty SoundID.");
                sound = null;
                return false;
            }

            if (!audioDictionary.TryGetValue(id!, out sound))
            {
                Debug.LogWarning($"Sound with ID '{id}' not found! Use \"Tools/Audio/Generate Sound ID Constants\" and use those to ensure type-safety.");
                return false;
            }

            return true;
        }

        #region Coroutine Animations

        protected static IEnumerator FadeOutMusicCoroutine(AudioSource source, float fadeOutDuration, float originalVolume, bool stopOnComplete = true)
        {
            float timer = 0;
            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                source.volume = Mathf.Lerp(originalVolume, 0, timer / fadeOutDuration);
                yield return null;
            }

            if (stopOnComplete) source.Stop();
        }

        #endregion
        
        #if UNITY_EDITOR
        
        [UnityEditor.MenuItem("GameObject/Audio/Audio Manager")]
        private static void SpawnAudioManager(UnityEditor.MenuCommand menuCommand)
        {
            // Create a new GameObject
            GameObject go = new GameObject("Audio Manager");
            var audioManager = go.AddComponent<AudioManager>();
            
            UnityEditor.GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            
            GameObject musicSource = new GameObject("Music Source");
            var music = musicSource.AddComponent<AudioSource>();
            GameObject sfxSource = new GameObject("SFX Source");
            var sfx = sfxSource.AddComponent<AudioSource>();
            
            audioManager.musicSource = music;
            audioManager.sfxSource = sfx;
            
            UnityEditor.GameObjectUtility.SetParentAndAlign(musicSource, go);
            UnityEditor.GameObjectUtility.SetParentAndAlign(sfxSource, go);
        
            // Register undo and select the object
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Created Audio Manager");
            UnityEditor.Selection.activeObject = go;
        }
        
        
        
        #endif
    }
}