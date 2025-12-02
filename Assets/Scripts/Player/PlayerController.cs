using UnityEngine;

namespace HorribleBosses.Player
{
    /// <summary>
    /// First-person player controller handling movement, jumping, and sprinting.
    /// Attach to a player GameObject with a CharacterController component.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float jumpForce = 7f;
        [SerializeField] private float gravity = -20f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;

        [Header("Head Bob")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float bobFrequency = 2f;
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private Transform cameraTransform;

        // Components
        private CharacterController controller;

        // Movement state
        private Vector3 velocity;
        private bool isGrounded;
        private bool isSprinting;
        private float bobTimer;
        private float defaultCameraY;

        // Public properties for other systems to check
        public bool IsGrounded => isGrounded;
        public bool IsSprinting => isSprinting;
        public bool IsMoving => controller.velocity.magnitude > 0.1f;
        public float CurrentSpeed => controller.velocity.magnitude;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform != null)
            {
                defaultCameraY = cameraTransform.localPosition.y;
            }
        }

        private void Update()
        {
            CheckGround();
            HandleMovement();
            HandleJump();
            ApplyGravity();

            if (enableHeadBob && cameraTransform != null)
            {
                HandleHeadBob();
            }
        }

        private void CheckGround()
        {
            // Check if player is on ground using a sphere at feet
            isGrounded = Physics.CheckSphere(
                groundCheck != null ? groundCheck.position : transform.position,
                groundDistance,
                groundMask
            );

            // Reset downward velocity when grounded
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
        }

        private void HandleMovement()
        {
            // Get input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // Check sprint
            isSprinting = Input.GetKey(KeyCode.LeftShift) && vertical > 0;
            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // Calculate movement direction relative to player facing
            Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
            moveDirection = moveDirection.normalized;

            // Apply movement
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        private void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }
        }

        private void ApplyGravity()
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleHeadBob()
        {
            if (!isGrounded || !IsMoving)
            {
                // Smoothly return to default position
                bobTimer = 0;
                Vector3 pos = cameraTransform.localPosition;
                pos.y = Mathf.Lerp(pos.y, defaultCameraY, Time.deltaTime * 8f);
                cameraTransform.localPosition = pos;
                return;
            }

            // Calculate bob based on movement
            float speedMultiplier = isSprinting ? 1.5f : 1f;
            bobTimer += Time.deltaTime * bobFrequency * speedMultiplier;

            float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;
            Vector3 newPos = cameraTransform.localPosition;
            newPos.y = defaultCameraY + bobOffset;
            cameraTransform.localPosition = newPos;
        }

        // Public method to apply knockback (from explosions, boss attacks, etc.)
        public void ApplyKnockback(Vector3 force)
        {
            velocity += force;
        }
    }
}
