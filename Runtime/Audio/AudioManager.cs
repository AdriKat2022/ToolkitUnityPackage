using System.Collections;
using System.Collections.Generic;
using AdriKat.Toolkit.Attributes;
using AdriKat.Toolkit.DesignPatterns;
using AdriKat.Toolkit.Utility.Extensions;
using UnityEngine;

namespace AdriKat.Toolkit.Audio
{
    [AddComponentMenu("Toolkit/Audio/Audio Manager")]
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Database")]
        public AudioDatabase database;
        [SerializeField]
        protected bool loadAudioDatabaseOnAwake = true;

        [Header("Sources")]
        public AudioSource[] sources;
        
        [ButtonAction(false, 10, nameof(CreateNewChannel), nameof(RefreshChildrenSources))]
        public int defaultMusicChannel;
        public int defaultSFXChannel = 1;
        
        private Dictionary<string, AudioData> _audioDictionary;
        
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
            _audioDictionary = new Dictionary<string, AudioData>();
            
            if (database == null)
            {
                Debug.LogWarning("No sound database assigned to AudioManager!");
                return;
            }
            
            foreach (var sound in database.allSounds)
            {
                if (sound != null && !string.IsNullOrEmpty(sound.id))
                    _audioDictionary[sound.id] = sound;
            }
        }

        #endregion

        #region Control Methods (Play/Stop)

        /// <summary>
        /// Stops the track currently playing in the defaultMusicChannel.
        /// </summary>
        /// <param name="fadeOutDuration">Duration of the fadeout before completely stopping the track.</param>
        public void StopMusic(float fadeOutDuration = 0.3f)
        {
            StopMusic(defaultMusicChannel, fadeOutDuration);
        }

        /// <summary>
        /// Stops the track playing in the provided channel.
        /// </summary>
        /// <param name="channel">ID of the channel to stop.</param>
        /// <param name="fadeOutDuration">Duration of the fadeout before completely stopping the track.</param>
        public void StopMusic(int channel, float fadeOutDuration = 0.3f)
        {
            AudioSource audioSource = GetSource(channel);
            StartCoroutine(FadeOutMusicCoroutine(audioSource, fadeOutDuration, audioSource.volume));
        }

        
        /// <summary>
        /// Plays the provided track in the defaultMusicChannel. 
        /// </summary>
        /// <param name="id">ID of the track to play (Generally stored in the AudioIDs class).</param>
        /// <param name="loop">Set the channel to repeat playing tracks.</param>
        /// <param name="volume">Override volume for the track.</param>
        public void PlayMusic(string id, bool loop = true, float volume = 1f)
        {
            PlayMusic(defaultMusicChannel, id, loop, volume);
        }

        /// <summary>
        /// Plays the provided track in the provided channel.
        /// </summary>
        /// <param name="channel">ID of the channel to use to play the track.</param>
        /// <param name="id">ID of the track to play (Generally stored in the AudioIDs class).</param>
        /// <param name="loop">Set the channel to repeat playing tracks.</param>
        /// <param name="volume">Override volume for the track.</param>
        public void PlayMusic(int channel, string id, bool loop = true, float volume = 1f)
        {
            if (!TryGetSoundClipData(id, out AudioData sound)) return;

            AudioSource musicSource = GetSource(channel);
            
            musicSource.loop = loop;
            musicSource.clip = sound.clip;
            musicSource.volume = volume * sound.volumeMultiplier;
            musicSource.Play();
        }
        
        /// <summary>
        /// Plays the provided track in the provided channel.
        /// </summary>
        /// <param name="audioClip">AudioClip to play.</param>
        /// <param name="loop">Set the channel to repeat playing tracks.</param>
        /// <param name="volume">Override volume for the track.</param>
        /// <param name="channel">ID of the channel to use to play the track.</param>
        public void PlayMusic(AudioClip audioClip, bool loop = true, float volume = 1f, int? channel = null)
        {
            if (audioClip == null) return;

            AudioSource musicSource = GetSource(channel ?? defaultMusicChannel);
            
            musicSource.loop = loop;
            musicSource.clip = audioClip;
            musicSource.volume = volume;
            musicSource.Play();
        }
        
        
        /// <summary>
        /// Plays the provided track in the defaultSFXChannel.
        /// </summary>
        /// <param name="id">ID of the track to play (Generally stored in the AudioIDs class).</param>
        /// <param name="volume">Override volume for the track.</param>
        public void PlaySFX(string id, float volume = 1f)
        {
            PlaySFX(defaultSFXChannel, id, volume);
        }

        /// <summary>
        /// Plays the provided track in the provided channel.
        /// </summary>
        /// <param name="channel">ID of the channel to use to play the track.</param>
        /// <param name="id">ID of the track to play (Generally stored in the AudioIDs class).</param>
        /// <param name="volume">Override volume for the track.</param>
        public void PlaySFX(int channel, string id, float volume = 1f)
        {
            if (!TryGetSoundClipData(id, out var sound)) return;

            AudioSource sfxSource = GetSource(channel);
            
            sfxSource.PlayOneShot(sound.clip, volume * sound.volumeMultiplier);
        }
        
        /// <summary>
        /// Plays the provided track in the provided channel.
        /// </summary>
        /// <param name="audioClip">AudioClip to play.</param>
        /// <param name="loop">Set the channel to repeat playing tracks.</param>
        /// <param name="volume">Override volume for the track.</param>
        /// <param name="channel">ID of the channel to use to play the track.</param>
        public void PlaySFX(AudioClip audioClip, bool loop = true, float volume = 1f, int? channel = null)
        {
            if (audioClip == null) return;

            AudioSource sfxSource = GetSource(channel ?? defaultSFXChannel);
            
            sfxSource.loop = loop;
            sfxSource.clip = audioClip;
            sfxSource.volume = volume;
            sfxSource.Play();
        }
        
        #endregion

        #region Helper Methods

        protected AudioSource GetSource(int channel)
        {
            return sources[channel];
        }
        
        protected AudioSource GetSourceOrDefault(int channel, AudioSource defaultSource)
        {
            return channel >= 0 && channel < sources.Length ? sources[channel] : defaultSource;
        }
        
        protected bool TryGetSoundClipData(string id, out AudioData sound)
        {
            if (id.IsNullOrEmpty())
            {
                Debug.LogError($"Null or empty SoundID.");
                sound = null;
                return false;
            }

            if (!_audioDictionary.TryGetValue(id!, out sound))
            {
                Debug.LogWarning($"Sound with ID '{id}' not found! Use \"Tools/Audio/Generate Sound ID Constants\" and use those to ensure type-safety.");
                return false;
            }

            return true;
        }

        #endregion
        
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
        
        private void CreateNewChannel()
        {
            #if UNITY_EDITOR
            
            // Create a new GameObject
            GameObject go = new GameObject("New Channel");
            var newAudioSource = go.AddComponent<AudioSource>();
            
            // Extends the current array by appending the new source to it.
            AudioSource[] newArray = new AudioSource[sources.Length + 1];
            for (int i = 0; i < newArray.Length; i++)
            {
                newArray[i] = i < sources.Length ? sources[i] : newAudioSource;
            }
            
            sources = newArray;
            
            UnityEditor.GameObjectUtility.SetParentAndAlign(go, gameObject);
            
            // Register undo and select the object
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Created Audio Source");
            UnityEditor.Selection.activeObject = go;
            #endif
        }

        private void RefreshChildrenSources()
        {
            sources = GetComponentsInChildren<AudioSource>();
        }
        
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
            music.loop = true;
            music.playOnAwake = false;
            
            GameObject sfxSource = new GameObject("SFX Source");
            var sfx = sfxSource.AddComponent<AudioSource>();
            sfx.loop = false;
            sfx.playOnAwake = false;
            
            audioManager.defaultMusicChannel = 0;
            audioManager.defaultSFXChannel = 1;
            audioManager.sources = new[] {music, sfx};
            
            UnityEditor.GameObjectUtility.SetParentAndAlign(musicSource, go);
            UnityEditor.GameObjectUtility.SetParentAndAlign(sfxSource, go);
        
            // Register undo and select the object
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Created Audio Manager");
            UnityEditor.Selection.activeObject = go;
        }
        
        #endif
    }

    /// <summary>
    /// Shortcut class to avoid calling AudioManager.Instance every time.
    /// </summary>
    public static class Audio
    {
        /// <summary>
        /// Stops the track currently playing in the defaultMusicChannel.
        /// </summary>
        /// <param name="fadeOutDuration">Duration of the fadeout before completely stopping the track.</param>
        public static void StopMusic(float fadeOutDuration = 0.3f)
        {
            AudioManager.Instance.StopMusic(fadeOutDuration);
        }

        /// <summary>
        /// Stops the track playing in the provided channel.
        /// </summary>
        /// <param name="channel">ID of the channel to stop.</param>
        /// <param name="fadeOutDuration">Duration of the fadeout before completely stopping the track.</param>
        public static void StopMusic(int channel, float fadeOutDuration = 0.3f)
        {
            AudioManager.Instance.StopMusic(channel, fadeOutDuration);
        }

        /// <summary>
        /// Plays the provided track in the defaultMusicChannel. 
        /// </summary>
        /// <param name="id">ID of the track to play (Generally stored in the AudioIDs class).</param>
        /// <param name="loop">Set the channel to repeat playing tracks.</param>
        /// <param name="volume">Override volume for the track.</param>
        public static void PlayMusic(string id, bool loop = true, float volume = 1f)
        {
            AudioManager.Instance.PlayMusic(id, loop, volume);
        }

        /// <summary>
        /// Plays the provided track in the provided channel.
        /// </summary>
        /// <param name="channel">ID of the channel to use to play the track.</param>
        /// <param name="id">ID of the track to play (Generally stored in the AudioIDs class).</param>
        /// <param name="loop">Set the channel to repeat playing tracks.</param>
        /// <param name="volume">Override volume for the track.</param>
        public static void PlayMusic(int channel, string id, bool loop = true, float volume = 1f)
        {
            AudioManager.Instance.PlayMusic(channel, id, loop, volume);
        }

        /// <summary>
        /// Plays the provided track in the defaultSFXChannel.
        /// </summary>
        /// <param name="id">ID of the track to play (Generally stored in the AudioIDs class).</param>
        /// <param name="volume">Override volume for the track.</param>
        public static void PlaySFX(string id, float volume = 1f)
        {
            AudioManager.Instance.PlaySFX(id, volume);
        }

        /// <summary>
        /// Plays the provided track in the provided channel.
        /// </summary>
        /// <param name="channel">ID of the channel to use to play the track.</param>
        /// <param name="id">ID of the track to play (Generally stored in the AudioIDs class).</param>
        /// <param name="volume">Override volume for the track.</param>
        public static void PlaySFX(int channel, string id, float volume = 1f)
        {
            AudioManager.Instance.PlaySFX(channel, id, volume);
        }
    }
}