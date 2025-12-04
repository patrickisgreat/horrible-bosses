using UnityEngine;
using UnityEngine.AI;

namespace HorribleBosses.Utilities
{
    /// <summary>
    /// Automatically sets up a test scene with all required components.
    /// Add this to an empty scene and press Play to generate everything.
    /// </summary>
    public class SceneBootstrapper : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private bool generateOnStart = true;
        [SerializeField] private float arenaSize = 30f;
        [SerializeField] private bool addTestBoss = true;

        [Header("References (Optional - will create if missing)")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject bossPrefab;

        [Header("Generated Objects")]
        [SerializeField] private bool hasGenerated;

        private void Start()
        {
            if (generateOnStart && !hasGenerated)
            {
                GenerateTestScene();
            }
        }

        [ContextMenu("Generate Test Scene")]
        public void GenerateTestScene()
        {
            Debug.Log("=== Generating Horrible Bosses Test Scene ===");

            CreateArena();
            CreateLighting();
            CreatePlayer();
            CreateManagers();
            CreateUI();

            if (addTestBoss)
            {
                CreateTestBoss();
            }

            CreatePickups();
            BakeNavMesh();

            hasGenerated = true;
            Debug.Log("=== Test Scene Generation Complete! ===");
            Debug.Log("Press PLAY to test. Use ` for debug menu, F1-F6 for cheats.");
        }

        private void CreateArena()
        {
            Debug.Log("Creating arena...");

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(arenaSize, 0.5f, arenaSize);
            floor.isStatic = true;
            floor.layer = LayerMask.NameToLayer("Default");

            // Add NavMeshSurface marker
            var navModifier = floor.AddComponent<NavMeshModifier>();
            navModifier.overrideArea = true;
            navModifier.area = 0; // Walkable

            // Walls
            CreateWall("Wall_North", new Vector3(0, 2.5f, arenaSize / 2), new Vector3(arenaSize, 5f, 1f));
            CreateWall("Wall_South", new Vector3(0, 2.5f, -arenaSize / 2), new Vector3(arenaSize, 5f, 1f));
            CreateWall("Wall_East", new Vector3(arenaSize / 2, 2.5f, 0), new Vector3(1f, 5f, arenaSize));
            CreateWall("Wall_West", new Vector3(-arenaSize / 2, 2.5f, 0), new Vector3(1f, 5f, arenaSize));

            // Some obstacles (desks/cover)
            CreateObstacle("Desk1", new Vector3(5, 0.5f, 5), new Vector3(2f, 1f, 1f));
            CreateObstacle("Desk2", new Vector3(-5, 0.5f, -5), new Vector3(2f, 1f, 1f));
            CreateObstacle("Desk3", new Vector3(-8, 0.5f, 3), new Vector3(1f, 1f, 2f));
            CreateObstacle("Desk4", new Vector3(8, 0.5f, -3), new Vector3(1f, 1f, 2f));

            // Pillars
            CreateObstacle("Pillar1", new Vector3(8, 2f, 8), new Vector3(1f, 4f, 1f));
            CreateObstacle("Pillar2", new Vector3(-8, 2f, -8), new Vector3(1f, 4f, 1f));
            CreateObstacle("Pillar3", new Vector3(-8, 2f, 8), new Vector3(1f, 4f, 1f));
            CreateObstacle("Pillar4", new Vector3(8, 2f, -8), new Vector3(1f, 4f, 1f));
        }

        private void CreateWall(string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.isStatic = true;
        }

        private void CreateObstacle(string name, Vector3 position, Vector3 scale)
        {
            GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obs.name = name;
            obs.transform.position = position;
            obs.transform.localScale = scale;
            obs.isStatic = true;

            // Mark as NavMesh obstacle
            var navObs = obs.AddComponent<NavMeshObstacle>();
            navObs.carving = true;
        }

        private void CreateLighting()
        {
            Debug.Log("Creating lighting...");

            // Main directional light
            GameObject lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.9f);
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private void CreatePlayer()
        {
            Debug.Log("Creating player...");

            // Main player object
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");
            player.transform.position = new Vector3(0, 1f, -10f);

            // CharacterController
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0, 1f, 0);

            // Player scripts
            player.AddComponent<Player.PlayerController>();
            var playerHealth = player.AddComponent<Player.PlayerHealth>();

            // Ground check
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(player.transform);
            groundCheck.transform.localPosition = Vector3.zero;

            // Camera holder
            GameObject cameraHolder = new GameObject("CameraHolder");
            cameraHolder.transform.SetParent(player.transform);
            cameraHolder.transform.localPosition = new Vector3(0, 1.6f, 0);

            // Camera
            Camera.main?.gameObject.SetActive(false); // Disable default camera
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(cameraHolder.transform);
            camObj.transform.localPosition = Vector3.zero;
            var cam = camObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            camObj.AddComponent<AudioListener>();
            var playerCam = camObj.AddComponent<Player.PlayerCamera>();

            // Weapon holder
            GameObject weaponHolder = new GameObject("WeaponHolder");
            weaponHolder.transform.SetParent(camObj.transform);
            weaponHolder.transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
            var weaponManager = weaponHolder.AddComponent<Weapons.WeaponManager>();

            // Create weapons
            CreateWeapon<Weapons.Pistol>(weaponHolder.transform, "Pistol", 0);
            CreateWeapon<Weapons.Shotgun>(weaponHolder.transform, "Shotgun", 1);
            CreateWeapon<Weapons.Chainsaw>(weaponHolder.transform, "Chainsaw", 2);
            CreateWeapon<Weapons.Flamethrower>(weaponHolder.transform, "Flamethrower", 3);
        }

        private void CreateWeapon<T>(Transform parent, string name, int slot) where T : Weapons.WeaponBase
        {
            GameObject weaponObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weaponObj.name = name;
            weaponObj.transform.SetParent(parent);
            weaponObj.transform.localPosition = Vector3.zero;
            weaponObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.3f);
            Destroy(weaponObj.GetComponent<Collider>());

            var weapon = weaponObj.AddComponent<T>();
            weaponObj.SetActive(false);
        }

        private void CreateManagers()
        {
            Debug.Log("Creating managers...");

            GameObject managers = new GameObject("--- MANAGERS ---");

            // GameManager
            GameObject gmObj = new GameObject("GameManager");
            gmObj.transform.SetParent(managers.transform);
            gmObj.AddComponent<Managers.GameManager>();

            // AudioManager
            GameObject amObj = new GameObject("AudioManager");
            amObj.transform.SetParent(managers.transform);
            amObj.AddComponent<Managers.AudioManager>();

            // SaveManager
            GameObject smObj = new GameObject("SaveManager");
            smObj.transform.SetParent(managers.transform);
            smObj.AddComponent<Managers.SaveManager>();

            // DebugManager
            GameObject dmObj = new GameObject("DebugManager");
            dmObj.transform.SetParent(managers.transform);
            dmObj.AddComponent<DebugManager>();

            // DamageNumbers
            GameObject dnObj = new GameObject("DamageNumbers");
            dnObj.transform.SetParent(managers.transform);
            dnObj.AddComponent<Combat.DamageNumbers>();
        }

        private void CreateUI()
        {
            Debug.Log("Creating UI...");

            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // HUD placeholder
            GameObject hud = new GameObject("HUD");
            hud.transform.SetParent(canvasObj.transform);
            hud.AddComponent<UI.HUDManager>();
        }

        private void CreateTestBoss()
        {
            Debug.Log("Creating test boss...");

            GameObject boss = new GameObject("TestBoss");
            boss.transform.position = new Vector3(0, 0, 10f);

            // Visual placeholder
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "BossBody";
            visual.transform.SetParent(boss.transform);
            visual.transform.localPosition = new Vector3(0, 1f, 0);
            visual.transform.localScale = new Vector3(1f, 1f, 1f);
            Destroy(visual.GetComponent<Collider>());

            // Collider on parent
            var capsule = boss.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = new Vector3(0, 1f, 0);

            // NavMeshAgent
            var agent = boss.AddComponent<NavMeshAgent>();
            agent.speed = 4f;
            agent.angularSpeed = 360f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 2f;
            agent.radius = 0.5f;
            agent.height = 2f;

            // Boss scripts
            boss.AddComponent<Boss.BossController>();
            boss.AddComponent<Boss.BossHealth>();
            boss.AddComponent<Boss.BossAI>();
            boss.AddComponent<Boss.BossCustomization>();

            // Create and assign boss data
            var bossData = ScriptableObject.CreateInstance<Boss.BossData>();
            bossData.bossName = "Test Boss";
            bossData.title = "Debug Manager";
            bossData.catchphrase = "I'm just here for testing!";
            bossData.difficulty = Boss.DifficultyPreset.Employee;

            var controller = boss.GetComponent<Boss.BossController>();
            // BossData will be assigned via Inspector in real usage
        }

        private void CreatePickups()
        {
            Debug.Log("Creating pickups...");

            CreatePickup<Pickups.HealthPickup>("HealthPickup", new Vector3(10, 0.5f, 0));
            CreatePickup<Pickups.HealthPickup>("HealthPickup", new Vector3(-10, 0.5f, 0));
            CreatePickup<Pickups.AmmoPickup>("AmmoPickup", new Vector3(0, 0.5f, 10));
            CreatePickup<Pickups.AmmoPickup>("AmmoPickup", new Vector3(0, 0.5f, -10));
            CreatePickup<Pickups.FuelPickup>("FuelPickup", new Vector3(5, 0.5f, -5));
        }

        private void CreatePickup<T>(string name, Vector3 position) where T : Pickups.PickupBase
        {
            GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pickup.name = name;
            pickup.transform.position = position;
            pickup.transform.localScale = Vector3.one * 0.5f;

            var collider = pickup.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 1f;

            pickup.AddComponent<T>();
        }

        private void BakeNavMesh()
        {
            Debug.Log("Note: NavMesh needs to be baked manually in Editor.");
            Debug.Log("Go to Window > AI > Navigation > Bake");
        }

        [ContextMenu("Clear Generated Scene")]
        public void ClearScene()
        {
            // Find and destroy generated objects
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj != gameObject && obj.transform.parent == null)
                {
                    DestroyImmediate(obj);
                }
            }
            hasGenerated = false;
            Debug.Log("Scene cleared!");
        }
    }
}
