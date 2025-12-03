using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace HorribleBosses.Managers
{
    /// <summary>
    /// Handles saving and loading custom boss data to persistent storage.
    /// Bosses are stored as JSON files in the persistent data path.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string customBossFolder = "CustomBosses";
        [SerializeField] private string presetBossFolder = "PresetBosses";

        // Cached bosses
        private List<BossSaveData> customBosses = new List<BossSaveData>();
        private List<BossSaveData> presetBosses = new List<BossSaveData>();

        // Properties
        public string CustomBossPath => Path.Combine(Application.persistentDataPath, customBossFolder);
        public string PresetBossPath => Path.Combine(Application.streamingAssetsPath, presetBossFolder);
        public List<BossSaveData> CustomBosses => customBosses;
        public List<BossSaveData> PresetBosses => presetBosses;
        public List<BossSaveData> AllBosses
        {
            get
            {
                var all = new List<BossSaveData>(presetBosses);
                all.AddRange(customBosses);
                return all;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureDirectoriesExist();
            LoadAllBosses();
        }

        private void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(CustomBossPath))
            {
                Directory.CreateDirectory(CustomBossPath);
            }
        }

        /// <summary>
        /// Load all bosses from disk
        /// </summary>
        public void LoadAllBosses()
        {
            LoadPresetBosses();
            LoadCustomBosses();
        }

        private void LoadPresetBosses()
        {
            presetBosses.Clear();

            // Load built-in presets from StreamingAssets
            if (Directory.Exists(PresetBossPath))
            {
                string[] files = Directory.GetFiles(PresetBossPath, "*.json");
                foreach (string file in files)
                {
                    BossSaveData boss = LoadBossFromFile(file);
                    if (boss != null)
                    {
                        boss.isPreset = true;
                        presetBosses.Add(boss);
                    }
                }
            }

            // If no presets found, create defaults
            if (presetBosses.Count == 0)
            {
                CreateDefaultPresets();
            }

            Debug.Log($"Loaded {presetBosses.Count} preset bosses");
        }

        private void LoadCustomBosses()
        {
            customBosses.Clear();

            if (Directory.Exists(CustomBossPath))
            {
                string[] files = Directory.GetFiles(CustomBossPath, "*.json");
                foreach (string file in files)
                {
                    BossSaveData boss = LoadBossFromFile(file);
                    if (boss != null)
                    {
                        boss.isPreset = false;
                        customBosses.Add(boss);
                    }
                }
            }

            Debug.Log($"Loaded {customBosses.Count} custom bosses");
        }

        private BossSaveData LoadBossFromFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                BossSaveData data = JsonUtility.FromJson<BossSaveData>(json);
                data.filePath = path;
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load boss from {path}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Save a custom boss to disk
        /// </summary>
        public bool SaveCustomBoss(BossSaveData boss)
        {
            if (boss == null) return false;

            // Generate filename from boss name
            string safeName = SanitizeFileName(boss.bossName);
            string fileName = $"{safeName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
            string path = Path.Combine(CustomBossPath, fileName);

            return SaveBossToFile(boss, path);
        }

        /// <summary>
        /// Update an existing custom boss
        /// </summary>
        public bool UpdateCustomBoss(BossSaveData boss)
        {
            if (boss == null || string.IsNullOrEmpty(boss.filePath)) return false;
            if (boss.isPreset) return false; // Can't modify presets

            return SaveBossToFile(boss, boss.filePath);
        }

        private bool SaveBossToFile(BossSaveData boss, string path)
        {
            try
            {
                string json = JsonUtility.ToJson(boss, true);
                File.WriteAllText(path, json);
                boss.filePath = path;

                // Refresh list
                LoadCustomBosses();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save boss to {path}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a custom boss
        /// </summary>
        public bool DeleteCustomBoss(BossSaveData boss)
        {
            if (boss == null || boss.isPreset) return false;
            if (string.IsNullOrEmpty(boss.filePath)) return false;

            try
            {
                if (File.Exists(boss.filePath))
                {
                    File.Delete(boss.filePath);
                }
                customBosses.Remove(boss);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to delete boss: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Convert save data to runtime BossData ScriptableObject
        /// </summary>
        public Boss.BossData ConvertToBossData(BossSaveData saveData)
        {
            Boss.BossData data = ScriptableObject.CreateInstance<Boss.BossData>();

            data.bossName = saveData.bossName;
            data.title = saveData.title;
            data.catchphrase = saveData.catchphrase;

            data.bodyType = (Boss.BodyType)saveData.bodyType;
            data.outfit = (Boss.OutfitType)saveData.outfit;
            data.headType = (Boss.HeadType)saveData.headType;

            data.skinTone = saveData.GetSkinTone();
            data.primaryClothingColor = saveData.GetPrimaryColor();
            data.secondaryClothingColor = saveData.GetSecondaryColor();
            data.hairColor = saveData.GetHairColor();

            data.maxHealth = saveData.maxHealth;
            data.baseDamage = saveData.baseDamage;
            data.moveSpeed = saveData.moveSpeed;
            data.chaseSpeed = saveData.chaseSpeed;

            data.difficulty = (Boss.DifficultyPreset)saveData.difficulty;

            data.canThrowObjects = saveData.canThrowObjects;
            data.canCharge = saveData.canCharge;
            data.canSummonInterns = saveData.canSummonInterns;
            data.hasShield = saveData.hasShield;

            return data;
        }

        /// <summary>
        /// Create default preset bosses (called if no presets found)
        /// </summary>
        private void CreateDefaultPresets()
        {
            presetBosses.Add(new BossSaveData
            {
                bossName = "Mr. Micromanager",
                title = "Assistant Regional Manager",
                catchphrase = "I need that TPS report on my desk by yesterday!",
                bodyType = 1, // Average
                outfit = 0,   // BusinessSuit
                headType = 2, // Combover
                skinToneR = 0.9f, skinToneG = 0.75f, skinToneB = 0.65f,
                primaryColorR = 0.2f, primaryColorG = 0.2f, primaryColorB = 0.3f,
                secondaryColorR = 1f, secondaryColorG = 1f, secondaryColorB = 1f,
                hairColorR = 0.3f, hairColorG = 0.2f, hairColorB = 0.1f,
                maxHealth = 200f,
                baseDamage = 15f,
                moveSpeed = 4f,
                chaseSpeed = 6f,
                difficulty = 1, // Medium
                isPreset = true
            });

            presetBosses.Add(new BossSaveData
            {
                bossName = "Karen from HR",
                title = "Human Resources Director",
                catchphrase = "I'd like to speak to YOUR manager!",
                bodyType = 1,
                outfit = 4, // PowerSuit
                headType = 0,
                skinToneR = 0.95f, skinToneG = 0.8f, skinToneB = 0.7f,
                primaryColorR = 0.8f, primaryColorG = 0.1f, primaryColorB = 0.2f,
                secondaryColorR = 0.1f, secondaryColorG = 0.1f, secondaryColorB = 0.1f,
                hairColorR = 0.9f, hairColorG = 0.85f, hairColorB = 0.5f,
                maxHealth = 180f,
                baseDamage = 20f,
                moveSpeed = 5f,
                chaseSpeed = 7f,
                difficulty = 1,
                canSummonInterns = true,
                isPreset = true
            });

            presetBosses.Add(new BossSaveData
            {
                bossName = "Big Tony",
                title = "CEO",
                catchphrase = "You're not a team player!",
                bodyType = 3, // Huge
                outfit = 0,   // BusinessSuit
                headType = 1, // Bald
                skinToneR = 0.85f, skinToneG = 0.65f, skinToneB = 0.55f,
                primaryColorR = 0.05f, primaryColorG = 0.05f, primaryColorB = 0.1f,
                secondaryColorR = 0.6f, secondaryColorG = 0.5f, secondaryColorB = 0.1f,
                hairColorR = 0f, hairColorG = 0f, hairColorB = 0f,
                maxHealth = 500f,
                baseDamage = 30f,
                moveSpeed = 3f,
                chaseSpeed = 5f,
                difficulty = 3, // Nightmare
                canThrowObjects = true,
                canCharge = true,
                hasShield = true,
                isPreset = true
            });

            presetBosses.Add(new BossSaveData
            {
                bossName = "Chad from Sales",
                title = "Regional Sales Lead",
                catchphrase = "Let's circle back on that, champ!",
                bodyType = 1,
                outfit = 2, // HawaiianShirt
                headType = 3, // Slicked
                skinToneR = 0.95f, skinToneG = 0.7f, skinToneB = 0.5f,
                primaryColorR = 0.2f, primaryColorG = 0.6f, primaryColorB = 0.8f,
                secondaryColorR = 1f, secondaryColorG = 0.9f, secondaryColorB = 0.7f,
                hairColorR = 0.4f, hairColorG = 0.3f, hairColorB = 0.2f,
                maxHealth = 150f,
                baseDamage = 12f,
                moveSpeed = 6f,
                chaseSpeed = 9f,
                difficulty = 0, // Easy
                isPreset = true
            });

            presetBosses.Add(new BossSaveData
            {
                bossName = "The Intern King",
                title = "Senior Intern Coordinator",
                catchphrase = "Interns! Attack!",
                bodyType = 0, // Skinny
                outfit = 5,   // Suspenders
                headType = 4, // Messy
                skinToneR = 0.9f, skinToneG = 0.8f, skinToneB = 0.75f,
                primaryColorR = 0.4f, primaryColorG = 0.3f, primaryColorB = 0.2f,
                secondaryColorR = 0.9f, secondaryColorG = 0.85f, secondaryColorB = 0.7f,
                hairColorR = 0.6f, hairColorG = 0.4f, hairColorB = 0.2f,
                maxHealth = 120f,
                baseDamage = 8f,
                moveSpeed = 4f,
                chaseSpeed = 5f,
                difficulty = 2, // Hard (because of interns)
                canSummonInterns = true,
                isPreset = true
            });

            Debug.Log("Created default preset bosses");
        }

        private string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name.Replace(' ', '_');
        }
    }

    /// <summary>
    /// Serializable boss data for JSON storage
    /// </summary>
    [System.Serializable]
    public class BossSaveData
    {
        // Identity
        public string bossName = "Custom Boss";
        public string title = "Manager";
        public string catchphrase = "You're fired!";

        // Gender & Body (stored as ints for enum serialization)
        public int gender;          // Male, Female, NonBinary
        public int bodyType;        // Slim, Average, Athletic, Stocky, Large
        public float heightScale = 1f;

        // Head & Face
        public int hairStyle;       // Bald, Buzzcut, Short, Combover, etc.
        public int beardStyle;      // None, Stubble, Goatee, FullBeard, etc.
        public int faceShape;       // Oval, Round, Square, Long, Heart
        public int eyeShape;        // Normal, Narrow, Wide, Tired, Angry
        public int noseType;        // Small, Average, Large, Hooked, Upturned

        // Outfit
        public int outfit;          // BusinessSuit, PowerSuit, CasualFriday, etc.
        public int accessory;       // None, Glasses, Sunglasses, Monocle, etc.

        // Colors (stored as floats for JSON)
        public float skinToneR = 0.9f, skinToneG = 0.75f, skinToneB = 0.65f;
        public float eyeColorR = 0.4f, eyeColorG = 0.3f, eyeColorB = 0.2f;
        public float hairColorR, hairColorG, hairColorB;
        public float primaryColorR = 0.3f, primaryColorG = 0.3f, primaryColorB = 0.35f;
        public float secondaryColorR = 1f, secondaryColorG = 1f, secondaryColorB = 1f;
        public float accessoryColorR, accessoryColorG, accessoryColorB;

        // Stats
        public float maxHealth = 200f;
        public float baseDamage = 15f;
        public float moveSpeed = 4f;
        public float chaseSpeed = 6f;
        public int difficulty = 1;

        // Abilities
        public bool canThrowObjects;
        public bool canCharge;
        public bool canSummonInterns;
        public bool hasShield;

        // Meta
        public bool isPreset;
        [System.NonSerialized] public string filePath;

        // Color helpers
        public Color GetSkinTone() => new Color(skinToneR, skinToneG, skinToneB);
        public Color GetEyeColor() => new Color(eyeColorR, eyeColorG, eyeColorB);
        public Color GetHairColor() => new Color(hairColorR, hairColorG, hairColorB);
        public Color GetPrimaryColor() => new Color(primaryColorR, primaryColorG, primaryColorB);
        public Color GetSecondaryColor() => new Color(secondaryColorR, secondaryColorG, secondaryColorB);
        public Color GetAccessoryColor() => new Color(accessoryColorR, accessoryColorG, accessoryColorB);

        public void SetSkinTone(Color c) { skinToneR = c.r; skinToneG = c.g; skinToneB = c.b; }
        public void SetEyeColor(Color c) { eyeColorR = c.r; eyeColorG = c.g; eyeColorB = c.b; }
        public void SetHairColor(Color c) { hairColorR = c.r; hairColorG = c.g; hairColorB = c.b; }
        public void SetPrimaryColor(Color c) { primaryColorR = c.r; primaryColorG = c.g; primaryColorB = c.b; }
        public void SetSecondaryColor(Color c) { secondaryColorR = c.r; secondaryColorG = c.g; secondaryColorB = c.b; }
        public void SetAccessoryColor(Color c) { accessoryColorR = c.r; accessoryColorG = c.g; accessoryColorB = c.b; }
    }
}
