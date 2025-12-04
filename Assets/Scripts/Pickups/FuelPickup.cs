using UnityEngine;

namespace HorribleBosses.Pickups
{
    /// <summary>
    /// Fuel pickup - restores fuel for chainsaw and flamethrower.
    /// Office themed: Gas can, lighter fluid, "motivation".
    /// </summary>
    public class FuelPickup : PickupBase
    {
        public enum FuelType
        {
            All,            // Both chainsaw and flamethrower
            Chainsaw,
            Flamethrower
        }

        [Header("Fuel Settings")]
        [SerializeField] private FuelType fuelType = FuelType.All;
        [SerializeField] private int fuelAmount = 50;

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

            switch (fuelType)
            {
                case FuelType.All:
                    weaponManager.AddAmmoToWeapon<Weapons.Chainsaw>(fuelAmount);
                    weaponManager.AddAmmoToWeapon<Weapons.Flamethrower>(fuelAmount);
                    Debug.Log($"Picked up fuel: +{fuelAmount} (Found the janitor's stash!)");
                    break;

                case FuelType.Chainsaw:
                    weaponManager.AddAmmoToWeapon<Weapons.Chainsaw>(fuelAmount);
                    Debug.Log($"Picked up chainsaw fuel: +{fuelAmount}");
                    break;

                case FuelType.Flamethrower:
                    weaponManager.AddAmmoToWeapon<Weapons.Flamethrower>(fuelAmount);
                    Debug.Log($"Picked up flamethrower fuel: +{fuelAmount}");
                    break;
            }
        }
    }
}
