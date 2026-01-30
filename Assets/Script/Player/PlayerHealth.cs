using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어 체력 관리 (SRP - 체력만 담당)
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;  // 체력 비율 전달 (0~1)
    public UnityEvent OnDamaged;
    public UnityEvent OnDeath;

    // 프로퍼티
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;
    public float HealthRatio => currentHealth / maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// 데미지를 받습니다. (IDamageable 구현)
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnDamaged?.Invoke();
        OnHealthChanged?.Invoke(HealthRatio);

        Debug.Log($"[Player] 데미지: {damage}, 현재 체력: {currentHealth}/{maxHealth}");

        if (!IsAlive)
        {
            Die();
        }
    }

    /// <summary>
    /// 체력을 회복합니다.
    /// </summary>
    public void Heal(float amount)
    {
        if (!IsAlive) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(HealthRatio);
    }

    /// <summary>
    /// 체력을 최대로 회복합니다.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(HealthRatio);
    }

    private void Die()
    {
        Debug.Log("[Player] 사망!");
        OnDeath?.Invoke();
    }
}
