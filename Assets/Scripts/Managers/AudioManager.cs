using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace HorribleBosses.Managers
{
    /// <summary>
    /// Centralized audio management for music, SFX, and ambient sounds.
    /// Singleton for easy access throughout the game.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip combatMusic;
        [SerializeField] private AudioClip combatIntenseMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip gameOverMusic;

        [Header("Settings")]
        [SerializeField] private float musicFadeDuration = 1f;
        [SerializeField] private float intenseMusicHealthThreshold = 0.3f;

        [Header("SFX Pooling")]
        [SerializeField] private int sfxPoolSize = 10;
        [SerializeField] private GameObject sfxSourcePrefab;

        // Audio source pool for overlapping SFX
        private List<AudioSource> sfxPool = new List<AudioSource>();
        private int currentPoolIndex;

        // State
        private float targetMusicVolume = 1f;
        private bool isFading;
        private AudioClip pendingMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSFXPool();
            LoadVolumeSettings();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart.AddListener(OnGameStart);
                GameManager.Instance.OnVictory.AddListener(OnVictory);
                GameManager.Instance.OnGameOver.AddListener(OnGameOver);
            }
        }

        private void InitializeSFXPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                AudioSource source;
                if (sfxSourcePrefab != null)
                {
                    GameObject obj = Instantiate(sfxSourcePrefab, transform);
                    source = obj.GetComponent<AudioSource>();
                }
                else
                {
                    GameObject obj = new GameObject($"SFX_Source_{i}");
                    obj.transform.SetParent(transform);
                    source = obj.AddComponent<AudioSource>();
                    source.playOnAwake = false;
                }
                sfxPool.Add(source);
            }
        }

        private void LoadVolumeSettings()
        {
            SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 0.7f));
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
        }

        // Volume control
        public void SetMasterVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                audioMixer.SetFloat(masterVolumeParam, VolumeToDecibels(volume));
            }
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }

        public void SetMusicVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                audioMixer.SetFloat(musicVolumeParam, VolumeToDecibels(volume));
            }
            else if (musicSource != null)
            {
                musicSource.volume = volume;
            }
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }

        public void SetSFXVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                audioMixer.SetFloat(sfxVolumeParam, VolumeToDecibels(volume));
            }
            else if (sfxSource != null)
            {
                sfxSource.volume = volume;
            }
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }

        private float VolumeToDecibels(float volume)
        {
            return volume > 0 ? Mathf.Log10(volume) * 20f : -80f;
        }

        // Music control
        public void PlayMusic(AudioClip clip, bool fade = true)
        {
            if (clip == null) return;

            if (fade && musicSource.isPlaying)
            {
                pendingMusic = clip;
                StartCoroutine(FadeMusic(0f, () =>
                {
                    musicSource.clip = pendingMusic;
                    musicSource.Play();
                    StartCoroutine(FadeMusic(targetMusicVolume, null));
                }));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.volume = targetMusicVolume;
                musicSource.Play();
            }
        }

        public void StopMusic(bool fade = true)
        {
            if (fade)
            {
                StartCoroutine(FadeMusic(0f, () => musicSource.Stop()));
            }
            else
            {
                musicSource.Stop();
            }
        }

        private System.Collections.IEnumerator FadeMusic(float targetVolume, System.Action onComplete)
        {
            isFading = true;
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / musicFadeDuration);
                yield return null;
            }

            musicSource.volume = targetVolume;
            isFading = false;
            onComplete?.Invoke();
        }

        // SFX control
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetPooledSource();
            source.clip = clip;
            source.volume = volume * PlayerPrefs.GetFloat("SFXVolume", 1f);
            source.pitch = pitch;
            source.Play();
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetPooledSource();
            source.transform.position = position;
            source.spatialBlend = 1f; // 3D sound
            source.clip = clip;
            source.volume = volume * PlayerPrefs.GetFloat("SFXVolume", 1f);
            source.pitch = pitch;
            source.Play();
        }

        public void PlaySFXWithVariation(AudioClip clip, float volumeVariation = 0.1f, float pitchVariation = 0.1f)
        {
            float volume = 1f + Random.Range(-volumeVariation, volumeVariation);
            float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            PlaySFX(clip, volume, pitch);
        }

        private AudioSource GetPooledSource()
        {
            AudioSource source = sfxPool[currentPoolIndex];
            currentPoolIndex = (currentPoolIndex + 1) % sfxPool.Count;
            source.spatialBlend = 0f; // Reset to 2D
            return source;
        }

        // Ambient control
        public void PlayAmbient(AudioClip clip, float volume = 0.5f)
        {
            if (ambientSource == null || clip == null) return;

            ambientSource.clip = clip;
            ambientSource.volume = volume;
            ambientSource.loop = true;
            ambientSource.Play();
        }

        public void StopAmbient()
        {
            if (ambientSource != null)
                ambientSource.Stop();
        }

        // Game state music
        private void OnGameStart()
        {
            PlayMusic(combatMusic);
        }

        private void OnVictory()
        {
            PlayMusic(victoryMusic);
        }

        private void OnGameOver()
        {
            PlayMusic(gameOverMusic);
        }

        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        public void PlayCombatMusic(bool intense = false)
        {
            AudioClip clip = intense ? combatIntenseMusic : combatMusic;
            if (clip == null) clip = combatMusic;
            PlayMusic(clip);
        }

        // Called by boss health to intensify music
        public void OnBossHealthChanged(float healthPercent)
        {
            if (healthPercent <= intenseMusicHealthThreshold && combatIntenseMusic != null)
            {
                if (musicSource.clip != combatIntenseMusic)
                {
                    PlayCombatMusic(true);
                }
            }
        }
    }
}
