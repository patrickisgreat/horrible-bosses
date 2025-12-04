using UnityEngine;

namespace HorribleBosses.Pickups
{
    /// <summary>
    /// Ammo pickup - restores ammo for weapons.
    /// Office themed: Stapler refills, paper clips, printer cartridges.
    /// </summary>
    public class AmmoPickup : PickupBase
    {
        public enum AmmoType
        {
            All,        // Refills all weapons
            Pistol,
            Shotgun
        }

        [Header("Ammo Settings")]
        [SerializeField] private AmmoType ammoType = AmmoType.All;
        [SerializeField] private int ammoAmount = 30;

        protected override bool CanCollect(GameObject collector)
        {
            var weaponManager = collector.GetComponent<Weapons.WeaponManager>();
            if (weaponManager == null) weaponManager = collector.GetComponentInChildren<Weapons.WeaponManager>();

            return weaponManager != null;
        }

        protected override void ApplyPickup(GameObject collector)
        {
            var weaponManager = collector.GetComponent<Weapons.WeaponManager>();
            if (weaponManager == null) weaponManager = collector.GetComponentInChildren<Weapons.WeaponManager>();

            if (weaponManager == null) return;

            switch (ammoType)
            {
                case AmmoType.All:
                    foreach (var weapon in weaponManager.Weapons)
                    {
                        if (weapon != null)
                            weapon.AddAmmo(ammoAmount);
                    }
                    Debug.Log($"Picked up ammo: +{ammoAmount} all weapons (Restocked from supply closet!)");
                    break;

                case AmmoType.Pistol:
                    weaponManager.AddAmmoToWeapon<Weapons.Pistol>(ammoAmount);
                    Debug.Log($"Picked up pistol ammo: +{ammoAmount}");
                    break;

                case AmmoType.Shotgun:
                    weaponManager.AddAmmoToWeapon<Weapons.Shotgun>(ammoAmount);
                    Debug.Log($"Picked up shotgun ammo: +{ammoAmount}");
                    break;
            }
        }
    }
}
