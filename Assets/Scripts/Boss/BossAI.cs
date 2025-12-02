using UnityEngine;
using UnityEngine.AI;

namespace HorribleBosses.Boss
{
    /// <summary>
    /// Boss AI state machine handling behavior patterns.
    /// Uses NavMeshAgent for navigation.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class BossAI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossHealth bossHealth;
        [SerializeField] private Transform player;

        [Header("Detection")]
        [SerializeField] private float detectionRange = 12f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float loseTargetRange = 25f;
        [SerializeField] private LayerMask sightBlockingLayers;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 6f;
        [SerializeField] private float rageSpeedMultiplier = 1.3f;

        [Header("Combat")]
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private float attackWindup = 0.5f;  // Time before damage applies
        [SerializeField] private float attackRecovery = 0.3f; // Time after attack before can act

        [Header("Stagger")]
        [SerializeField] private float staggerDuration = 1.5f;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolWaitTime = 2f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        // Components
        private NavMeshAgent agent;
        private Animator animator;

        // State
        private BossState currentState = BossState.Idle;
        private float lastAttackTime;
        private float stateTimer;
        private int currentPatrolIndex;
        private bool isAttacking;
        private bool isStaggered;
        private Vector3 lastKnownPlayerPos;

        // Properties
        public BossState CurrentState => currentState;
        public bool IsInCombat => currentState == BossState.Chase || currentState == BossState.Attack;
        public float DistanceToPlayer => player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            if (bossHealth == null)
                bossHealth = GetComponent<BossHealth>();
        }

        private void Start()
        {
            // Subscribe to health events
            if (bossHealth != null)
            {
                bossHealth.OnStaggered.AddListener(OnStaggered);
                bossHealth.OnRageTriggered.AddListener(OnRageTriggered);
                bossHealth.OnDeath.AddListener(OnDeath);
            }

            // Find player if not assigned
            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
            }

            SetState(BossState.Idle);
        }

        private void Update()
        {
            if (bossHealth != null && bossHealth.IsDead) return;
            if (isStaggered) return;

            stateTimer += Time.deltaTime;

            switch (currentState)
            {
                case BossState.Idle:
                    UpdateIdle();
                    break;
                case BossState.Patrol:
                    UpdatePatrol();
                    break;
                case BossState.Alert:
                    UpdateAlert();
                    break;
                case BossState.Chase:
                    UpdateChase();
                    break;
                case BossState.Attack:
                    UpdateAttack();
                    break;
                case BossState.Rage:
                    UpdateRage();
                    break;
            }

            // Always check for player in range
            CheckForPlayer();
        }

        /// <summary>
        /// Initialize AI with stats from BossData
        /// </summary>
        public void Initialize(BossStats stats)
        {
            detectionRange = stats.detectionRange;
            attackRange = stats.attackRange;
            patrolSpeed = stats.moveSpeed;
            chaseSpeed = stats.chaseSpeed;
            attackCooldown = stats.attackCooldown;
            attackDamage = stats.damage;

            if (agent != null)
            {
                agent.speed = patrolSpeed;
            }
        }

        private void SetState(BossState newState)
        {
            if (currentState == newState) return;

            // Exit current state
            OnExitState(currentState);

            currentState = newState;
            stateTimer = 0f;

            // Enter new state
            OnEnterState(newState);
        }

        private void OnEnterState(BossState state)
        {
            switch (state)
            {
                case BossState.Idle:
                    agent.isStopped = true;
                    break;

                case BossState.Patrol:
                    agent.isStopped = false;
                    agent.speed = patrolSpeed;
                    SetNextPatrolPoint();
                    break;

                case BossState.Alert:
                    agent.isStopped = true;
                    // Play alert sound/animation
                    break;

                case BossState.Chase:
                    agent.isStopped = false;
                    agent.speed = chaseSpeed;
                    break;

                case BossState.Attack:
                    agent.isStopped = true;
                    break;

                case BossState.Rage:
                    agent.speed = chaseSpeed * rageSpeedMultiplier;
                    // Play rage sound
                    break;

                case BossState.Dead:
                    agent.isStopped = true;
                    agent.enabled = false;
                    break;
            }
        }

        private void OnExitState(BossState state)
        {
            // Clean up state-specific things
        }

        private void UpdateIdle()
        {
            // Transition to patrol after a moment
            if (stateTimer >= patrolWaitTime && patrolPoints.Length > 0)
            {
                SetState(BossState.Patrol);
            }
        }

        private void UpdatePatrol()
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                // Reached patrol point, wait then move to next
                if (stateTimer >= patrolWaitTime)
                {
                    SetNextPatrolPoint();
                    stateTimer = 0f;
                }
            }
        }

        private void UpdateAlert()
        {
            // Face player
            if (player != null)
            {
                Vector3 lookDir = (player.position - transform.position).normalized;
                lookDir.y = 0;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    Time.deltaTime * 5f
                );
            }

            // Transition to chase after alert duration
            if (stateTimer >= 1f)
            {
                SetState(bossHealth.IsInRage ? BossState.Rage : BossState.Chase);
            }
        }

        private void UpdateChase()
        {
            if (player == null) return;

            // Move towards player
            agent.SetDestination(player.position);
            lastKnownPlayerPos = player.position;

            // Check if in attack range
            if (DistanceToPlayer <= attackRange && CanAttack())
            {
                SetState(BossState.Attack);
            }

            // Lost player?
            if (DistanceToPlayer > loseTargetRange)
            {
                SetState(BossState.Patrol);
            }
        }

        private void UpdateAttack()
        {
            if (isAttacking) return;

            // Face player
            if (player != null)
            {
                Vector3 lookDir = (player.position - transform.position).normalized;
                lookDir.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDir);
            }

            // Execute attack
            StartAttack();
        }

        private void UpdateRage()
        {
            // Rage mode - more aggressive chasing and attacking
            if (player == null) return;

            agent.SetDestination(player.position);

            if (DistanceToPlayer <= attackRange && CanAttack())
            {
                StartAttack();
            }
        }

        private void CheckForPlayer()
        {
            if (player == null) return;
            if (currentState == BossState.Dead) return;

            float distance = DistanceToPlayer;

            // Already in combat states
            if (IsInCombat || currentState == BossState.Rage) return;

            // Check detection range
            if (distance <= detectionRange)
            {
                // Line of sight check
                if (HasLineOfSight())
                {
                    SetState(BossState.Alert);
                }
            }
        }

        private bool HasLineOfSight()
        {
            if (player == null) return false;

            Vector3 origin = transform.position + Vector3.up * 1.5f; // Eye height
            Vector3 direction = (player.position + Vector3.up - origin).normalized;
            float distance = Vector3.Distance(origin, player.position);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, sightBlockingLayers))
            {
                // Something blocking view
                return hit.transform == player || hit.transform.IsChildOf(player);
            }

            return true;
        }

        private void SetNextPatrolPoint()
        {
            if (patrolPoints.Length == 0) return;

            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        private bool CanAttack()
        {
            return Time.time - lastAttackTime >= attackCooldown && !isAttacking;
        }

        private void StartAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            // Windup
            Invoke(nameof(DealDamage), attackWindup);
            Invoke(nameof(EndAttack), attackWindup + attackRecovery);

            // Animation trigger
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }

        private void DealDamage()
        {
            if (player == null) return;
            if (bossHealth != null && bossHealth.IsDead) return;

            // Check if still in range
            if (DistanceToPlayer <= attackRange * 1.5f)
            {
                // Get player health component
                var playerHealth = player.GetComponent<Player.PlayerHealth>();
                if (playerHealth != null)
                {
                    float damage = bossHealth.IsInRage ? attackDamage * 1.5f : attackDamage;
                    playerHealth.TakeDamage(damage, transform.position);
                }
            }
        }

        private void EndAttack()
        {
            isAttacking = false;

            // Determine next state
            if (bossHealth.IsInRage)
            {
                SetState(BossState.Rage);
            }
            else if (DistanceToPlayer <= detectionRange)
            {
                SetState(BossState.Chase);
            }
            else
            {
                SetState(BossState.Patrol);
            }
        }

        private void OnStaggered()
        {
            isStaggered = true;
            agent.isStopped = true;

            // Play stagger animation
            if (animator != null)
            {
                animator.SetTrigger("Stagger");
            }

            Invoke(nameof(RecoverFromStagger), staggerDuration);
        }

        private void RecoverFromStagger()
        {
            isStaggered = false;
            agent.isStopped = false;

            // Resume combat
            if (bossHealth.IsInRage)
            {
                SetState(BossState.Rage);
            }
            else
            {
                SetState(BossState.Chase);
            }
        }

        private void OnRageTriggered()
        {
            SetState(BossState.Rage);
        }

        private void OnDeath()
        {
            SetState(BossState.Dead);
            CancelInvoke();

            // Ragdoll would be enabled here
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }
        }

        /// <summary>
        /// External alert (from other bosses, alarms, etc.)
        /// </summary>
        public void AlertToPosition(Vector3 position)
        {
            if (currentState == BossState.Dead) return;
            if (IsInCombat) return;

            lastKnownPlayerPos = position;
            SetState(BossState.Alert);
        }

        /// <summary>
        /// Set patrol points at runtime
        /// </summary>
        public void SetPatrolPoints(Transform[] points)
        {
            patrolPoints = points;
        }

        private void OnDrawGizmosSelected()
        {
            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Patrol path
            if (patrolPoints != null && patrolPoints.Length > 1)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    if (patrolPoints[i] == null) continue;
                    int next = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[next] == null) continue;
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
                }
            }
        }
    }

    public enum BossState
    {
        Idle,
        Patrol,
        Alert,
        Chase,
        Attack,
        Rage,
        Dead
    }
}
