using UnityEngine;
using UnityEngine.Events;

namespace HorribleBosses.Boss
{
    /// <summary>
    /// Main boss controller that ties together all boss systems.
    /// Acts as the central hub for boss behavior.
    /// </summary>
    public class BossController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private BossData bossData;

        [Header("Components")]
        [SerializeField] private BossHealth bossHealth;
        [SerializeField] private BossAI bossAI;
        [SerializeField] private BossCustomization customization;

        [Header("UI")]
        [SerializeField] private Transform nameTagAnchor;
        [SerializeField] private float nameTagHeight = 2.5f;

        [Header("Taunts")]
        [SerializeField] private float tauntCooldown = 8f;
        [SerializeField] private float tauntChance = 0.3f;

        [Header("Events")]
        public UnityEvent<BossController> OnBossSpawned;
        public UnityEvent<BossController> OnBossDefeated;

        // Runtime data
        private BossStats stats;
        private float lastTauntTime;
        private AudioSource audioSource;

        // Properties
        public BossData Data => bossData;
        public BossStats Stats => stats;
        public BossHealth Health => bossHealth;
        public BossAI AI => bossAI;
        public string BossName => bossData != null ? bossData.bossName : "Boss";
        public string BossTitle => bossData != null ? bossData.title : "";
        public bool IsDefeated => bossHealth != null && bossHealth.IsDead;

        private void Awake()
        {
            if (bossHealth == null) bossHealth = GetComponent<BossHealth>();
            if (bossAI == null) bossAI = GetComponent<BossAI>();
            if (customization == null) customization = GetComponent<BossCustomization>();

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (nameTagAnchor == null)
            {
                GameObject anchor = new GameObject("NameTagAnchor");
                anchor.transform.SetParent(transform);
                anchor.transform.localPosition = Vector3.up * nameTagHeight;
                nameTagAnchor = anchor.transform;
            }
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (bossData == null)
            {
                Debug.LogWarning("BossController has no BossData assigned!");
                return;
            }

            stats = bossData.GetScaledStats();

            if (bossHealth != null)
            {
                bossHealth.Initialize(stats);
                bossHealth.OnDeath.AddListener(HandleDeath);
                bossHealth.OnRageTriggered.AddListener(HandleRage);
                bossHealth.OnDamageTaken.AddListener(HandleDamageTaken);
            }

            if (bossAI != null)
            {
                bossAI.Initialize(stats);
            }

            if (customization != null)
            {
                customization.ApplyCustomization(bossData);
            }

            OnBossSpawned?.Invoke(this);
            Debug.Log($"Boss initialized: {bossData.bossName} ({bossData.difficulty})");
        }

        public void Initialize(BossData data)
        {
            bossData = data;
            Initialize();
        }

        private void Update()
        {
            if (bossAI != null && bossAI.IsInCombat && !IsDefeated)
            {
                TryTaunt();
            }
            UpdateNameTagRotation();
        }

        private void TryTaunt()
        {
            if (Time.time - lastTauntTime < tauntCooldown) return;
            if (Random.value > tauntChance) return;
            PlayTaunt();
            lastTauntTime = Time.time;
        }

        private void PlayTaunt()
        {
            if (bossData == null || bossData.tauntSounds == null || bossData.tauntSounds.Length == 0)
                return;

            AudioClip taunt = bossData.tauntSounds[Random.Range(0, bossData.tauntSounds.Length)];
            if (taunt != null && audioSource != null)
            {
                audioSource.PlayOneShot(taunt);
            }
            Debug.Log($"{bossData.bossName}: \"{bossData.catchphrase}\"");
        }

        private void HandleDamageTaken(float damage)
        {
            if (bossData != null && bossData.hurtSounds != null && bossData.hurtSounds.Length > 0)
            {
                AudioClip hurt = bossData.hurtSounds[Random.Range(0, bossData.hurtSounds.Length)];
                if (hurt != null && audioSource != null)
                {
                    audioSource.PlayOneShot(hurt);
                }
            }
        }

        private void HandleRage()
        {
            if (bossData != null && bossData.rageSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(bossData.rageSound);
            }
            Debug.Log($"{BossName} is ENRAGED! \"{bossData.catchphrase}\"");
        }

        private void HandleDeath()
        {
            if (bossData != null && bossData.deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(bossData.deathSound);
            }
            OnBossDefeated?.Invoke(this);
            Debug.Log($"{BossName} has been defeated! Time to update your resume.");
        }

        private void UpdateNameTagRotation()
        {
            if (nameTagAnchor == null) return;
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                nameTagAnchor.LookAt(mainCam.transform);
                nameTagAnchor.Rotate(0, 180, 0);
            }
        }

        public Vector3 GetNameTagPosition()
        {
            return nameTagAnchor != null ? nameTagAnchor.position : transform.position + Vector3.up * nameTagHeight;
        }

        public void AlertTo(Vector3 position)
        {
            if (bossAI != null) bossAI.AlertToPosition(position);
        }

        public void Reset()
        {
            if (bossHealth != null) bossHealth.Reset();
            Initialize();
        }

        public void ApplyNewData(BossData newData)
        {
            bossData = newData;
            Initialize();
        }
    }
}
