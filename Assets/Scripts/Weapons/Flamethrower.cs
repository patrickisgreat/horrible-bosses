using UnityEngine;
using System.Collections.Generic;

namespace HorribleBosses.Weapons
{
    /// <summary>
    /// Flamethrower weapon. Cone of fire with damage over time.
    /// For when you need to really heat up the quarterly review.
    /// </summary>
    public class Flamethrower : WeaponBase
    {
        [Header("Flamethrower Settings")]
        [SerializeField] private float damagePerSecond = 40f;
        [SerializeField] private float burnDamagePerSecond = 15f;
        [SerializeField] private float burnDuration = 3f;
        [SerializeField] private float coneAngle = 30f;
        [SerializeField] private float flameRange = 10f;
        [SerializeField] private float fuelConsumptionRate = 15f;
        [SerializeField] private float maxFuel = 100f;
        [SerializeField] private float currentFuel;

        [Header("Burst Mode")]
        [SerializeField] private float burstFuelCost = 30f;
        [SerializeField] private float burstDamageMultiplier = 3f;
        [SerializeField] private float burstRange = 15f;
        [SerializeField] private float burstCooldown = 3f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem flameParticles;
        [SerializeField] private ParticleSystem burstParticles;
        [SerializeField] private Light flameLight;

        // State
        private bool isFiring;
        private float lastBurstTime;
        private Dictionary<Combat.IDamageable, float> burningTargets = new Dictionary<Combat.IDamageable, float>();

        protected override void Awake()
        {
            base.Awake();

            // Set flamethrower defaults
            weaponName = "HR Incinerator";
            weaponSlot = 3;
            damage = damagePerSecond;
            fireRate = 0f; // Continuous
            isAutomatic = true;
            usesAmmo = false; // Uses fuel
            magazineSize = (int)maxFuel;
            range = flameRange;
            recoilAmount = Vector2.zero;
            damageType = Combat.DamageType.Fire;

            currentFuel = maxFuel;
        }

        private void Update()
        {
            if (!isEquipped) return;

            // Update fuel display
            currentAmmo = Mathf.RoundToInt(currentFuel);

            // Process burning targets
            UpdateBurningTargets();
        }

        public override void HandleInput()
        {
            if (!isEquipped) return;

            // Primary fire - continuous flame
            if (Input.GetButton("Fire1") && currentFuel > 0)
            {
                StartFlame();
                ApplyFlameDamage();
            }
            else
            {
                StopFlame();
            }

            // Secondary fire - fuel burst
            if (Input.GetButtonDown("Fire2") && CanBurst())
            {
                PerformBurst();
            }
        }

        private void StartFlame()
        {
            if (!isFiring)
            {
                isFiring = true;
                if (flameParticles != null) flameParticles.Play();
                if (flameLight != null) flameLight.enabled = true;
            }

            // Consume fuel
            currentFuel -= fuelConsumptionRate * Time.deltaTime;
            if (currentFuel <= 0)
            {
                currentFuel = 0;
                StopFlame();
            }
        }

        private void StopFlame()
        {
            if (isFiring)
            {
                isFiring = false;
                if (flameParticles != null) flameParticles.Stop();
                if (flameLight != null) flameLight.enabled = false;
            }
        }

        private void ApplyFlameDamage()
        {
            Vector3 origin = playerCamera != null
                ? playerCamera.transform.position
                : transform.position;
            Vector3 forward = playerCamera != null
                ? playerCamera.transform.forward
                : transform.forward;

            // Find all colliders in range
            Collider[] hits = Physics.OverlapSphere(origin, flameRange);

            foreach (var hit in hits)
            {
                // Check if in cone
                Vector3 toTarget = (hit.transform.position - origin).normalized;
                float angle = Vector3.Angle(forward, toTarget);

                if (angle <= coneAngle / 2f)
                {
                    // Check line of sight
                    if (Physics.Raycast(origin, toTarget, out RaycastHit rayHit, flameRange))
                    {
                        if (rayHit.collider == hit)
                        {
                            Combat.IDamageable damageable = hit.GetComponent<Combat.IDamageable>();
                            if (damageable == null)
                            {
                                damageable = hit.GetComponentInParent<Combat.IDamageable>();
                            }

                            if (damageable != null && !damageable.IsDead)
                            {
                                // Apply direct flame damage
                                float damageThisFrame = damagePerSecond * Time.deltaTime;
                                damageable.TakeDamage(damageThisFrame, origin, Combat.DamageType.Fire);

                                // Set on fire (refresh burn timer)
                                SetOnFire(damageable);
                            }
                        }
                    }
                }
            }
        }

        private void SetOnFire(Combat.IDamageable target)
        {
            burningTargets[target] = Time.time + burnDuration;
        }

        private void UpdateBurningTargets()
        {
            List<Combat.IDamageable> toRemove = new List<Combat.IDamageable>();

            foreach (var kvp in burningTargets)
            {
                Combat.IDamageable target = kvp.Key;
                float burnEndTime = kvp.Value;

                if (target == null || target.IsDead || Time.time >= burnEndTime)
                {
                    toRemove.Add(target);
                    continue;
                }

                // Apply burn damage over time
                target.TakeDamage(
                    burnDamagePerSecond * Time.deltaTime,
                    Vector3.zero,
                    Combat.DamageType.Fire
                );
            }

            foreach (var target in toRemove)
            {
                burningTargets.Remove(target);
            }
        }

        private bool CanBurst()
        {
            if (currentFuel < burstFuelCost) return false;
            if (Time.time - lastBurstTime < burstCooldown) return false;
            return true;
        }

        private void PerformBurst()
        {
            lastBurstTime = Time.time;
            currentFuel -= burstFuelCost;

            // Play burst effect
            if (burstParticles != null) burstParticles.Play();

            // Screen shake
            if (playerCamera != null)
            {
                playerCamera.ShakeScreen(0.5f);
            }

            Vector3 origin = playerCamera != null
                ? playerCamera.transform.position
                : transform.position;
            Vector3 forward = playerCamera != null
                ? playerCamera.transform.forward
                : transform.forward;

            // Damage everything in burst cone
            Collider[] hits = Physics.OverlapSphere(origin, burstRange);

            foreach (var hit in hits)
            {
                Vector3 toTarget = (hit.transform.position - origin).normalized;
                float angle = Vector3.Angle(forward, toTarget);

                if (angle <= coneAngle / 2f)
                {
                    Combat.IDamageable damageable = hit.GetComponent<Combat.IDamageable>();
                    if (damageable == null)
                    {
                        damageable = hit.GetComponentInParent<Combat.IDamageable>();
                    }

                    if (damageable != null && !damageable.IsDead)
                    {
                        // Heavy burst damage
                        damageable.TakeDamage(
                            damagePerSecond * burstDamageMultiplier,
                            origin,
                            Combat.DamageType.Fire
                        );

                        // Extended burn
                        burningTargets[damageable] = Time.time + burnDuration * 2f;
                    }
                }
            }

            Debug.Log("FUEL BURST! Heating up the room temperature!");
        }

        protected override void OnFire()
        {
            // Not used - flamethrower uses continuous damage
        }

        protected override void OnEquip()
        {
            Debug.Log("Equipped: HR Incinerator - Time to burn some bridges.");
        }

        protected override void OnUnequip()
        {
            StopFlame();
            burningTargets.Clear();
        }

        /// <summary>
        /// Add fuel to the flamethrower
        /// </summary>
        public void AddFuel(float amount)
        {
            currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
        }

        public override void AddAmmo(int amount)
        {
            AddFuel(amount);
        }
    }
}
