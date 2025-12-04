using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace HorribleBosses.Combat
{
    /// <summary>
    /// Spawns and manages floating damage numbers.
    /// Uses object pooling for performance.
    /// </summary>
    public class DamageNumbers : MonoBehaviour
    {
        public static DamageNumbers Instance { get; private set; }

        [Header("Prefab")]
        [SerializeField] private GameObject damageTextPrefab;

        [Header("Pool Settings")]
        [SerializeField] private int poolSize = 20;

        [Header("Animation")]
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float floatHeight = 1.5f;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float randomSpread = 0.5f;

        [Header("Scaling")]
        [SerializeField] private float baseScale = 0.02f;
        [SerializeField] private float criticalScale = 0.03f;
        [SerializeField] private float distanceScaleFactor = 0.1f;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color criticalColor = Color.yellow;
        [SerializeField] private Color healColor = Color.green;
        [SerializeField] private Color fireColor = new Color(1f, 0.5f, 0f);

        private List<DamageTextInstance> pool = new List<DamageTextInstance>();
        private Camera mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            mainCamera = Camera.main;
            CreatePool();
        }

        private void CreatePool()
        {
            if (damageTextPrefab == null)
            {
                // Create a simple text prefab if none assigned
                CreateDefaultPrefab();
            }

            for (int i = 0; i < poolSize; i++)
            {
                CreatePooledInstance();
            }
        }

        private void CreateDefaultPrefab()
        {
            damageTextPrefab = new GameObject("DamageTextPrefab");
            var tmp = damageTextPrefab.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 36;
            tmp.fontStyle = FontStyles.Bold;
            damageTextPrefab.SetActive(false);
            damageTextPrefab.transform.SetParent(transform);
        }

        private DamageTextInstance CreatePooledInstance()
        {
            GameObject obj = Instantiate(damageTextPrefab, transform);
            var instance = new DamageTextInstance
            {
                gameObject = obj,
                text = obj.GetComponent<TextMeshPro>(),
                isActive = false
            };
            obj.SetActive(false);
            pool.Add(instance);
            return instance;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            foreach (var instance in pool)
            {
                if (!instance.isActive) continue;

                instance.timer += deltaTime;

                // Float upward
                instance.gameObject.transform.position += Vector3.up * floatSpeed * deltaTime;

                // Face camera
                if (mainCamera != null)
                {
                    instance.gameObject.transform.LookAt(
                        instance.gameObject.transform.position + mainCamera.transform.forward
                    );
                }

                // Fade out
                float alpha = 1f - (instance.timer / lifetime);
                if (instance.text != null)
                {
                    Color c = instance.text.color;
                    c.a = alpha;
                    instance.text.color = c;
                }

                // Scale based on distance
                if (mainCamera != null)
                {
                    float distance = Vector3.Distance(mainCamera.transform.position, instance.gameObject.transform.position);
                    float scale = instance.baseScale * (1f + distance * distanceScaleFactor);
                    instance.gameObject.transform.localScale = Vector3.one * scale;
                }

                // Deactivate when done
                if (instance.timer >= lifetime)
                {
                    instance.isActive = false;
                    instance.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Spawn damage number at position
        /// </summary>
        public void SpawnDamage(Vector3 position, float damage, DamageType type = DamageType.Normal, bool isCritical = false)
        {
            var instance = GetPooledInstance();
            if (instance == null) return;

            // Random offset
            Vector3 offset = new Vector3(
                Random.Range(-randomSpread, randomSpread),
                Random.Range(0, randomSpread),
                Random.Range(-randomSpread, randomSpread)
            );

            instance.gameObject.transform.position = position + offset;
            instance.timer = 0f;
            instance.isActive = true;
            instance.baseScale = isCritical ? criticalScale : baseScale;
            instance.gameObject.transform.localScale = Vector3.one * instance.baseScale;

            if (instance.text != null)
            {
                instance.text.text = Mathf.RoundToInt(damage).ToString();
                instance.text.color = GetColorForType(type, isCritical);

                if (isCritical)
                {
                    instance.text.text += "!";
                }
            }

            instance.gameObject.SetActive(true);
        }

        /// <summary>
        /// Spawn heal number
        /// </summary>
        public void SpawnHeal(Vector3 position, float amount)
        {
            var instance = GetPooledInstance();
            if (instance == null) return;

            instance.gameObject.transform.position = position + Vector3.up * 0.5f;
            instance.timer = 0f;
            instance.isActive = true;
            instance.baseScale = baseScale;

            if (instance.text != null)
            {
                instance.text.text = "+" + Mathf.RoundToInt(amount);
                instance.text.color = healColor;
            }

            instance.gameObject.SetActive(true);
        }

        private Color GetColorForType(DamageType type, bool isCritical)
        {
            if (isCritical) return criticalColor;

            return type switch
            {
                DamageType.Fire => fireColor,
                _ => normalColor
            };
        }

        private DamageTextInstance GetPooledInstance()
        {
            foreach (var instance in pool)
            {
                if (!instance.isActive) return instance;
            }

            // Pool exhausted, create new
            return CreatePooledInstance();
        }

        private class DamageTextInstance
        {
            public GameObject gameObject;
            public TextMeshPro text;
            public bool isActive;
            public float timer;
            public float baseScale;
        }
    }
}
