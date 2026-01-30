using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 적 기본 클래스 - 체력, 데미지 처리
/// 모든 적 타입의 기반
/// </summary>
public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;

    [Header("Detection")]
    [SerializeField] protected float detectionRange = 15f;
    [SerializeField] protected float attackRange = 10f;
    [SerializeField] protected float fieldOfView = 120f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask obstacleLayer;

    [Header("Events")]
    public UnityEvent<float, float> OnHealthChanged;  // current, max
    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamaged;

    // 상태
    protected Transform playerTransform;
    protected bool isDead;
    protected bool playerDetected;

    // 프로퍼티
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public bool PlayerDetected => playerDetected;
    public Transform PlayerTransform => playerTransform;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    protected virtual void Update()
    {
        if (isDead) return;

        CheckPlayerDetection();
    }

    /// <summary>
    /// 플레이어 감지 체크
    /// </summary>
    protected virtual void CheckPlayerDetection()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 거리 체크
        if (distance > detectionRange)
        {
            playerDetected = false;
            return;
        }

        // 시야각 체크
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > fieldOfView / 2f)
        {
            playerDetected = false;
            return;
        }

        // 장애물 체크 (레이캐스트)
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 playerEyePosition = playerTransform.position + Vector3.up * 1.5f;

        if (Physics.Linecast(eyePosition, playerEyePosition, obstacleLayer))
        {
            playerDetected = false;
            return;
        }

        playerDetected = true;
    }

    /// <summary>
    /// 플레이어가 공격 범위 내인지
    /// </summary>
    public bool IsPlayerInAttackRange()
    {
        if (playerTransform == null) return false;
        return Vector3.Distance(transform.position, playerTransform.position) <= attackRange;
    }

    /// <summary>
    /// 플레이어를 향해 회전
    /// </summary>
    public void LookAtPlayer()
    {
        if (playerTransform == null) return;

        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    // ========================================
    // IDamageable 구현
    // ========================================

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnDamaged?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"[Enemy] {gameObject.name} took {damage} damage. HP: {currentHealth}/{maxHealth}");

        // 피격 시 플레이어 감지
        if (!playerDetected && playerTransform != null)
        {
            playerDetected = true;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        OnDeath?.Invoke();

        Debug.Log($"[Enemy] {gameObject.name} died!");

        // 사망 처리 (상속 클래스에서 오버라이드 가능)
        Destroy(gameObject, 2f);
    }

    // ========================================
    // 기즈모
    // ========================================

    protected virtual void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 시야각
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
