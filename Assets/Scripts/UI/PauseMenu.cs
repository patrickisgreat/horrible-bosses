using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HorribleBosses.UI
{
    /// <summary>
    /// In-game pause menu with resume, restart, options, and quit.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject confirmQuitPanel;

        [Header("Pause Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("Options")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Button optionsBackButton;

        [Header("Confirm Quit")]
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        [SerializeField] private TMP_Text confirmText;

        [Header("Audio")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip buttonClickSound;

        private System.Action pendingConfirmAction;

        private void Start()
        {
            SetupButtons();
            LoadOptions();
            HideAll();

            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.OnGamePause.AddListener(Show);
                Managers.GameManager.Instance.OnGameResume.AddListener(Hide);
            }
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (optionsButton != null)
                optionsButton.onClick.AddListener(OnOptionsClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (optionsBackButton != null)
                optionsBackButton.onClick.AddListener(OnOptionsBackClicked);

            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(OnConfirmYes);

            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(OnConfirmNo);

            // Options sliders
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (sensitivitySlider != null)
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        private void LoadOptions()
        {
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

            if (sensitivitySlider != null)
                sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 100f);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            ShowPanel(pausePanel);
        }

        public void Hide()
        {
            HideAll();
            gameObject.SetActive(false);
        }

        private void HideAll()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (confirmQuitPanel != null) confirmQuitPanel.SetActive(false);
        }

        private void ShowPanel(GameObject panel)
        {
            HideAll();
            if (panel != null) panel.SetActive(true);
        }

        private void PlayClickSound()
        {
            if (sfxSource != null && buttonClickSound != null)
            {
                sfxSource.PlayOneShot(buttonClickSound);
            }
        }

        private void ShowConfirmDialog(string message, System.Action onConfirm)
        {
            pendingConfirmAction = onConfirm;
            if (confirmText != null)
                confirmText.text = message;
            ShowPanel(confirmQuitPanel);
        }

        // Button handlers
        private void OnResumeClicked()
        {
            PlayClickSound();
            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.ResumeGame();
            }
        }

        private void OnRestartClicked()
        {
            PlayClickSound();
            ShowConfirmDialog("Restart the fight?\nAll progress will be lost.", () =>
            {
                if (Managers.GameManager.Instance != null)
                {
                    Managers.GameManager.Instance.RestartGame();
                }
                Hide();
            });
        }

        private void OnOptionsClicked()
        {
            PlayClickSound();
            ShowPanel(optionsPanel);
        }

        private void OnMainMenuClicked()
        {
            PlayClickSound();
            ShowConfirmDialog("Return to main menu?\nAll progress will be lost.", () =>
            {
                if (Managers.GameManager.Instance != null)
                {
                    Managers.GameManager.Instance.ReturnToMenu();
                }
            });
        }

        private void OnQuitClicked()
        {
            PlayClickSound();
            ShowConfirmDialog("Quit to desktop?\nAll progress will be lost.", () =>
            {
                if (Managers.GameManager.Instance != null)
                {
                    Managers.GameManager.Instance.QuitGame();
                }
            });
        }

        private void OnOptionsBackClicked()
        {
            PlayClickSound();
            PlayerPrefs.Save();
            ShowPanel(pausePanel);
        }

        private void OnConfirmYes()
        {
            PlayClickSound();
            pendingConfirmAction?.Invoke();
            pendingConfirmAction = null;
        }

        private void OnConfirmNo()
        {
            PlayClickSound();
            pendingConfirmAction = null;
            ShowPanel(pausePanel);
        }

        // Options handlers
        private void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            // AudioManager would apply this
        }

        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        private void OnSensitivityChanged(float value)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", value);
            // Apply to player camera
            var playerCam = FindObjectOfType<Player.PlayerCamera>();
            if (playerCam != null)
            {
                playerCam.Sensitivity = value;
            }
        }
    }
}
