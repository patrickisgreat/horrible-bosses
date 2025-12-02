using UnityEngine;

namespace HorribleBosses.Weapons
{
    /// <summary>
    /// Standard semi-automatic pistol. Reliable, accurate, unlimited reserve ammo.
    /// The trusty sidearm for dealing with middle management.
    /// </summary>
    public class Pistol : WeaponBase
    {
        [Header("Pistol Settings")]
        [SerializeField] private float accuracy = 0.5f; // Spread angle in degrees
        [SerializeField] private GameObject bulletTrailPrefab;
        [SerializeField] private float trailDuration = 0.05f;

        protected override void Awake()
        {
            base.Awake();

            // Set pistol defaults
            weaponName = "Office Pistol";
            weaponSlot = 0;
            damage = 15f;
            fireRate = 0.25f;
            isAutomatic = false;
            magazineSize = 12;
            reserveAmmo = -1; // Unlimited reserves for the trusty pistol
            reloadTime = 1.2f;
            range = 100f;
            recoilAmount = new Vector2(2f, 0.5f);
            damageType = Combat.DamageType.Normal;
        }

        protected override void OnFire()
        {
            // Perform hitscan
            RaycastHit? hit = DoHitscan(accuracy);

            // Visual bullet trail
            if (bulletTrailPrefab != null && muzzlePoint != null)
            {
                Vector3 endPoint;
                if (hit.HasValue)
                {
                    endPoint = hit.Value.point;
                }
                else
                {
                    endPoint = (playerCamera != null ? playerCamera.transform : transform).position
                        + (playerCamera != null ? playerCamera.transform : transform).forward * range;
                }

                CreateBulletTrail(muzzlePoint.position, endPoint);
            }
        }

        private void CreateBulletTrail(Vector3 start, Vector3 end)
        {
            if (bulletTrailPrefab == null) return;

            GameObject trail = Instantiate(bulletTrailPrefab);
            LineRenderer line = trail.GetComponent<LineRenderer>();

            if (line != null)
            {
                line.SetPosition(0, start);
                line.SetPosition(1, end);
            }

            Destroy(trail, trailDuration);
        }

        protected override void OnEquip()
        {
            // Could play equip animation here
            Debug.Log("Equipped: Office Pistol - Time to file some complaints.");
        }
    }
}
