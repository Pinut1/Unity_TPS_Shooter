using UnityEngine;

public class SandBag_Enemy : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     protected override void Start()
    {
        base.Start();
        currentHp = 100000;
    }

    // Update removed as it was empty and calling base.Update()
}
