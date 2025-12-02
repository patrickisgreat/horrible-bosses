using UnityEngine;

namespace HorribleBosses.Combat
{
    /// <summary>
    /// Interface for any object that can receive damage.
    /// Implement this on Player, Boss, destructible objects, etc.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Current health of the object
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// Maximum health of the object
        /// </summary>
        float MaxHealth { get; }

        /// <summary>
        /// Whether the object is dead/destroyed
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Apply damage to this object
        /// </summary>
        /// <param name="damage">Amount of damage</param>
        /// <param name="damageSource">World position of damage source (for directional effects)</param>
        /// <param name="damageType">Type of damage for resistance calculations</param>
        void TakeDamage(float damage, Vector3 damageSource = default, DamageType damageType = DamageType.Normal);
    }

    /// <summary>
    /// Types of damage for resistance/weakness calculations
    /// </summary>
    public enum DamageType
    {
        Normal,     // Standard bullet damage
        Fire,       // Flamethrower, burning
        Explosive,  // Explosions, grenades
        Melee,      // Chainsaw, punches
        Electric    // Future: taser, electric traps
    }
}
