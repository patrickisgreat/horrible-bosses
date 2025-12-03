using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HorribleBosses.Managers
{
    /// <summary>
    /// Central game manager handling game state, spawning, and flow.
    /// Singleton pattern for easy access.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Player.PlayerHealth playerHealth;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform bossSpawnPoint;

        [Header("Boss Settings")]
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private Boss.BossData currentBossData;

        [Header("Game Settings")]
        [SerializeField] private float respawnDelay = 3f;
        [SerializeField] private int playerLives = 3;

        [Header("Events")]
        public UnityEvent OnGameStart;
        public UnityEvent OnGamePause;
        public UnityEvent OnGameResume;
        public UnityEvent OnGameOver;
        public UnityEvent OnVictory;
        public UnityEvent<int> OnLivesChanged;
        public UnityEvent<int> OnScoreChanged;

        // State
        private GameState currentState = GameState.Menu;
        private int currentLives;
        private int currentScore;
        private Boss.BossController currentBoss;
        private bool isPaused;

        // Properties
        public GameState CurrentState => currentState;
        public bool IsPaused => isPaused;
        public int Lives => currentLives;
        public int Score => currentScore;
        public Boss.BossController CurrentBoss => currentBoss;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            currentLives = playerLives;

            if (playerHealth != null)
            {
                playerHealth.OnDeath.AddListener(HandlePlayerDeath);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameState.Playing)
                {
                    TogglePause();
                }
            }
        }

        public void StartGame()
        {
            currentState = GameState.Playing;
            currentLives = playerLives;
            currentScore = 0;
            isPaused = false;

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            SpawnBoss();
            OnGameStart?.Invoke();
            OnLivesChanged?.Invoke(currentLives);
            OnScoreChanged?.Invoke(currentScore);

            Debug.Log("Game Started - Time to show your boss who's really in charge!");
        }

        public void StartGameWithBoss(Boss.BossData bossData)
        {
            currentBossData = bossData;
            StartGame();
        }

        private void SpawnBoss()
        {
            if (bossPrefab == null || currentBossData == null) return;

            Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : Vector3.zero;
            Quaternion spawnRot = bossSpawnPoint != null ? bossSpawnPoint.rotation : Quaternion.identity;

            GameObject bossObj = Instantiate(bossPrefab, spawnPos, spawnRot);
            currentBoss = bossObj.GetComponent<Boss.BossController>();

            if (currentBoss != null)
            {
                currentBoss.Initialize(currentBossData);
                currentBoss.OnBossDefeated.AddListener(HandleBossDefeated);
            }
        }

        private void HandlePlayerDeath()
        {
            currentLives--;
            OnLivesChanged?.Invoke(currentLives);

            if (currentLives <= 0)
            {
                GameOver();
            }
            else
            {
                Invoke(nameof(RespawnPlayer), respawnDelay);
            }
        }

        private void RespawnPlayer()
        {
            if (playerHealth == null) return;

            // Move player to spawn
            if (playerSpawnPoint != null && playerHealth.transform.parent != null)
            {
                var playerTransform = playerHealth.transform.parent;
                playerTransform.position = playerSpawnPoint.position;
                playerTransform.rotation = playerSpawnPoint.rotation;
            }

            playerHealth.Respawn();
            Debug.Log($"Back to work! {currentLives} lives remaining.");
        }

        private void HandleBossDefeated(Boss.BossController boss)
        {
            AddScore(1000);
            Invoke(nameof(Victory), 2f);
        }

        public void Victory()
        {
            currentState = GameState.Victory;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            OnVictory?.Invoke();
            Debug.Log("VICTORY! You've defeated your horrible boss!");
        }

        public void GameOver()
        {
            currentState = GameState.GameOver;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            OnGameOver?.Invoke();
            Debug.Log("GAME OVER - Your boss wins this round...");
        }

        public void TogglePause()
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;

            isPaused = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            OnGamePause?.Invoke();
        }

        public void ResumeGame()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            OnGameResume?.Invoke();
        }

        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            if (currentBoss != null)
            {
                Destroy(currentBoss.gameObject);
            }
            StartGame();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            currentState = GameState.Menu;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        public void SetBossData(Boss.BossData data)
        {
            currentBossData = data;
        }
    }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Victory
    }
}
