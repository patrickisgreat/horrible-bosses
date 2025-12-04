using UnityEngine;

namespace HorribleBosses.Pickups
{
    /// <summary>
    /// Base class for all pickups. Handles rotation, bobbing, and collection.
    /// </summary>
    public abstract class PickupBase : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] protected float rotationSpeed = 90f;
        [SerializeField] protected float bobSpeed = 2f;
        [SerializeField] protected float bobHeight = 0.2f;

        [Header("Collection")]
        [SerializeField] protected float respawnTime = 30f;
        [SerializeField] protected bool respawns = true;
        [SerializeField] protected string playerTag = "Player";

        [Header("Audio")]
        [SerializeField] protected AudioClip pickupSound;

        [Header("Effects")]
        [SerializeField] protected GameObject pickupEffectPrefab;

        protected Vector3 startPosition;
        protected MeshRenderer meshRenderer;
        protected Collider pickupCollider;
        protected bool isCollected;

        protected virtual void Awake()
        {
            startPosition = transform.position;
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            pickupCollider = GetComponent<Collider>();
        }

        protected virtual void Update()
        {
            if (isCollected) return;

            // Rotate
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Bob up and down
            Vector3 pos = startPosition;
            pos.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = pos;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;
            if (!other.CompareTag(playerTag)) return;

            if (CanCollect(other.gameObject))
            {
                Collect(other.gameObject);
            }
        }

        protected abstract bool CanCollect(GameObject collector);
        protected abstract void ApplyPickup(GameObject collector);

        protected virtual void Collect(GameObject collector)
        {
            isCollected = true;

            ApplyPickup(collector);

            // Play sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Spawn effect
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // Hide or destroy
            if (respawns)
            {
                SetVisible(false);
                Invoke(nameof(Respawn), respawnTime);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void Respawn()
        {
            isCollected = false;
            SetVisible(true);
        }

        protected void SetVisible(bool visible)
        {
            if (meshRenderer != null) meshRenderer.enabled = visible;
            if (pickupCollider != null) pickupCollider.enabled = visible;
        }
    }
}
