using UnityEngine;

namespace HorribleBosses.Weapons
{
    /// <summary>
    /// Pump-action shotgun. High damage spread shot, devastating up close.
    /// For when you really need to make a point in the meeting.
    /// </summary>
    public class Shotgun : WeaponBase
    {
        [Header("Shotgun Settings")]
        [SerializeField] private int pelletsPerShot = 8;
        [SerializeField] private float spreadAngle = 5f;
        [SerializeField] private float damagePerPellet = 12f;
        [SerializeField] private float pumpTime = 0.5f; // Time after shot before can fire again

        [Header("Effects")]
        [SerializeField] private float screenShakeIntensity = 0.4f;

        private bool isPumping;

        protected override void Awake()
        {
            base.Awake();

            // Set shotgun defaults
            weaponName = "Board Room Blaster";
            weaponSlot = 1;
            damage = damagePerPellet; // Per pellet
            fireRate = 0.8f; // Includes pump time
            isAutomatic = false;
            magazineSize = 8;
            reserveAmmo = 32;
            reloadTime = 2.5f; // Full reload
            range = 30f; // Effective range
            recoilAmount = new Vector2(5f, 2f);
            damageType = Combat.DamageType.Normal;
        }

        protected override void OnFire()
        {
            // Fire multiple pellets
            for (int i = 0; i < pelletsPerShot; i++)
            {
                DoHitscan(spreadAngle);
            }

            // Extra screen shake for the shotgun
            if (playerCamera != null)
            {
                playerCamera.ShakeScreen(screenShakeIntensity);
            }

            // Start pump animation
            isPumping = true;
            Invoke(nameof(FinishPump), pumpTime);
        }

        private void FinishPump()
        {
            isPumping = false;
            // Could play pump sound here
        }

        protected override bool CanFire()
        {
            if (isPumping) return false;
            return base.CanFire();
        }

        protected override void OnEquip()
        {
            isPumping = false;
            Debug.Log("Equipped: Board Room Blaster - Let's restructure some departments.");
        }

        protected override void OnUnequip()
        {
            isPumping = false;
            CancelInvoke(nameof(FinishPump));
        }
    }
}
