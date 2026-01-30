using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 AI 컨트롤러 - FSM 기반
/// 상태: Idle, Patrol, Chase, Attack, Cover
/// </summary>
[RequireComponent(typeof(EnemyBase))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float chaseTimeout = 5f;  // 플레이어를 잃으면 대기 시간

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("Cover")]
    [SerializeField] private float coverSearchRadius = 10f;
    [SerializeField] private LayerMask coverLayer;

    [Header("Combat")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask shootLayers;

    // 컴포넌트
    private EnemyBase enemyBase;
    private NavMeshAgent agent;
    private Animator animator;

    // FSM
    private EnemyStateMachine stateMachine;
    public EnemyIdleState IdleState { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }

    // 타이머
    private float lastAttackTime;
    private float lostPlayerTimer;

    // 프로퍼티
    public EnemyBase EnemyBase => enemyBase;
    public NavMeshAgent Agent => agent;
    public Animator Animator => animator;
    public Transform[] PatrolPoints => patrolPoints;
    public float PatrolWaitTime => patrolWaitTime;
    public float AttackCooldown => attackCooldown;
    public float ChaseTimeout => chaseTimeout;
    public float Damage => damage;
    public Transform FirePoint => firePoint;

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // FSM 초기화
        stateMachine = new EnemyStateMachine();
        IdleState = new EnemyIdleState(this, stateMachine);
        PatrolState = new EnemyPatrolState(this, stateMachine);
        ChaseState = new EnemyChaseState(this, stateMachine);
        AttackState = new EnemyAttackState(this, stateMachine);
    }

    private void Start()
    {
        // 초기 상태: Patrol 포인트가 있으면 Patrol, 없으면 Idle
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            stateMachine.Initialize(PatrolState);
        }
        else
        {
            stateMachine.Initialize(IdleState);
        }
    }

    private void Update()
    {
        if (enemyBase.IsDead)
        {
            agent.isStopped = true;
            return;
        }

        stateMachine.CurrentState?.Update();

        // 애니메이션 업데이트
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = agent.velocity.magnitude / agent.speed;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }

    /// <summary>
    /// 공격 가능 여부
    /// </summary>
    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    /// <summary>
    /// 공격 실행
    /// </summary>
    public void Attack()
    {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;
        animator?.SetTrigger("Attack");

        // 레이캐스트 사격
        if (firePoint != null && enemyBase.PlayerTransform != null)
        {
            Vector3 direction = (enemyBase.PlayerTransform.position + Vector3.up - firePoint.position).normalized;

            if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, enemyBase.AttackRange, shootLayers))
            {
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                    Debug.Log($"[EnemyAI] Hit {hit.collider.name} for {damage} damage");
                }
            }
        }
    }

    /// <summary>
    /// 목적지로 이동
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            agent.SetDestination(destination);
        }
    }

    /// <summary>
    /// 이동 정지
    /// </summary>
    public void StopMoving()
    {
        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
        }
    }

    /// <summary>
    /// 목적지 도달 여부
    /// </summary>
    public bool HasReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
