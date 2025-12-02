using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace HorribleBosses.Weapons
{
    /// <summary>
    /// Manages weapon inventory, switching, and input handling.
    /// Attach to the player and assign weapons as children.
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Player.PlayerCamera playerCamera;
        [SerializeField] private Transform weaponHolder; // Parent transform for weapon models

        [Header("Weapons")]
        [SerializeField] private List<WeaponBase> weapons = new List<WeaponBase>();
        [SerializeField] private int startingWeaponIndex = 0;

        [Header("Switching")]
        [SerializeField] private float switchCooldown = 0.3f;

        [Header("Events")]
        public UnityEvent<WeaponBase> OnWeaponChanged;
        public UnityEvent<int, int, int> OnAmmoChanged; // current, magazine, reserve

        // State
        private int currentWeaponIndex = -1;
        private WeaponBase currentWeapon;
        private float lastSwitchTime;

        // Properties
        public WeaponBase CurrentWeapon => currentWeapon;
        public int CurrentWeaponIndex => currentWeaponIndex;
        public List<WeaponBase> Weapons => weapons;

        private void Start()
        {
            // Disable all weapons initially
            foreach (var weapon in weapons)
            {
                if (weapon != null)
                {
                    weapon.gameObject.SetActive(false);
                }
            }

            // Equip starting weapon
            if (weapons.Count > 0 && startingWeaponIndex < weapons.Count)
            {
                SwitchToWeapon(startingWeaponIndex);
            }
        }

        private void Update()
        {
            HandleWeaponSwitchInput();

            // Let current weapon handle its input
            if (currentWeapon != null)
            {
                currentWeapon.HandleInput();

                // Update ammo UI
                OnAmmoChanged?.Invoke(
                    currentWeapon.CurrentAmmo,
                    currentWeapon.MagazineSize,
                    currentWeapon.ReserveAmmo
                );
            }
        }

        private void HandleWeaponSwitchInput()
        {
            if (Time.time - lastSwitchTime < switchCooldown) return;

            // Number keys 1-4
            if (Input.GetKeyDown(KeyCode.Alpha1)) TrySwitchToSlot(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) TrySwitchToSlot(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) TrySwitchToSlot(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) TrySwitchToSlot(3);

            // Scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.1f)
            {
                SwitchToNextWeapon();
            }
            else if (scroll < -0.1f)
            {
                SwitchToPreviousWeapon();
            }
        }

        /// <summary>
        /// Try to switch to a weapon in the given slot
        /// </summary>
        private void TrySwitchToSlot(int slot)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i] != null && weapons[i].WeaponSlot == slot)
                {
                    SwitchToWeapon(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Switch to weapon by index
        /// </summary>
        public void SwitchToWeapon(int index)
        {
            if (index < 0 || index >= weapons.Count) return;
            if (weapons[index] == null) return;
            if (index == currentWeaponIndex) return;

            // Unequip current weapon
            if (currentWeapon != null)
            {
                currentWeapon.Unequip();
            }

            // Equip new weapon
            currentWeaponIndex = index;
            currentWeapon = weapons[index];
            currentWeapon.Equip(playerCamera);

            lastSwitchTime = Time.time;

            OnWeaponChanged?.Invoke(currentWeapon);
        }

        /// <summary>
        /// Switch to the next weapon in the list
        /// </summary>
        public void SwitchToNextWeapon()
        {
            if (weapons.Count <= 1) return;

            int nextIndex = (currentWeaponIndex + 1) % weapons.Count;

            // Skip null weapons
            int attempts = 0;
            while (weapons[nextIndex] == null && attempts < weapons.Count)
            {
                nextIndex = (nextIndex + 1) % weapons.Count;
                attempts++;
            }

            SwitchToWeapon(nextIndex);
        }

        /// <summary>
        /// Switch to the previous weapon in the list
        /// </summary>
        public void SwitchToPreviousWeapon()
        {
            if (weapons.Count <= 1) return;

            int prevIndex = currentWeaponIndex - 1;
            if (prevIndex < 0) prevIndex = weapons.Count - 1;

            // Skip null weapons
            int attempts = 0;
            while (weapons[prevIndex] == null && attempts < weapons.Count)
            {
                prevIndex--;
                if (prevIndex < 0) prevIndex = weapons.Count - 1;
                attempts++;
            }

            SwitchToWeapon(prevIndex);
        }

        /// <summary>
        /// Add a weapon to inventory
        /// </summary>
        public void AddWeapon(WeaponBase weapon)
        {
            if (weapon == null) return;
            if (weapons.Contains(weapon)) return;

            weapons.Add(weapon);
            weapon.gameObject.SetActive(false);

            // If this is our first weapon, equip it
            if (weapons.Count == 1)
            {
                SwitchToWeapon(0);
            }
        }

        /// <summary>
        /// Check if player has a specific weapon type
        /// </summary>
        public bool HasWeapon<T>() where T : WeaponBase
        {
            foreach (var weapon in weapons)
            {
                if (weapon is T) return true;
            }
            return false;
        }

        /// <summary>
        /// Add ammo to a specific weapon type
        /// </summary>
        public void AddAmmoToWeapon<T>(int amount) where T : WeaponBase
        {
            foreach (var weapon in weapons)
            {
                if (weapon is T)
                {
                    weapon.AddAmmo(amount);
                    return;
                }
            }
        }

        /// <summary>
        /// Add ammo to current weapon
        /// </summary>
        public void AddAmmoToCurrentWeapon(int amount)
        {
            currentWeapon?.AddAmmo(amount);
        }
    }
}
