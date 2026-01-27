using UnityEngine;

public abstract class Enemy : MonoBehaviour
{

    // 자식들만 사용 할 수 있도록 protected
    [SerializeField] protected int maxHp = 100;
    protected int currentHp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        currentHp = maxHp;
    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;

        Debug.Log("Ray HP : " + currentHp);

        if (currentHp <= 0)
        {
            Die();
        }

    }


        protected virtual void Die()
        {
            Destroy(gameObject);
        }

}

