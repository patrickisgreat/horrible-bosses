using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace HorribleBosses.UI
{
    /// <summary>
    /// Main menu controller with play, boss creator, options, and quit.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject bossSelectPanel;

        [Header("Main Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button bossCreatorButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        [Header("Options")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Button optionsBackButton;

        [Header("Boss Selection")]
        [SerializeField] private Boss.BossData[] presetBosses;
        [SerializeField] private Transform bossButtonContainer;
        [SerializeField] private Button bossButtonPrefab;
        [SerializeField] private Button bossSelectBackButton;
        [SerializeField] private Button customBossButton;

        [Header("Scenes")]
        [SerializeField] private string gameSceneName = "Arena";
        [SerializeField] private string bossCreatorSceneName = "BossCreator";

        [Header("Audio")]
        [SerializeField] private AudioSource menuMusic;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioSource sfxSource;

        private Boss.BossData selectedBoss;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;

            SetupButtons();
            SetupOptions();
            PopulateBossSelection();

            ShowPanel(mainPanel);
        }

        private void SetupButtons()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (bossCreatorButton != null)
                bossCreatorButton.onClick.AddListener(OnBossCreatorClicked);

            if (optionsButton != null)
                optionsButton.onClick.AddListener(OnOptionsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (optionsBackButton != null)
                optionsBackButton.onClick.AddListener(OnOptionsBackClicked);

            if (bossSelectBackButton != null)
                bossSelectBackButton.onClick.AddListener(OnBossSelectBackClicked);

            if (customBossButton != null)
                customBossButton.onClick.AddListener(OnBossCreatorClicked);
        }

        private void SetupOptions()
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (sensitivitySlider != null)
            {
                sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 100f);
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            if (qualityDropdown != null)
            {
                qualityDropdown.value = QualitySettings.GetQualityLevel();
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
        }

        private void PopulateBossSelection()
        {
            if (bossButtonContainer == null || bossButtonPrefab == null) return;

            foreach (var bossData in presetBosses)
            {
                if (bossData == null) continue;

                Button btn = Instantiate(bossButtonPrefab, bossButtonContainer);
                TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();

                if (btnText != null)
                {
                    btnText.text = $"{bossData.bossName}\n<size=70%>{bossData.title}</size>\n<size=60%>[{bossData.difficulty}]</size>";
                }

                Boss.BossData bossCopy = bossData;
                btn.onClick.AddListener(() => SelectBoss(bossCopy));
            }
        }

        private void ShowPanel(GameObject panel)
        {
            if (mainPanel != null) mainPanel.SetActive(panel == mainPanel);
            if (optionsPanel != null) optionsPanel.SetActive(panel == optionsPanel);
            if (bossSelectPanel != null) bossSelectPanel.SetActive(panel == bossSelectPanel);
        }

        private void PlayClickSound()
        {
            if (sfxSource != null && buttonClickSound != null)
            {
                sfxSource.PlayOneShot(buttonClickSound);
            }
        }

        // Button handlers
        private void OnPlayClicked()
        {
            PlayClickSound();
            ShowPanel(bossSelectPanel);
        }

        private void OnBossCreatorClicked()
        {
            PlayClickSound();
            SceneManager.LoadScene(bossCreatorSceneName);
        }

        private void OnOptionsClicked()
        {
            PlayClickSound();
            ShowPanel(optionsPanel);
        }

        private void OnQuitClicked()
        {
            PlayClickSound();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void OnOptionsBackClicked()
        {
            PlayClickSound();
            SaveOptions();
            ShowPanel(mainPanel);
        }

        private void OnBossSelectBackClicked()
        {
            PlayClickSound();
            ShowPanel(mainPanel);
        }

        private void SelectBoss(Boss.BossData boss)
        {
            PlayClickSound();
            selectedBoss = boss;
            StartGameWithBoss(boss);
        }

        private void StartGameWithBoss(Boss.BossData boss)
        {
            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.SetBossData(boss);
            }

            SceneManager.LoadScene(gameSceneName);
        }

        // Options handlers
        private void OnMusicVolumeChanged(float value)
        {
            if (menuMusic != null)
                menuMusic.volume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (sfxSource != null)
                sfxSource.volume = value;
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        private void OnSensitivityChanged(float value)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", value);
        }

        private void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        private void OnQualityChanged(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }

        private void SaveOptions()
        {
            PlayerPrefs.Save();
        }
    }
}
