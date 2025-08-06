using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using AdriKat.Toolkit.Attributes;
using AdriKat.Toolkit.CodePatterns;
using AdriKat.Toolkit.Utility;

namespace AdriKat.Toolkit.Audio
{
    [AddComponentMenu("TK/Audio/AudioManager")]
    public class AudioManager : Singleton<AudioManager>
    {
        public AudioDatabase database;
        [SerializeField, ShowIf(nameof(database))]
        protected bool loadSoundClipDataDatabaseOnAwake = true;

        public AudioSource sfxSource;
        public AudioSource musicSource;

        private Dictionary<string, AudioData> audioDictionary;
        
        #region Initialization

        protected override void Awake()
        {
            base.Awake();
            
            if (loadSoundClipDataDatabaseOnAwake)
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
    }
}