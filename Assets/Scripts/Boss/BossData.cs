using UnityEngine;

namespace HorribleBosses.Boss
{
    /// <summary>
    /// ScriptableObject containing all customizable boss data.
    /// Create presets or save player-created bosses.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBoss", menuName = "Horrible Bosses/Boss Data")]
    public class BossData : ScriptableObject
    {
        [Header("Identity")]
        public string bossName = "Mr. Manager";
        public string title = "Regional Manager"; // Displayed under name

        [TextArea(2, 4)]
        public string catchphrase = "You're fired!"; // Said during combat

        [Header("Appearance")]
        public BodyType bodyType = BodyType.Average;
        public OutfitType outfit = OutfitType.BusinessSuit;
        public HeadType headType = HeadType.Default;

        [Header("Colors")]
        public Color skinTone = new Color(0.9f, 0.75f, 0.65f);
        public Color primaryClothingColor = Color.gray;
        public Color secondaryClothingColor = Color.white;
        public Color hairColor = Color.black;

        [Header("Stats - Base Values")]
        [Range(100f, 1000f)]
        public float maxHealth = 200f;

        [Range(5f, 50f)]
        public float baseDamage = 15f;

        [Range(2f, 10f)]
        public float moveSpeed = 4f;

        [Range(4f, 15f)]
        public float chaseSpeed = 6f;

        [Header("Difficulty Scaling")]
        public DifficultyPreset difficulty = DifficultyPreset.Medium;

        [Header("Behavior")]
        [Range(1f, 5f)]
        public float attackCooldown = 2f;

        [Range(5f, 20f)]
        public float detectionRange = 12f;

        [Range(1f, 4f)]
        public float attackRange = 2f;

        [Range(0.3f, 0.7f)]
        public float rageHealthThreshold = 0.3f; // Enter rage below this HP %

        [Header("Special Abilities")]
        public bool canThrowObjects = false;
        public bool canCharge = false;
        public bool canSummonInterns = false; // Spawns minions
        public bool hasShield = false; // Temporary damage immunity

        [Header("Audio")]
        public AudioClip[] tauntSounds;
        public AudioClip[] hurtSounds;
        public AudioClip[] attackSounds;
        public AudioClip rageSound;
        public AudioClip deathSound;

        /// <summary>
        /// Get stats modified by difficulty
        /// </summary>
        public BossStats GetScaledStats()
        {
            float healthMult = 1f;
            float damageMult = 1f;
            float speedMult = 1f;
            float attackSpeedMult = 1f;

            switch (difficulty)
            {
                case DifficultyPreset.Easy:
                    healthMult = 0.7f;
                    damageMult = 0.6f;
                    speedMult = 0.8f;
                    attackSpeedMult = 0.7f;
                    break;
                case DifficultyPreset.Medium:
                    // Default values
                    break;
                case DifficultyPreset.Hard:
                    healthMult = 1.5f;
                    damageMult = 1.4f;
                    speedMult = 1.2f;
                    attackSpeedMult = 1.3f;
                    break;
                case DifficultyPreset.Nightmare:
                    healthMult = 2.5f;
                    damageMult = 2f;
                    speedMult = 1.4f;
                    attackSpeedMult = 1.6f;
                    break;
            }

            return new BossStats
            {
                maxHealth = maxHealth * healthMult,
                damage = baseDamage * damageMult,
                moveSpeed = moveSpeed * speedMult,
                chaseSpeed = chaseSpeed * speedMult,
                attackCooldown = attackCooldown / attackSpeedMult,
                detectionRange = detectionRange,
                attackRange = attackRange,
                rageThreshold = rageHealthThreshold
            };
        }

        /// <summary>
        /// Create a runtime copy of this data for modification
        /// </summary>
        public BossData CreateRuntimeCopy()
        {
            return Instantiate(this);
        }
    }

    /// <summary>
    /// Calculated stats after difficulty scaling
    /// </summary>
    [System.Serializable]
    public struct BossStats
    {
        public float maxHealth;
        public float damage;
        public float moveSpeed;
        public float chaseSpeed;
        public float attackCooldown;
        public float detectionRange;
        public float attackRange;
        public float rageThreshold;
    }

    public enum BodyType
    {
        Skinny,
        Average,
        Large,
        Huge
    }

    public enum OutfitType
    {
        BusinessSuit,
        Casual,
        HawaiianShirt,
        GolfAttire,
        PowerSuit,
        Suspenders
    }

    public enum HeadType
    {
        Default,
        Bald,
        Combover,
        Slicked,
        Messy,
        Toupee
    }

    public enum DifficultyPreset
    {
        Easy,
        Medium,
        Hard,
        Nightmare
    }
}
