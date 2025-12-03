using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HorribleBosses.Boss;
using HorribleBosses.Managers;

namespace HorribleBosses.UI
{
    /// <summary>
    /// Full boss creator/editor UI with live preview.
    /// Allows players to create and save custom bosses.
    /// </summary>
    public class BossCreatorUI : MonoBehaviour
    {
        [Header("Preview")]
        [SerializeField] private Transform previewSpawnPoint;
        [SerializeField] private GameObject bossPreviewPrefab;
        [SerializeField] private float previewRotateSpeed = 30f;

        [Header("Identity Panel")]
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField titleInput;
        [SerializeField] private TMP_InputField catchphraseInput;

        [Header("Body Panel")]
        [SerializeField] private TMP_Dropdown genderDropdown;
        [SerializeField] private TMP_Dropdown bodyTypeDropdown;
        [SerializeField] private Slider heightSlider;
        [SerializeField] private TMP_Text heightLabel;

        [Header("Face Panel")]
        [SerializeField] private TMP_Dropdown hairStyleDropdown;
        [SerializeField] private TMP_Dropdown beardStyleDropdown;
        [SerializeField] private TMP_Dropdown faceShapeDropdown;
        [SerializeField] private TMP_Dropdown eyeShapeDropdown;
        [SerializeField] private TMP_Dropdown noseTypeDropdown;

        [Header("Outfit Panel")]
        [SerializeField] private TMP_Dropdown outfitDropdown;
        [SerializeField] private TMP_Dropdown accessoryDropdown;

        [Header("Colors Panel")]
        [SerializeField] private ColorPickerUI skinColorPicker;
        [SerializeField] private ColorPickerUI eyeColorPicker;
        [SerializeField] private ColorPickerUI hairColorPicker;
        [SerializeField] private ColorPickerUI primaryColorPicker;
        [SerializeField] private ColorPickerUI secondaryColorPicker;

        [Header("Stats Panel")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider damageSlider;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private TMP_Dropdown difficultyDropdown;
        [SerializeField] private TMP_Text healthLabel;
        [SerializeField] private TMP_Text damageLabel;
        [SerializeField] private TMP_Text speedLabel;

        [Header("Abilities Panel")]
        [SerializeField] private Toggle throwObjectsToggle;
        [SerializeField] private Toggle chargeToggle;
        [SerializeField] private Toggle summonInternsToggle;
        [SerializeField] private Toggle shieldToggle;

        [Header("Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button testFightButton;
        [SerializeField] private Button randomizeButton;
        [SerializeField] private Button backButton;

        [Header("Tab Navigation")]
        [SerializeField] private Button[] tabButtons;
        [SerializeField] private GameObject[] tabPanels;

        // Current boss being edited
        private BossSaveData currentBoss;
        private GameObject previewInstance;
        private BossCustomization previewCustomization;

        // Display names for enums
        private readonly string[] bodyTypeNames = { "Slim", "Average", "Athletic", "Stocky", "Large" };
        private readonly string[] hairStyleNames = { "Bald", "Buzzcut", "Short", "Combover", "Slicked", "Pompadour", "Messy", "Ponytail", "Toupee", "Mohawk" };
        private readonly string[] beardStyleNames = { "None", "Stubble", "Goatee", "Full Beard", "Mustache", "Mutton Chops", "Soul Patch" };
        private readonly string[] faceShapeNames = { "Oval", "Round", "Square", "Long", "Heart" };
        private readonly string[] eyeShapeNames = { "Normal", "Narrow", "Wide", "Tired", "Angry" };
        private readonly string[] noseTypeNames = { "Small", "Average", "Large", "Hooked", "Upturned" };
        private readonly string[] outfitNames = { "Business Suit", "Power Suit", "Casual Friday", "Hawaiian Shirt", "Golf Attire", "Suspenders", "Tech Bro", "Lab Coat", "Security", "Janitor" };
        private readonly string[] accessoryNames = { "None", "Glasses", "Sunglasses", "Monocle", "Earpiece", "Gold Chain", "Tie", "Bowtie", "Lanyard", "Headset" };
        private readonly string[] difficultyNames = { "Intern (Easy)", "Employee (Medium)", "Manager (Hard)", "Executive (Nightmare)" };

        private void Start()
        {
            SetupDropdowns();
            SetupListeners();
            SetupTabs();

            // Start with new boss or load existing
            CreateNewBoss();
        }

        private void Update()
        {
            // Rotate preview
            if (previewInstance != null)
            {
                previewInstance.transform.Rotate(Vector3.up, previewRotateSpeed * Time.deltaTime);
            }
        }

        private void SetupDropdowns()
        {
            SetupDropdown(genderDropdown, new[] { "Male", "Female", "Non-Binary" });
            SetupDropdown(bodyTypeDropdown, bodyTypeNames);
            SetupDropdown(hairStyleDropdown, hairStyleNames);
            SetupDropdown(beardStyleDropdown, beardStyleNames);
            SetupDropdown(faceShapeDropdown, faceShapeNames);
            SetupDropdown(eyeShapeDropdown, eyeShapeNames);
            SetupDropdown(noseTypeDropdown, noseTypeNames);
            SetupDropdown(outfitDropdown, outfitNames);
            SetupDropdown(accessoryDropdown, accessoryNames);
            SetupDropdown(difficultyDropdown, difficultyNames);
        }

        private void SetupDropdown(TMP_Dropdown dropdown, string[] options)
        {
            if (dropdown == null) return;
            dropdown.ClearOptions();
            dropdown.AddOptions(new System.Collections.Generic.List<string>(options));
        }

        private void SetupListeners()
        {
            // Identity
            nameInput?.onValueChanged.AddListener(v => { currentBoss.bossName = v; });
            titleInput?.onValueChanged.AddListener(v => { currentBoss.title = v; });
            catchphraseInput?.onValueChanged.AddListener(v => { currentBoss.catchphrase = v; });

            // Body
            genderDropdown?.onValueChanged.AddListener(v => { currentBoss.gender = v; UpdatePreview(); });
            bodyTypeDropdown?.onValueChanged.AddListener(v => { currentBoss.bodyType = v; UpdatePreview(); });
            heightSlider?.onValueChanged.AddListener(v => { currentBoss.heightScale = v; UpdateHeightLabel(); UpdatePreview(); });

            // Face
            hairStyleDropdown?.onValueChanged.AddListener(v => { currentBoss.hairStyle = v; UpdatePreview(); });
            beardStyleDropdown?.onValueChanged.AddListener(v => { currentBoss.beardStyle = v; UpdatePreview(); });
            faceShapeDropdown?.onValueChanged.AddListener(v => { currentBoss.faceShape = v; UpdatePreview(); });
            eyeShapeDropdown?.onValueChanged.AddListener(v => { currentBoss.eyeShape = v; UpdatePreview(); });
            noseTypeDropdown?.onValueChanged.AddListener(v => { currentBoss.noseType = v; UpdatePreview(); });

            // Outfit
            outfitDropdown?.onValueChanged.AddListener(v => { currentBoss.outfit = v; UpdatePreview(); });
            accessoryDropdown?.onValueChanged.AddListener(v => { currentBoss.accessory = v; UpdatePreview(); });

            // Stats
            healthSlider?.onValueChanged.AddListener(v => { currentBoss.maxHealth = v; UpdateStatLabels(); });
            damageSlider?.onValueChanged.AddListener(v => { currentBoss.baseDamage = v; UpdateStatLabels(); });
            speedSlider?.onValueChanged.AddListener(v => { currentBoss.moveSpeed = v; currentBoss.chaseSpeed = v * 1.5f; UpdateStatLabels(); });
            difficultyDropdown?.onValueChanged.AddListener(v => { currentBoss.difficulty = v; });

            // Abilities
            throwObjectsToggle?.onValueChanged.AddListener(v => currentBoss.canThrowObjects = v);
            chargeToggle?.onValueChanged.AddListener(v => currentBoss.canCharge = v);
            summonInternsToggle?.onValueChanged.AddListener(v => currentBoss.canSummonInterns = v);
            shieldToggle?.onValueChanged.AddListener(v => currentBoss.hasShield = v);

            // Buttons
            saveButton?.onClick.AddListener(SaveBoss);
            testFightButton?.onClick.AddListener(TestFight);
            randomizeButton?.onClick.AddListener(RandomizeBoss);
            backButton?.onClick.AddListener(GoBack);
        }

        private void SetupTabs()
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int index = i;
                tabButtons[i]?.onClick.AddListener(() => SelectTab(index));
            }
            SelectTab(0);
        }

        private void SelectTab(int index)
        {
            for (int i = 0; i < tabPanels.Length; i++)
            {
                if (tabPanels[i] != null)
                    tabPanels[i].SetActive(i == index);
            }
        }

        public void CreateNewBoss()
        {
            currentBoss = new BossSaveData
            {
                bossName = "Custom Boss",
                title = "Manager",
                catchphrase = "You're fired!",
                maxHealth = 200f,
                baseDamage = 15f,
                moveSpeed = 4f,
                chaseSpeed = 6f,
                difficulty = 1
            };

            LoadBossToUI();
            SpawnPreview();
        }

        public void LoadBoss(BossSaveData boss)
        {
            currentBoss = boss;
            LoadBossToUI();
            SpawnPreview();
        }

        private void LoadBossToUI()
        {
            if (currentBoss == null) return;

            // Identity
            if (nameInput != null) nameInput.text = currentBoss.bossName;
            if (titleInput != null) titleInput.text = currentBoss.title;
            if (catchphraseInput != null) catchphraseInput.text = currentBoss.catchphrase;

            // Body
            if (genderDropdown != null) genderDropdown.value = currentBoss.gender;
            if (bodyTypeDropdown != null) bodyTypeDropdown.value = currentBoss.bodyType;
            if (heightSlider != null) heightSlider.value = currentBoss.heightScale;

            // Face
            if (hairStyleDropdown != null) hairStyleDropdown.value = currentBoss.hairStyle;
            if (beardStyleDropdown != null) beardStyleDropdown.value = currentBoss.beardStyle;
            if (faceShapeDropdown != null) faceShapeDropdown.value = currentBoss.faceShape;
            if (eyeShapeDropdown != null) eyeShapeDropdown.value = currentBoss.eyeShape;
            if (noseTypeDropdown != null) noseTypeDropdown.value = currentBoss.noseType;

            // Outfit
            if (outfitDropdown != null) outfitDropdown.value = currentBoss.outfit;
            if (accessoryDropdown != null) accessoryDropdown.value = currentBoss.accessory;

            // Colors
            skinColorPicker?.SetColor(currentBoss.GetSkinTone());
            eyeColorPicker?.SetColor(currentBoss.GetEyeColor());
            hairColorPicker?.SetColor(currentBoss.GetHairColor());
            primaryColorPicker?.SetColor(currentBoss.GetPrimaryColor());
            secondaryColorPicker?.SetColor(currentBoss.GetSecondaryColor());

            // Stats
            if (healthSlider != null) healthSlider.value = currentBoss.maxHealth;
            if (damageSlider != null) damageSlider.value = currentBoss.baseDamage;
            if (speedSlider != null) speedSlider.value = currentBoss.moveSpeed;
            if (difficultyDropdown != null) difficultyDropdown.value = currentBoss.difficulty;

            // Abilities
            if (throwObjectsToggle != null) throwObjectsToggle.isOn = currentBoss.canThrowObjects;
            if (chargeToggle != null) chargeToggle.isOn = currentBoss.canCharge;
            if (summonInternsToggle != null) summonInternsToggle.isOn = currentBoss.canSummonInterns;
            if (shieldToggle != null) shieldToggle.isOn = currentBoss.hasShield;

            UpdateHeightLabel();
            UpdateStatLabels();
        }

        private void UpdateHeightLabel()
        {
            if (heightLabel != null && currentBoss != null)
            {
                string desc = currentBoss.heightScale < 0.95f ? "Short" :
                              currentBoss.heightScale > 1.05f ? "Tall" : "Average";
                heightLabel.text = $"Height: {desc} ({currentBoss.heightScale:F2}x)";
            }
        }

        private void UpdateStatLabels()
        {
            if (healthLabel != null) healthLabel.text = $"Health: {currentBoss.maxHealth:F0}";
            if (damageLabel != null) damageLabel.text = $"Damage: {currentBoss.baseDamage:F0}";
            if (speedLabel != null) speedLabel.text = $"Speed: {currentBoss.moveSpeed:F1}";
        }

        private void SpawnPreview()
        {
            if (previewInstance != null)
                Destroy(previewInstance);

            if (bossPreviewPrefab != null && previewSpawnPoint != null)
            {
                previewInstance = Instantiate(bossPreviewPrefab, previewSpawnPoint.position, previewSpawnPoint.rotation);
                previewCustomization = previewInstance.GetComponent<BossCustomization>();
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            if (previewCustomization == null || currentBoss == null) return;

            // Convert save data and apply to preview
            BossData data = SaveManager.Instance?.ConvertToBossData(currentBoss);
            if (data != null)
            {
                previewCustomization.ApplyCustomization(data);
            }
        }

        private void SaveBoss()
        {
            if (SaveManager.Instance == null) return;

            if (currentBoss.isPreset)
            {
                // Create a copy of preset
                currentBoss.isPreset = false;
                currentBoss.filePath = null;
            }

            bool success = string.IsNullOrEmpty(currentBoss.filePath)
                ? SaveManager.Instance.SaveCustomBoss(currentBoss)
                : SaveManager.Instance.UpdateCustomBoss(currentBoss);

            if (success)
            {
                Debug.Log($"Boss '{currentBoss.bossName}' saved successfully!");
            }
        }

        private void TestFight()
        {
            if (GameManager.Instance != null && currentBoss != null)
            {
                BossData data = SaveManager.Instance?.ConvertToBossData(currentBoss);
                if (data != null)
                {
                    GameManager.Instance.StartGameWithBoss(data);
                }
            }
        }

        private void RandomizeBoss()
        {
            currentBoss = new BossSaveData
            {
                bossName = GetRandomBossName(),
                title = GetRandomTitle(),
                catchphrase = GetRandomCatchphrase(),
                gender = Random.Range(0, 3),
                bodyType = Random.Range(0, 5),
                heightScale = Random.Range(0.85f, 1.15f),
                hairStyle = Random.Range(0, 10),
                beardStyle = Random.Range(0, 7),
                faceShape = Random.Range(0, 5),
                eyeShape = Random.Range(0, 5),
                noseType = Random.Range(0, 5),
                outfit = Random.Range(0, 10),
                accessory = Random.Range(0, 10),
                maxHealth = Random.Range(100f, 500f),
                baseDamage = Random.Range(10f, 30f),
                moveSpeed = Random.Range(3f, 7f),
                chaseSpeed = Random.Range(5f, 10f),
                difficulty = Random.Range(0, 4),
                canThrowObjects = Random.value > 0.7f,
                canCharge = Random.value > 0.7f,
                canSummonInterns = Random.value > 0.8f,
                hasShield = Random.value > 0.8f
            };

            // Random colors
            currentBoss.SetSkinTone(Random.ColorHSV(0.05f, 0.1f, 0.3f, 0.8f, 0.5f, 0.9f));
            currentBoss.SetEyeColor(Random.ColorHSV(0f, 1f, 0.3f, 0.7f, 0.3f, 0.7f));
            currentBoss.SetHairColor(Random.ColorHSV(0f, 0.15f, 0f, 0.8f, 0.1f, 0.6f));
            currentBoss.SetPrimaryColor(Random.ColorHSV(0f, 1f, 0.2f, 0.8f, 0.2f, 0.8f));
            currentBoss.SetSecondaryColor(Random.ColorHSV(0f, 1f, 0.2f, 0.8f, 0.2f, 0.8f));

            LoadBossToUI();
            UpdatePreview();
        }

        private string GetRandomBossName()
        {
            string[] firstNames = { "Bob", "Karen", "Chad", "Linda", "Steve", "Brenda", "Mike", "Susan", "Dave", "Patricia", "Tony", "Janet" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Wilson", "Anderson", "Taylor" };
            return $"{firstNames[Random.Range(0, firstNames.Length)]} {lastNames[Random.Range(0, lastNames.Length)]}";
        }

        private string GetRandomTitle()
        {
            string[] titles = { "Regional Manager", "VP of Synergy", "Chief Disruption Officer", "Senior VP", "Director of Operations", "Head of HR", "Sales Lead", "Team Lead", "Department Head", "Executive Coordinator" };
            return titles[Random.Range(0, titles.Length)];
        }

        private string GetRandomCatchphrase()
        {
            string[] phrases = { "You're fired!", "See me in my office!", "That's going in your file!", "Not a team player!", "Let's circle back on that!", "Per my last email...", "We need to synergize!", "Think outside the box!", "It's not personal, it's business!", "I need that by EOD!" };
            return phrases[Random.Range(0, phrases.Length)];
        }

        private void GoBack()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Simple color picker helper component
    /// </summary>
    public class ColorPickerUI : MonoBehaviour
    {
        [SerializeField] private Slider hueSlider;
        [SerializeField] private Slider satSlider;
        [SerializeField] private Slider valSlider;
        [SerializeField] private Image previewImage;

        public System.Action<Color> OnColorChanged;

        private void Start()
        {
            hueSlider?.onValueChanged.AddListener(_ => UpdateColor());
            satSlider?.onValueChanged.AddListener(_ => UpdateColor());
            valSlider?.onValueChanged.AddListener(_ => UpdateColor());
        }

        public void SetColor(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            if (hueSlider != null) hueSlider.value = h;
            if (satSlider != null) satSlider.value = s;
            if (valSlider != null) valSlider.value = v;
            UpdatePreview();
        }

        private void UpdateColor()
        {
            Color color = GetColor();
            UpdatePreview();
            OnColorChanged?.Invoke(color);
        }

        private void UpdatePreview()
        {
            if (previewImage != null)
                previewImage.color = GetColor();
        }

        public Color GetColor()
        {
            float h = hueSlider != null ? hueSlider.value : 0;
            float s = satSlider != null ? satSlider.value : 0;
            float v = valSlider != null ? valSlider.value : 1;
            return Color.HSVToRGB(h, s, v);
        }
    }
}
