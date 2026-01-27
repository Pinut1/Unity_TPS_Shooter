using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{

    [Header("체력 설정")]
    public int maxHp = 100;
    public int currentHp;

    [Header("UI 연결")]
    public LinearHealthBar linearHealthBar;

    [Header("자동 회복")]
    public bool autoRegen = true;
    public float regenDelay = 3.0f; // 데미지 입고 3초 뒤 회복 시작
    public float regenRate = 10f;   // 초당 회복량

    private float lastDamageTime;
    private bool isDead = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        linearHealthBar.CreateSegments();
        currentHp = 0;
        UpdateHealthUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (autoRegen && !isDead && currentHp < maxHp)
        {
            if (Time.time - lastDamageTime > regenDelay)
            {
                currentHp += (int)(regenRate * Time.deltaTime);
                if (currentHp > maxHp) currentHp = maxHp;
                UpdateHealthUI();
            }
        }
    }
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHp -= damage; 
        lastDamageTime = Time.time;

        Debug.Log("플레이어 데미지 남은 체력 " + currentHp);

        if(currentHp <= 0)
        {
            Die();
        }
    }
    void UpdateHealthUI()
    {
        if (linearHealthBar != null)
        {

            linearHealthBar.UpdateHealth(currentHp, maxHp);
        }

    }
    void Die()
    {
        isDead = true;
        Debug.Log("플레이어 사망");
    }
}
