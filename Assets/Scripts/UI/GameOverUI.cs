using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HorribleBosses.UI
{
    /// <summary>
    /// Handles victory and game over screens.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("Victory Panel")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text victoryTitleText;
        [SerializeField] private TMP_Text victoryBossNameText;
        [SerializeField] private TMP_Text victoryScoreText;
        [SerializeField] private TMP_Text victoryTimeText;
        [SerializeField] private Button victoryRestartButton;
        [SerializeField] private Button victoryMenuButton;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text gameOverTitleText;
        [SerializeField] private TMP_Text gameOverBossNameText;
        [SerializeField] private TMP_Text gameOverTauntText;
        [SerializeField] private Button gameOverRestartButton;
        [SerializeField] private Button gameOverMenuButton;

        [Header("Victory Messages")]
        [SerializeField] private string[] victoryMessages = new string[]
        {
            "YOU'RE PROMOTED!",
            "BOSS DEFEATED!",
            "VICTORY!",
            "YOU'RE THE BOSS NOW!",
            "HOSTILE TAKEOVER COMPLETE!"
        };

        [Header("Game Over Messages")]
        [SerializeField] private string[] gameOverMessages = new string[]
        {
            "YOU'RE FIRED!",
            "PERFORMANCE REVIEW: FAILED",
            "SEVERANCE DENIED",
            "BACK TO THE CUBICLE",
            "HR WILL BE IN TOUCH"
        };

        [Header("Boss Taunts")]
        [SerializeField] private string[] bossTaunts = new string[]
        {
            "Did you really think you could beat me?",
            "That's going on your permanent record.",
            "See me in my office. Oh wait, you can't.",
            "I expected more from you. Disappointed.",
            "This will affect your bonus."
        };

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip gameOverMusic;
        [SerializeField] private AudioClip buttonClickSound;

        private float gameStartTime;

        private void Start()
        {
            SetupButtons();
            HideAll();

            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.OnGameStart.AddListener(OnGameStart);
                Managers.GameManager.Instance.OnVictory.AddListener(ShowVictory);
                Managers.GameManager.Instance.OnGameOver.AddListener(ShowGameOver);
            }
        }

        private void SetupButtons()
        {
            if (victoryRestartButton != null)
                victoryRestartButton.onClick.AddListener(OnRestartClicked);

            if (victoryMenuButton != null)
                victoryMenuButton.onClick.AddListener(OnMenuClicked);

            if (gameOverRestartButton != null)
                gameOverRestartButton.onClick.AddListener(OnRestartClicked);

            if (gameOverMenuButton != null)
                gameOverMenuButton.onClick.AddListener(OnMenuClicked);
        }

        private void HideAll()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void OnGameStart()
        {
            gameStartTime = Time.time;
            HideAll();
        }

        public void ShowVictory()
        {
            HideAll();

            if (victoryPanel != null)
                victoryPanel.SetActive(true);

            // Random victory message
            if (victoryTitleText != null)
                victoryTitleText.text = victoryMessages[Random.Range(0, victoryMessages.Length)];

            // Boss name
            if (victoryBossNameText != null && Managers.GameManager.Instance?.CurrentBoss != null)
            {
                var boss = Managers.GameManager.Instance.CurrentBoss;
                victoryBossNameText.text = $"Defeated: {boss.BossName}\n<size=70%>{boss.BossTitle}</size>";
            }

            // Score
            if (victoryScoreText != null && Managers.GameManager.Instance != null)
            {
                victoryScoreText.text = $"Final Score: {Managers.GameManager.Instance.Score:N0}";
            }

            // Time
            if (victoryTimeText != null)
            {
                float totalTime = Time.time - gameStartTime;
                int minutes = Mathf.FloorToInt(totalTime / 60f);
                int seconds = Mathf.FloorToInt(totalTime % 60f);
                victoryTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }

            // Play victory music
            if (audioSource != null && victoryMusic != null)
            {
                audioSource.clip = victoryMusic;
                audioSource.Play();
            }
        }

        public void ShowGameOver()
        {
            HideAll();

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            // Random game over message
            if (gameOverTitleText != null)
                gameOverTitleText.text = gameOverMessages[Random.Range(0, gameOverMessages.Length)];

            // Boss name
            if (gameOverBossNameText != null && Managers.GameManager.Instance?.CurrentBoss != null)
            {
                var boss = Managers.GameManager.Instance.CurrentBoss;
                gameOverBossNameText.text = $"{boss.BossName}\n<size=70%>{boss.BossTitle}</size>";
            }

            // Random boss taunt
            if (gameOverTauntText != null)
            {
                string taunt = bossTaunts[Random.Range(0, bossTaunts.Length)];

                // Use boss catchphrase if available
                if (Managers.GameManager.Instance?.CurrentBoss?.Data != null)
                {
                    string catchphrase = Managers.GameManager.Instance.CurrentBoss.Data.catchphrase;
                    if (!string.IsNullOrEmpty(catchphrase))
                    {
                        taunt = catchphrase;
                    }
                }

                gameOverTauntText.text = $"\"{taunt}\"";
            }

            // Play game over music
            if (audioSource != null && gameOverMusic != null)
            {
                audioSource.clip = gameOverMusic;
                audioSource.Play();
            }
        }

        private void PlayClickSound()
        {
            if (audioSource != null && buttonClickSound != null)
            {
                audioSource.PlayOneShot(buttonClickSound);
            }
        }

        private void OnRestartClicked()
        {
            PlayClickSound();
            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.RestartGame();
            }
            HideAll();
        }

        private void OnMenuClicked()
        {
            PlayClickSound();
            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.ReturnToMenu();
            }
        }
    }
}
