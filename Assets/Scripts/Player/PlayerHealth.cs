using UnityEngine;
using UnityEngine.Events;

namespace HorribleBosses.Player
{
    /// <summary>
    /// Manages player health, damage, death, and regeneration.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Regeneration")]
        [SerializeField] private bool enableRegeneration = true;
        [SerializeField] private float regenDelay = 5f;      // Seconds after damage before regen starts
        [SerializeField] private float regenRate = 10f;      // Health per second

        [Header("Damage Feedback")]
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private float damageShakeIntensity = 0.3f;

        [Header("Events")]
        public UnityEvent<float, float> OnHealthChanged;  // current, max
        public UnityEvent OnDeath;
        public UnityEvent OnRespawn;
        public UnityEvent<float> OnDamageTaken;           // damage amount

        // Internal state
        private float lastDamageTime;
        private bool isDead;

        // Public properties
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => currentHealth / maxHealth;
        public bool IsDead => isDead;

        private void Start()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Update()
        {
            if (isDead) return;

            HandleRegeneration();
        }

        private void HandleRegeneration()
        {
            if (!enableRegeneration) return;
            if (currentHealth >= maxHealth) return;
            if (Time.time - lastDamageTime < regenDelay) return;

            // Regenerate health
            currentHealth = Mathf.Min(currentHealth + regenRate * Time.deltaTime, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Apply damage to the player
        /// </summary>
        public void TakeDamage(float damage, Vector3 damageSource = default)
        {
            if (isDead) return;
            if (damage <= 0) return;

            currentHealth -= damage;
            lastDamageTime = Time.time;

            // Screen shake feedback
            if (playerCamera != null)
            {
                playerCamera.ShakeScreen(damageShakeIntensity);
            }

            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal the player
        /// </summary>
        public void Heal(float amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Set health to a specific value
        /// </summary>
        public void SetHealth(float health)
        {
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;
            currentHealth = 0;

            OnDeath?.Invoke();

            Debug.Log("Player died!");

            // GameManager will handle respawn/game over
        }

        /// <summary>
        /// Respawn the player with full health
        /// </summary>
        public void Respawn()
        {
            isDead = false;
            currentHealth = maxHealth;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnRespawn?.Invoke();
        }

        /// <summary>
        /// Check if player can take damage (for invincibility frames, etc.)
        /// </summary>
        public bool CanTakeDamage()
        {
            return !isDead;
        }
    }
}
