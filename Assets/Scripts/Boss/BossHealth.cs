using UnityEngine;
using UnityEngine.Events;
using HorribleBosses.Combat;

namespace HorribleBosses.Boss
{
    /// <summary>
    /// Manages boss health, damage reactions, and death.
    /// Implements IDamageable for weapon compatibility.
    /// </summary>
    public class BossHealth : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 200f;
        [SerializeField] private float currentHealth;

        [Header("Damage Resistance")]
        [SerializeField] private float normalResistance = 0f;    // % damage reduction
        [SerializeField] private float fireResistance = 0f;
        [SerializeField] private float explosiveResistance = 0f;
        [SerializeField] private float meleeResistance = -0.25f; // Negative = weakness

        [Header("Stagger")]
        [SerializeField] private float staggerThreshold = 30f;   // Damage to trigger stagger
        [SerializeField] private float staggerCooldown = 3f;     // Min time between staggers

        [Header("Glory Kill")]
        [SerializeField] private float gloryKillThreshold = 0.15f; // HP % for glory kill state

        [Header("Events")]
        public UnityEvent<float, float> OnHealthChanged;    // current, max
        public UnityEvent<float> OnDamageTaken;             // damage amount
        public UnityEvent OnStaggered;
        public UnityEvent OnRageTriggered;
        public UnityEvent OnGloryKillReady;
        public UnityEvent OnDeath;

        // State
        private float accumulatedDamage;
        private float lastStaggerTime;
        private bool isDead;
        private bool isInRage;
        private bool isGloryKillable;
        private float rageThreshold = 0.3f;

        // IDamageable implementation
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;

        // Additional properties
        public float HealthPercent => currentHealth / maxHealth;
        public bool IsInRage => isInRage;
        public bool IsGloryKillable => isGloryKillable;
        public bool CanBeStaggered => Time.time - lastStaggerTime >= staggerCooldown;

        /// <summary>
        /// Initialize health from BossData
        /// </summary>
        public void Initialize(BossStats stats)
        {
            maxHealth = stats.maxHealth;
            currentHealth = maxHealth;
            rageThreshold = stats.rageThreshold;
            isDead = false;
            isInRage = false;
            isGloryKillable = false;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// IDamageable implementation - receive damage from weapons
        /// </summary>
        public void TakeDamage(float damage, Vector3 damageSource = default, DamageType damageType = DamageType.Normal)
        {
            if (isDead) return;
            if (damage <= 0) return;

            // Apply resistance
            float resistance = GetResistance(damageType);
            float finalDamage = damage * (1f - resistance);

            currentHealth -= finalDamage;
            accumulatedDamage += finalDamage;

            OnDamageTaken?.Invoke(finalDamage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Check stagger
            if (accumulatedDamage >= staggerThreshold && CanBeStaggered)
            {
                TriggerStagger();
            }

            // Check rage
            if (!isInRage && HealthPercent <= rageThreshold)
            {
                TriggerRage();
            }

            // Check glory kill
            if (!isGloryKillable && HealthPercent <= gloryKillThreshold && HealthPercent > 0)
            {
                isGloryKillable = true;
                OnGloryKillReady?.Invoke();
            }

            // Check death
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private float GetResistance(DamageType type)
        {
            return type switch
            {
                DamageType.Normal => normalResistance,
                DamageType.Fire => fireResistance,
                DamageType.Explosive => explosiveResistance,
                DamageType.Melee => meleeResistance,
                _ => 0f
            };
        }

        private void TriggerStagger()
        {
            accumulatedDamage = 0f;
            lastStaggerTime = Time.time;
            OnStaggered?.Invoke();
        }

        private void TriggerRage()
        {
            isInRage = true;
            OnRageTriggered?.Invoke();
            Debug.Log($"{gameObject.name} has entered RAGE mode!");
        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            currentHealth = 0;

            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has been defeated!");
        }

        /// <summary>
        /// Heal the boss (for special abilities)
        /// </summary>
        public void Heal(float amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Exit glory kill state if healed above threshold
            if (isGloryKillable && HealthPercent > gloryKillThreshold)
            {
                isGloryKillable = false;
            }
        }

        /// <summary>
        /// Reset boss for respawn/restart
        /// </summary>
        public void Reset()
        {
            currentHealth = maxHealth;
            isDead = false;
            isInRage = false;
            isGloryKillable = false;
            accumulatedDamage = 0f;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Set resistance values (for customization)
        /// </summary>
        public void SetResistances(float normal, float fire, float explosive, float melee)
        {
            normalResistance = Mathf.Clamp(normal, -1f, 0.9f);
            fireResistance = Mathf.Clamp(fire, -1f, 0.9f);
            explosiveResistance = Mathf.Clamp(explosive, -1f, 0.9f);
            meleeResistance = Mathf.Clamp(melee, -1f, 0.9f);
        }
    }
}
