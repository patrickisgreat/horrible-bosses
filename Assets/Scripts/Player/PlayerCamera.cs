using UnityEngine;

namespace HorribleBosses.Player
{
    /// <summary>
    /// First-person camera controller with mouse look.
    /// Attach to the player's camera or a camera holder.
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Mouse Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private bool invertY = false;

        [Header("Look Limits")]
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;

        [Header("References")]
        [SerializeField] private Transform playerBody;

        [Header("Screen Shake")]
        [SerializeField] private float shakeDecay = 5f;

        // Current rotation
        private float xRotation = 0f;

        // Screen shake
        private float currentShakeIntensity = 0f;
        private Vector3 shakeOffset;

        // Recoil
        private Vector3 currentRecoil;
        private Vector3 recoilVelocity;

        public float Sensitivity
        {
            get => mouseSensitivity;
            set => mouseSensitivity = value;
        }

        private void Start()
        {
            // Lock and hide cursor for FPS gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleMouseLook();
            UpdateScreenShake();
            UpdateRecoil();
        }

        private void HandleMouseLook()
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Invert Y if enabled
            if (invertY) mouseY = -mouseY;

            // Apply recoil to look
            mouseX += currentRecoil.y;
            mouseY += currentRecoil.x;

            // Vertical rotation (looking up/down) - clamped
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

            // Apply rotation with shake
            Vector3 finalRotation = new Vector3(xRotation, 0f, 0f) + shakeOffset;
            transform.localRotation = Quaternion.Euler(finalRotation);

            // Horizontal rotation (turning) - applied to player body
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * mouseX);
            }
        }

        private void UpdateScreenShake()
        {
            if (currentShakeIntensity > 0)
            {
                // Generate random shake offset
                shakeOffset = Random.insideUnitSphere * currentShakeIntensity;
                shakeOffset.z = 0; // No roll

                // Decay shake over time
                currentShakeIntensity = Mathf.Lerp(currentShakeIntensity, 0, shakeDecay * Time.deltaTime);

                if (currentShakeIntensity < 0.01f)
                {
                    currentShakeIntensity = 0;
                    shakeOffset = Vector3.zero;
                }
            }
        }

        private void UpdateRecoil()
        {
            // Smoothly return recoil to zero
            currentRecoil = Vector3.SmoothDamp(currentRecoil, Vector3.zero, ref recoilVelocity, 0.1f);
        }

        /// <summary>
        /// Trigger screen shake effect (from explosions, taking damage, etc.)
        /// </summary>
        /// <param name="intensity">Shake intensity (0.1 = subtle, 1.0 = intense)</param>
        public void ShakeScreen(float intensity)
        {
            currentShakeIntensity = Mathf.Max(currentShakeIntensity, intensity);
        }

        /// <summary>
        /// Apply weapon recoil
        /// </summary>
        /// <param name="recoilAmount">Recoil in degrees (x = up, y = sideways)</param>
        public void AddRecoil(Vector2 recoilAmount)
        {
            currentRecoil += new Vector3(-recoilAmount.x, recoilAmount.y, 0);
        }

        /// <summary>
        /// Toggle cursor lock (for menus)
        /// </summary>
        public void SetCursorLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
