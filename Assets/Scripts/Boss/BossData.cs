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
        public string title = "Regional Manager";

        [TextArea(2, 4)]
        public string catchphrase = "You're fired!";

        [Header("Gender & Body")]
        public Gender gender = Gender.Male;
        public BodyType bodyType = BodyType.Average;

        [Range(0.85f, 1.15f)]
        public float heightScale = 1f; // 0.85 = short, 1.15 = tall

        [Header("Head & Face")]
        public HairStyle hairStyle = HairStyle.Short;
        public BeardStyle beardStyle = BeardStyle.None;
        public FaceShape faceShape = FaceShape.Oval;
        public EyeShape eyeShape = EyeShape.Normal;
        public NoseType noseType = NoseType.Average;

        [Header("Outfit")]
        public OutfitType outfit = OutfitType.BusinessSuit;
        public AccessoryType accessory = AccessoryType.None;

        [Header("Colors")]
        public Color skinTone = new Color(0.9f, 0.75f, 0.65f);
        public Color eyeColor = new Color(0.4f, 0.3f, 0.2f);
        public Color hairColor = Color.black;
        public Color primaryClothingColor = Color.gray;
        public Color secondaryClothingColor = Color.white;
        public Color accessoryColor = Color.black;

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
        public float rageHealthThreshold = 0.3f;

        [Header("Special Abilities")]
        public bool canThrowObjects = false;
        public bool canCharge = false;
        public bool canSummonInterns = false;
        public bool hasShield = false;

        [Header("Audio")]
        public AudioClip[] tauntSounds;
        public AudioClip[] hurtSounds;
        public AudioClip[] attackSounds;
        public AudioClip rageSound;
        public AudioClip deathSound;

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

        public BossData CreateRuntimeCopy()
        {
            return Instantiate(this);
        }
    }

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

    // ===== ENUMS WITH DESCRIPTIVE NAMES =====

    public enum Gender
    {
        Male,
        Female,
        NonBinary
    }

    public enum BodyType
    {
        Slim,           // "The Marathoner"
        Average,        // "Standard Issue"
        Athletic,       // "Gym Bro"
        Stocky,         // "The Tank"
        Large           // "Executive Size"
    }

    public enum HairStyle
    {
        Bald,           // "Chrome Dome"
        Buzzcut,        // "The Drill Sergeant"
        Short,          // "Business Standard"
        Combover,       // "The Denial"
        Slicked,        // "Wall Street"
        Pompadour,      // "The Showoff"
        Messy,          // "Startup Founder"
        Ponytail,       // "Weekend Warrior"
        Toupee,         // "The Cover-Up"
        Mohawk          // "HR Violation"
    }

    public enum BeardStyle
    {
        None,           // "Clean Shaven"
        Stubble,        // "Casual Friday"
        Goatee,         // "The Negotiator"
        FullBeard,      // "The Lumberjack"
        Mustache,       // "Old School"
        Mutton,         // "The Senator"
        SoulPatch       // "The Artist"
    }

    public enum FaceShape
    {
        Oval,           // "Classic"
        Round,          // "Friendly"
        Square,         // "The Jawline"
        Long,           // "Distinguished"
        Heart           // "Trustworthy"
    }

    public enum EyeShape
    {
        Normal,         // "Standard"
        Narrow,         // "The Scrutinizer"
        Wide,           // "The Enthusiast"
        Tired,          // "Overworked"
        Angry           // "Always Mad"
    }

    public enum NoseType
    {
        Small,          // "Button"
        Average,        // "Standard"
        Large,          // "The Sniffer"
        Hooked,         // "Roman"
        Upturned        // "The Snob"
    }

    public enum OutfitType
    {
        BusinessSuit,       // "Corporate Classic"
        PowerSuit,          // "The Executive"
        CasualFriday,       // "Khakis & Polo"
        HawaiianShirt,      // "The Vacation"
        GolfAttire,         // "Country Club"
        Suspenders,         // "Old Fashioned"
        TechBro,            // "Hoodie & Jeans"
        LabCoat,            // "The Scientist"
        SecurityUniform,    // "The Enforcer"
        JanitorCoveralls    // "The Sleeper"
    }

    public enum AccessoryType
    {
        None,
        Glasses,            // "The Intellectual"
        Sunglasses,         // "Too Cool"
        Monocle,            // "Old Money"
        Earpiece,           // "Always Connected"
        GoldChain,          // "The Closer"
        Tie,                // "Professional"
        Bowtie,             // "Quirky"
        Lanyard,            // "Badge of Honor"
        Headset             // "Call Center King"
    }

    public enum DifficultyPreset
    {
        Intern,         // Easy
        Employee,       // Medium
        Manager,        // Hard
        Executive       // Nightmare
    }
}
