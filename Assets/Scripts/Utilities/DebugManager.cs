using UnityEngine;

namespace HorribleBosses.Utilities
{
    /// <summary>
    /// Debug manager with cheat codes for testing.
    /// Press ` (backtick) to toggle debug menu.
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
        [SerializeField] private bool enableInBuild = false;

        private bool showDebugMenu;
        private bool godMode;
        private bool infiniteAmmo;
        private bool oneHitKill;
        private float timeScale = 1f;

        // References (found at runtime)
        private Player.PlayerHealth playerHealth;
        private Weapons.WeaponManager weaponManager;
        private Boss.BossController currentBoss;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            #if !UNITY_EDITOR
            if (!enableInBuild)
            {
                enabled = false;
                return;
            }
            #endif
        }

        private void Start()
        {
            FindReferences();
        }

        private void FindReferences()
        {
            playerHealth = FindObjectOfType<Player.PlayerHealth>();
            weaponManager = FindObjectOfType<Weapons.WeaponManager>();
            currentBoss = FindObjectOfType<Boss.BossController>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                showDebugMenu = !showDebugMenu;
                if (showDebugMenu) FindReferences();
            }

            // Quick cheats
            if (Input.GetKeyDown(KeyCode.F1)) ToggleGodMode();
            if (Input.GetKeyDown(KeyCode.F2)) ToggleInfiniteAmmo();
            if (Input.GetKeyDown(KeyCode.F3)) HealPlayer();
            if (Input.GetKeyDown(KeyCode.F4)) DamageBoss();
            if (Input.GetKeyDown(KeyCode.F5)) KillBoss();
            if (Input.GetKeyDown(KeyCode.F6)) SpawnBoss();

            // Apply god mode
            if (godMode && playerHealth != null && playerHealth.CurrentHealth < playerHealth.MaxHealth)
            {
                playerHealth.Heal(playerHealth.MaxHealth);
            }
        }

        private void OnGUI()
        {
            if (!showDebugMenu) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 500));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== DEBUG MENU ===", GUILayout.Height(25));
            GUILayout.Label("Press ` to toggle | F1-F6 for quick cheats");
            GUILayout.Space(10);

            // Player cheats
            GUILayout.Label("-- Player --");
            if (GUILayout.Button($"God Mode: {(godMode ? "ON" : "OFF")} (F1)"))
                ToggleGodMode();
            if (GUILayout.Button($"Infinite Ammo: {(infiniteAmmo ? "ON" : "OFF")} (F2)"))
                ToggleInfiniteAmmo();
            if (GUILayout.Button("Heal to Full (F3)"))
                HealPlayer();
            if (GUILayout.Button("Give All Ammo"))
                GiveAmmo();

            GUILayout.Space(10);

            // Boss cheats
            GUILayout.Label("-- Boss --");
            if (GUILayout.Button("Damage Boss -100 HP (F4)"))
                DamageBoss();
            if (GUILayout.Button("Kill Boss (F5)"))
                KillBoss();
            if (GUILayout.Button("Spawn New Boss (F6)"))
                SpawnBoss();
            if (GUILayout.Button($"One-Hit Kill: {(oneHitKill ? "ON" : "OFF")}"))
                oneHitKill = !oneHitKill;

            GUILayout.Space(10);

            // Time controls
            GUILayout.Label("-- Time --");
            GUILayout.Label($"Time Scale: {timeScale:F1}x");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.25x")) SetTimeScale(0.25f);
            if (GUILayout.Button("0.5x")) SetTimeScale(0.5f);
            if (GUILayout.Button("1x")) SetTimeScale(1f);
            if (GUILayout.Button("2x")) SetTimeScale(2f);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Info
            GUILayout.Label("-- Info --");
            if (playerHealth != null)
                GUILayout.Label($"Player HP: {playerHealth.CurrentHealth:F0}/{playerHealth.MaxHealth:F0}");
            if (currentBoss != null && currentBoss.Health != null)
                GUILayout.Label($"Boss HP: {currentBoss.Health.CurrentHealth:F0}/{currentBoss.Health.MaxHealth:F0}");
            GUILayout.Label($"FPS: {1f / Time.unscaledDeltaTime:F0}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void ToggleGodMode()
        {
            godMode = !godMode;
            Debug.Log($"God Mode: {(godMode ? "ENABLED" : "DISABLED")}");
        }

        private void ToggleInfiniteAmmo()
        {
            infiniteAmmo = !infiniteAmmo;
            Debug.Log($"Infinite Ammo: {(infiniteAmmo ? "ENABLED" : "DISABLED")}");

            // Apply to weapons
            if (infiniteAmmo && weaponManager != null)
            {
                foreach (var weapon in weaponManager.Weapons)
                {
                    if (weapon != null) weapon.AddAmmo(9999);
                }
            }
        }

        private void HealPlayer()
        {
            if (playerHealth != null)
            {
                playerHealth.Heal(playerHealth.MaxHealth);
                Debug.Log("Player healed to full!");
            }
        }

        private void GiveAmmo()
        {
            if (weaponManager != null)
            {
                foreach (var weapon in weaponManager.Weapons)
                {
                    if (weapon != null) weapon.AddAmmo(999);
                }
                Debug.Log("All ammo refilled!");
            }
        }

        private void DamageBoss()
        {
            FindReferences();
            if (currentBoss != null && currentBoss.Health != null)
            {
                float damage = oneHitKill ? 99999f : 100f;
                currentBoss.Health.TakeDamage(damage, Vector3.zero);
                Debug.Log($"Dealt {damage} damage to boss!");
            }
        }

        private void KillBoss()
        {
            FindReferences();
            if (currentBoss != null && currentBoss.Health != null)
            {
                currentBoss.Health.TakeDamage(99999f, Vector3.zero);
                Debug.Log("Boss eliminated!");
            }
        }

        private void SpawnBoss()
        {
            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.RestartGame();
                Debug.Log("New boss spawned!");
            }
        }

        private void SetTimeScale(float scale)
        {
            timeScale = scale;
            Time.timeScale = scale;
            Debug.Log($"Time scale set to {scale}x");
        }
    }
}
