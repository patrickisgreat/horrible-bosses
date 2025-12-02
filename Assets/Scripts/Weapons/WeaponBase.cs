using UnityEngine;
using HorribleBosses.Combat;

namespace HorribleBosses.Weapons
{
    /// <summary>
    /// Abstract base class for all weapons in Horrible Bosses.
    /// Inherit from this to create new weapon types.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Weapon Info")]
        [SerializeField] protected string weaponName = "Weapon";
        [SerializeField] protected Sprite weaponIcon;
        [SerializeField] protected int weaponSlot = 0; // 0-3 for keys 1-4

        [Header("Damage")]
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected DamageType damageType = DamageType.Normal;
        [SerializeField] protected float range = 100f;

        [Header("Fire Rate")]
        [SerializeField] protected float fireRate = 0.2f; // Seconds between shots
        [SerializeField] protected bool isAutomatic = false;

        [Header("Ammo")]
        [SerializeField] protected bool usesAmmo = true;
        [SerializeField] protected int magazineSize = 12;
        [SerializeField] protected int currentAmmo;
        [SerializeField] protected int reserveAmmo = 999; // -1 for unlimited reserves
        [SerializeField] protected float reloadTime = 1.5f;

        [Header("Recoil")]
        [SerializeField] protected Vector2 recoilAmount = new Vector2(1f, 0.5f); // Vertical, horizontal
        [SerializeField] protected float recoilRecoverySpeed = 5f;

        [Header("Audio")]
        [SerializeField] protected AudioClip fireSound;
        [SerializeField] protected AudioClip reloadSound;
        [SerializeField] protected AudioClip emptySound;

        [Header("Visual Effects")]
        [SerializeField] protected Transform muzzlePoint;
        [SerializeField] protected GameObject muzzleFlashPrefab;
        [SerializeField] protected GameObject impactEffectPrefab;

        // State
        protected bool isReloading;
        protected float nextFireTime;
        protected bool isEquipped;

        // References (set by WeaponManager)
        protected Player.PlayerCamera playerCamera;
        protected AudioSource audioSource;

        // Properties
        public string WeaponName => weaponName;
        public Sprite WeaponIcon => weaponIcon;
        public int WeaponSlot => weaponSlot;
        public int CurrentAmmo => currentAmmo;
        public int MagazineSize => magazineSize;
        public int ReserveAmmo => reserveAmmo;
        public bool IsReloading => isReloading;
        public bool IsEquipped => isEquipped;
        public bool UsesFuel => !usesAmmo && magazineSize > 0; // For chainsaw/flamethrower

        protected virtual void Awake()
        {
            currentAmmo = magazineSize;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Called by WeaponManager when this weapon is equipped
        /// </summary>
        public virtual void Equip(Player.PlayerCamera camera)
        {
            playerCamera = camera;
            isEquipped = true;
            gameObject.SetActive(true);
            OnEquip();
        }

        /// <summary>
        /// Called by WeaponManager when this weapon is unequipped
        /// </summary>
        public virtual void Unequip()
        {
            isEquipped = false;
            isReloading = false;
            gameObject.SetActive(false);
            OnUnequip();
        }

        /// <summary>
        /// Override for custom equip behavior
        /// </summary>
        protected virtual void OnEquip() { }

        /// <summary>
        /// Override for custom unequip behavior
        /// </summary>
        protected virtual void OnUnequip() { }

        /// <summary>
        /// Called every frame while equipped. Handle input here.
        /// </summary>
        public virtual void HandleInput()
        {
            if (!isEquipped || isReloading) return;

            // Fire input
            bool fireInput = isAutomatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");
            if (fireInput && CanFire())
            {
                Fire();
            }
            else if (fireInput && currentAmmo <= 0 && !isReloading)
            {
                PlayEmptySound();
            }

            // Reload input
            if (Input.GetKeyDown(KeyCode.R) && CanReload())
            {
                StartReload();
            }
        }

        /// <summary>
        /// Check if the weapon can fire
        /// </summary>
        protected virtual bool CanFire()
        {
            if (Time.time < nextFireTime) return false;
            if (usesAmmo && currentAmmo <= 0) return false;
            return true;
        }

        /// <summary>
        /// Fire the weapon - override in derived classes
        /// </summary>
        protected virtual void Fire()
        {
            nextFireTime = Time.time + fireRate;

            if (usesAmmo)
            {
                currentAmmo--;
            }

            // Recoil
            if (playerCamera != null)
            {
                float horizontalRecoil = Random.Range(-recoilAmount.y, recoilAmount.y);
                playerCamera.AddRecoil(new Vector2(recoilAmount.x, horizontalRecoil));
            }

            // Sound
            PlayFireSound();

            // Muzzle flash
            SpawnMuzzleFlash();

            // Derived classes implement the actual firing logic (hitscan, projectile, etc.)
            OnFire();
        }

        /// <summary>
        /// Override to implement weapon-specific firing behavior
        /// </summary>
        protected abstract void OnFire();

        /// <summary>
        /// Check if the weapon can reload
        /// </summary>
        protected virtual bool CanReload()
        {
            if (!usesAmmo) return false;
            if (currentAmmo >= magazineSize) return false;
            if (reserveAmmo == 0) return false;
            return true;
        }

        /// <summary>
        /// Start the reload process
        /// </summary>
        protected virtual void StartReload()
        {
            if (isReloading) return;
            isReloading = true;
            PlayReloadSound();
            Invoke(nameof(FinishReload), reloadTime);
        }

        /// <summary>
        /// Complete the reload
        /// </summary>
        protected virtual void FinishReload()
        {
            isReloading = false;

            int ammoNeeded = magazineSize - currentAmmo;

            if (reserveAmmo < 0) // Unlimited reserves
            {
                currentAmmo = magazineSize;
            }
            else
            {
                int ammoToAdd = Mathf.Min(ammoNeeded, reserveAmmo);
                currentAmmo += ammoToAdd;
                reserveAmmo -= ammoToAdd;
            }
        }

        /// <summary>
        /// Add ammo to reserves (from pickups)
        /// </summary>
        public virtual void AddAmmo(int amount)
        {
            if (reserveAmmo < 0) return; // Already unlimited
            reserveAmmo += amount;
        }

        /// <summary>
        /// Perform a hitscan raycast (for instant-hit weapons)
        /// </summary>
        protected RaycastHit? DoHitscan(float spreadAngle = 0f)
        {
            Vector3 direction = playerCamera != null
                ? playerCamera.transform.forward
                : transform.forward;

            // Apply spread
            if (spreadAngle > 0)
            {
                direction = Quaternion.Euler(
                    Random.Range(-spreadAngle, spreadAngle),
                    Random.Range(-spreadAngle, spreadAngle),
                    0
                ) * direction;
            }

            Vector3 origin = playerCamera != null
                ? playerCamera.transform.position
                : muzzlePoint?.position ?? transform.position;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
            {
                // Try to damage what we hit
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable == null)
                {
                    damageable = hit.collider.GetComponentInParent<IDamageable>();
                }

                if (damageable != null)
                {
                    damageable.TakeDamage(damage, origin, damageType);
                }

                // Spawn impact effect
                SpawnImpactEffect(hit.point, hit.normal);

                return hit;
            }

            return null;
        }

        /// <summary>
        /// Spawn muzzle flash effect
        /// </summary>
        protected virtual void SpawnMuzzleFlash()
        {
            if (muzzleFlashPrefab != null && muzzlePoint != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
                Destroy(flash, 0.1f);
            }
        }

        /// <summary>
        /// Spawn impact effect at hit point
        /// </summary>
        protected virtual void SpawnImpactEffect(Vector3 position, Vector3 normal)
        {
            if (impactEffectPrefab != null)
            {
                GameObject impact = Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
                Destroy(impact, 2f);
            }
        }

        protected void PlayFireSound()
        {
            if (fireSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(fireSound);
            }
        }

        protected void PlayReloadSound()
        {
            if (reloadSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(reloadSound);
            }
        }

        protected void PlayEmptySound()
        {
            if (emptySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(emptySound);
            }
        }
    }
}
