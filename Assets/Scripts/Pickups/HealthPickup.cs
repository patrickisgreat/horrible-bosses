using UnityEngine;

namespace HorribleBosses.Pickups
{
    /// <summary>
    /// Health pickup - restores player health.
    /// Office themed: Coffee mug, energy drink, stress ball, etc.
    /// </summary>
    public class HealthPickup : PickupBase
    {
        [Header("Health Settings")]
        [SerializeField] private float healAmount = 25f;
        [SerializeField] private bool percentageHeal = false; // If true, healAmount is percentage

        protected override bool CanCollect(GameObject collector)
        {
            var health = collector.GetComponent<Player.PlayerHealth>();
            if (health == null) health = collector.GetComponentInChildren<Player.PlayerHealth>();

            // Only collect if not at full health
            return health != null && health.CurrentHealth < health.MaxHealth;
        }

        protected override void ApplyPickup(GameObject collector)
        {
            var health = collector.GetComponent<Player.PlayerHealth>();
            if (health == null) health = collector.GetComponentInChildren<Player.PlayerHealth>();

            if (health != null)
            {
                float amount = percentageHeal ? health.MaxHealth * (healAmount / 100f) : healAmount;
                health.Heal(amount);
                Debug.Log($"Picked up health: +{amount} HP (Coffee break!)");
            }
        }
    }
}
