using UnityEngine;

namespace HorribleBosses.Weapons
{
    /// <summary>
    /// Melee chainsaw weapon. Continuous damage while revved and in contact.
    /// For those performance reviews that get a little too personal.
    /// </summary>
    public class Chainsaw : WeaponBase
    {
        [Header("Chainsaw Settings")]
        [SerializeField] private float damagePerSecond = 80f;
        [SerializeField] private float meleeRange = 2.5f;
        [SerializeField] private float meleeRadius = 0.5f;
        [SerializeField] private float fuelConsumptionRate = 10f; // Fuel per second while active
        [SerializeField] private float maxFuel = 100f;
        [SerializeField] private float currentFuel;

        [Header("Glory Kill")]
        [SerializeField] private float gloryKillThreshold = 0.2f; // Health % to trigger
        [SerializeField] private float gloryKillDamage = 500f;
        [SerializeField] private float gloryKillFuelRestore = 25f;

        [Header("Audio")]
        [SerializeField] private AudioClip idleSound;
        [SerializeField] private AudioClip revSound;
        [SerializeField] private AudioClip cutSound;

        [Header("Effects")]
        [SerializeField] private ParticleSystem bloodSpray;
        [SerializeField] private float screenShakeWhileCutting = 0.15f;

        // State
        private bool isRevved;
        private bool isCutting;
        private Combat.IDamageable currentTarget;

        protected override void Awake()
        {
            base.Awake();

            // Set chainsaw defaults
            weaponName = "Corporate Restructurer";
            weaponSlot = 2;
            damage = damagePerSecond;
            fireRate = 0f; // Continuous
            isAutomatic = true;
            usesAmmo = false; // Uses fuel instead
            magazineSize = (int)maxFuel;
            range = meleeRange;
            recoilAmount = Vector2.zero;
            damageType = Combat.DamageType.Melee;

            currentFuel = maxFuel;
        }

        private void Update()
        {
            if (!isEquipped) return;

            // Update fuel display (hijacking ammo system)
            currentAmmo = Mathf.RoundToInt(currentFuel);
        }

        public override void HandleInput()
        {
            if (!isEquipped) return;

            // Hold to rev
            if (Input.GetButton("Fire1") && currentFuel > 0)
            {
                Rev();
            }
            else
            {
                StopRev();
            }
        }

        private void Rev()
        {
            if (!isRevved)
            {
                isRevved = true;
                // Play rev sound
            }

            // Consume fuel while revved
            currentFuel -= fuelConsumptionRate * 0.5f * Time.deltaTime;

            // Check for targets in melee range
            CheckMeleeHit();

            if (currentFuel <= 0)
            {
                currentFuel = 0;
                StopRev();
            }
        }

        private void StopRev()
        {
            if (isRevved)
            {
                isRevved = false;
                isCutting = false;
                currentTarget = null;
            }
        }

        private void CheckMeleeHit()
        {
            Vector3 origin = playerCamera != null
                ? playerCamera.transform.position
                : transform.position;
            Vector3 direction = playerCamera != null
                ? playerCamera.transform.forward
                : transform.forward;

            // Spherecast for melee hit
            if (Physics.SphereCast(origin, meleeRadius, direction, out RaycastHit hit, meleeRange))
            {
                Combat.IDamageable damageable = hit.collider.GetComponent<Combat.IDamageable>();
                if (damageable == null)
                {
                    damageable = hit.collider.GetComponentInParent<Combat.IDamageable>();
                }

                if (damageable != null && !damageable.IsDead)
                {
                    CutTarget(damageable, origin);
                }
                else
                {
                    isCutting = false;
                    currentTarget = null;
                }
            }
            else
            {
                isCutting = false;
                currentTarget = null;
            }
        }

        private void CutTarget(Combat.IDamageable target, Vector3 damageSource)
        {
            isCutting = true;
            currentTarget = target;

            // Apply continuous damage
            float damageThisFrame = damagePerSecond * Time.deltaTime;
            target.TakeDamage(damageThisFrame, damageSource, Combat.DamageType.Melee);

            // Consume extra fuel while cutting
            currentFuel -= fuelConsumptionRate * Time.deltaTime;

            // Screen shake while cutting
            if (playerCamera != null)
            {
                playerCamera.ShakeScreen(screenShakeWhileCutting);
            }

            // Blood spray effect
            if (bloodSpray != null && !bloodSpray.isPlaying)
            {
                bloodSpray.Play();
            }

            // Check for glory kill opportunity
            float healthPercent = target.CurrentHealth / target.MaxHealth;
            if (healthPercent <= gloryKillThreshold && healthPercent > 0)
            {
                // Could show glory kill prompt here
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PerformGloryKill(target, damageSource);
                }
            }
        }

        private void PerformGloryKill(Combat.IDamageable target, Vector3 damageSource)
        {
            // Massive damage to finish them
            target.TakeDamage(gloryKillDamage, damageSource, Combat.DamageType.Melee);

            // Restore fuel
            currentFuel = Mathf.Min(currentFuel + gloryKillFuelRestore, maxFuel);

            // Big screen shake
            if (playerCamera != null)
            {
                playerCamera.ShakeScreen(0.6f);
            }

            Debug.Log("GLORY KILL! Your performance review is... terminated.");
        }

        protected override void OnFire()
        {
            // Not used - chainsaw uses continuous damage in Update
        }

        protected override void OnEquip()
        {
            Debug.Log("Equipped: Corporate Restructurer - Time for some aggressive downsizing.");
        }

        protected override void OnUnequip()
        {
            StopRev();
            if (bloodSpray != null) bloodSpray.Stop();
        }

        /// <summary>
        /// Add fuel to the chainsaw
        /// </summary>
        public void AddFuel(float amount)
        {
            currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
        }

        // Override ammo methods to work with fuel
        public override void AddAmmo(int amount)
        {
            AddFuel(amount);
        }
    }
}
