using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HorribleBosses.UI
{
    /// <summary>
    /// Manages the in-game HUD displaying player and boss information.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Player Health")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color hurtColor = Color.red;

        [Header("Ammo")]
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text weaponNameText;
        [SerializeField] private Image weaponIcon;

        [Header("Boss Health")]
        [SerializeField] private GameObject bossHealthPanel;
        [SerializeField] private Slider bossHealthSlider;
        [SerializeField] private Image bossHealthFill;
        [SerializeField] private TMP_Text bossNameText;
        [SerializeField] private TMP_Text bossTitleText;
        [SerializeField] private Color bossHealthColor = Color.red;
        [SerializeField] private Color bossRageColor = new Color(1f, 0.3f, 0f);

        [Header("Game Info")]
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text scoreText;

        [Header("Status Effects")]
        [SerializeField] private Image damageVignette;
        [SerializeField] private float vignetteDecaySpeed = 2f;

        [Header("Crosshair")]
        [SerializeField] private Image crosshair;
        [SerializeField] private Color normalCrosshairColor = Color.white;
        [SerializeField] private Color enemyCrosshairColor = Color.red;

        [Header("References")]
        [SerializeField] private Player.PlayerHealth playerHealth;
        [SerializeField] private Weapons.WeaponManager weaponManager;

        private Boss.BossController currentBoss;
        private float currentVignetteAlpha;

        private void Start()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged.AddListener(UpdatePlayerHealth);
                playerHealth.OnDamageTaken.AddListener(OnPlayerDamaged);
            }

            if (weaponManager != null)
            {
                weaponManager.OnWeaponChanged.AddListener(UpdateWeaponDisplay);
                weaponManager.OnAmmoChanged.AddListener(UpdateAmmoDisplay);
            }

            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.OnLivesChanged.AddListener(UpdateLives);
                Managers.GameManager.Instance.OnScoreChanged.AddListener(UpdateScore);
            }

            if (bossHealthPanel != null)
                bossHealthPanel.SetActive(false);

            if (damageVignette != null)
                SetVignetteAlpha(0f);
        }

        private void Update()
        {
            UpdateVignette();
            UpdateCrosshair();
        }

        public void SetBoss(Boss.BossController boss)
        {
            currentBoss = boss;

            if (boss != null)
            {
                if (bossHealthPanel != null)
                    bossHealthPanel.SetActive(true);

                if (bossNameText != null)
                    bossNameText.text = boss.BossName;

                if (bossTitleText != null)
                    bossTitleText.text = boss.BossTitle;

                boss.Health.OnHealthChanged.AddListener(UpdateBossHealth);
                boss.Health.OnRageTriggered.AddListener(OnBossRage);

                UpdateBossHealth(boss.Health.CurrentHealth, boss.Health.MaxHealth);
            }
            else
            {
                if (bossHealthPanel != null)
                    bossHealthPanel.SetActive(false);
            }
        }

        private void UpdatePlayerHealth(float current, float max)
        {
            float percent = current / max;

            if (healthSlider != null)
                healthSlider.value = percent;

            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

            if (healthFill != null)
                healthFill.color = Color.Lerp(hurtColor, healthyColor, percent);
        }

        private void OnPlayerDamaged(float damage)
        {
            currentVignetteAlpha = Mathf.Clamp01(damage / 30f);
        }

        private void UpdateVignette()
        {
            if (damageVignette == null) return;

            if (currentVignetteAlpha > 0)
            {
                currentVignetteAlpha -= vignetteDecaySpeed * Time.deltaTime;
                SetVignetteAlpha(currentVignetteAlpha);
            }
        }

        private void SetVignetteAlpha(float alpha)
        {
            if (damageVignette == null) return;
            Color c = damageVignette.color;
            c.a = alpha;
            damageVignette.color = c;
        }

        private void UpdateWeaponDisplay(Weapons.WeaponBase weapon)
        {
            if (weapon == null) return;

            if (weaponNameText != null)
                weaponNameText.text = weapon.WeaponName;

            if (weaponIcon != null && weapon.WeaponIcon != null)
            {
                weaponIcon.sprite = weapon.WeaponIcon;
                weaponIcon.enabled = true;
            }
        }

        private void UpdateAmmoDisplay(int current, int magazine, int reserve)
        {
            if (ammoText == null) return;

            if (reserve < 0)
                ammoText.text = $"{current} / ∞";
            else
                ammoText.text = $"{current} / {reserve}";
        }

        private void UpdateBossHealth(float current, float max)
        {
            float percent = current / max;

            if (bossHealthSlider != null)
                bossHealthSlider.value = percent;
        }

        private void OnBossRage()
        {
            if (bossHealthFill != null)
                bossHealthFill.color = bossRageColor;

            if (bossNameText != null)
                bossNameText.text = currentBoss.BossName + " [ENRAGED]";
        }

        private void UpdateLives(int lives)
        {
            if (livesText != null)
                livesText.text = $"Lives: {lives}";
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score:N0}";
        }

        private void UpdateCrosshair()
        {
            if (crosshair == null) return;

            // Raycast to check if aiming at enemy
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                var damageable = hit.collider.GetComponentInParent<Combat.IDamageable>();
                crosshair.color = damageable != null ? enemyCrosshairColor : normalCrosshairColor;
            }
            else
            {
                crosshair.color = normalCrosshairColor;
            }
        }

        public void ShowDamageNumber(Vector3 worldPos, float damage, bool isCritical = false)
        {
            // Could spawn floating damage text here
            // Implement with object pooling for performance
        }
    }
}
