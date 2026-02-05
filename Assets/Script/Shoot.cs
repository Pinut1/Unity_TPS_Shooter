using UnityEngine;

public class Shoot : MonoBehaviour
{
    RaycastHit rayHit;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float range = 100f;

    [Header("Effects")]
    [SerializeField] private GameObject commonEffect;
    [SerializeField] private GameObject metalEffect;
    [SerializeField] private GameObject sandEffect;

    float fireRate = 0.2f;
    float nextFire = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0) && Time.time >= nextFire)
        {
            nextFire = Time.time + fireRate;
            ShootBullet();
        }
    }

    void ShootBullet()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));  

        if(Physics.Raycast(ray, out rayHit, range))
        {
            Debug.Log(rayHit.collider.name);
            
            // 기본은 Common 이펙트
            GameObject effectToSpawn = commonEffect;

            // 레이어 확인
            if(rayHit.collider.gameObject.layer == LayerMask.NameToLayer("Metal"))
            {
                effectToSpawn = metalEffect;
            }
            else if(rayHit.collider.gameObject.layer == LayerMask.NameToLayer("Sand"))
            {
                effectToSpawn = sandEffect;
            }

            GameObject clone = Instantiate(effectToSpawn , rayHit.point , Quaternion.LookRotation(rayHit.normal));
            Destroy(clone, 2f);
        }

    }
}
