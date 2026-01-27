using UnityEngine;
using UnityEngine.AI;

public class Zombie : Enemy
{
    public Transform target;
    private NavMeshAgent agent;
    protected float moveSpeed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        agent = GetComponent<NavMeshAgent>();

        // Fix: Check target null and safely find player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
    protected override void Die()
    {
        Debug.Log("Zombie Die"); // Encoding fix
        base.Die();
    }
}
