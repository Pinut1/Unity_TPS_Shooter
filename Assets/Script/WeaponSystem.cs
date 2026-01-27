using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class WeaponSystem : MonoBehaviour
{
    [Header("Detail Ammo UI")] 
    public TMP_Text currentAmmoText; // 위쪽 (현재 탄약)
    public TMP_Text spareAmmoText;   // 아래쪽 (예비 탄약)
    public Image reloadRingImage;    // 재장전 게이지 (평소엔 꺼둠)

    [SerializeField] private Color normalColor = new Color(1f, 0.58f, 0f); // 디비전 주황색
    [SerializeField] private Color warningColor = Color.red;

    // 인스펙터에 UI , Recoil 연결
    [SerializeField] private RecoilSystem recoilSystem;
    [SerializeField] private DynamicCrosshair crosshair;  // UI 담당 (추가)

    // 타격 피드백
    [SerializeField] private HitIndicator hitIndicator; 

    private Camera _mainCamera;
    [SerializeField] float range = 100f; // 사거리

    public Animator animator;
    
    float fireRate = 0.1f; //발사 속도
    private float nextfireTime = 0f; // 다음 발사 시간 계산기

    [SerializeField] private Transform muzzlePoint; // 트랜스폼으로 위치 받아오기


    [SerializeField] private GameObject hitEffectPrefab;   

    private LineRenderer _lineRenderer;



    [Header("사운드 설정")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip dryFireSound;
    [SerializeField] private AudioClip cocKingSound;

    [Header("탄약 설정")]
    public int spareAmmo = 180;
    public int maxAmmo = 30;
    public int currentAmmo;
    public bool isReloading = false;

    public float reloadTime = 2.0f;
    [SerializeField] private AudioClip recoilSound;



    Ray ray = new Ray();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmo = maxAmmo;
        reloadRingImage.gameObject.SetActive(false);

        _lineRenderer = GetComponent<LineRenderer>();
        _mainCamera = Camera.main;
                   
        _lineRenderer.enabled = false;

        UpdateAmmoUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        if (Input.GetMouseButton(0) && Time.time >= nextfireTime)
        {
            if (currentAmmo > 0 )
            {
                nextfireTime = Time.time + fireRate;
                StartCoroutine(ShootingRoutine());
            }
            else
            {
                nextfireTime = Time.time + fireRate;
                PlaySound(dryFireSound);
                Debug.Log("탄약 부족");
            }
        }
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (currentAmmoText != null)
        {
            if(isReloading)
            {
                currentAmmoText.text = currentAmmo.ToString();
            }
            else currentAmmoText.color = (currentAmmo <= maxAmmo * 0.3) ? Color.red : Color.white;


            currentAmmoText.text = currentAmmo.ToString();
        }
        if (spareAmmoText != null)
        {
            spareAmmoText.text = spareAmmo.ToString();
            spareAmmoText.color = Color.gray;
        }
    }
    IEnumerator ShootingRoutine()
        {
        currentAmmo--;
        Debug.Log(currentAmmo);
        PlaySound(fireSound);

        ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        int layerMask = 1 << LayerMask.NameToLayer("Player");

        layerMask = ~layerMask;

        Vector3 targetPoint = ray.origin + ray.direction * range;

        if (Physics.Raycast(ray, out hit, range,layerMask))
        {
            // 맞았을 때: hit.point가 탄착점
            targetPoint = hit.point;
            Debug.Log("Hit point :" + hit.point + "Distance" + hit.distance + "name" + hit.collider.name);
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(10);
                GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

                hitIndicator.ShowHit();
                Destroy(effect, 1f);
            }

        }
     
        else targetPoint = ray.origin + (ray.direction * range);

        if (recoilSystem != null)
        {
            recoilSystem.FireRecoil();

            crosshair.FireRecoil();
        }
        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(0, muzzlePoint.position);
        _lineRenderer.SetPosition(1, targetPoint);

        yield return new WaitForSeconds(0.01f);

        _lineRenderer.enabled = false;
    }

    IEnumerator ReloadRoutine()
    {
        if(spareAmmo <= 0) yield break;

        isReloading = true;
        animator.SetTrigger("isReload");
        crosshair.gameObject.SetActive(false);
        if (reloadRingImage != null)
        {
            reloadRingImage.gameObject.SetActive(true);
            reloadRingImage.fillAmount = 0;
        }

        float timePassed = 0f;
        while (timePassed < reloadTime)
        {
            timePassed += Time.deltaTime;
            
            if(reloadTime!=null)
            {
                reloadRingImage.fillAmount = timePassed / reloadTime;
            }
            yield return null;
        }

        if (reloadRingImage != null)
        {
            reloadRingImage.fillAmount = 1f;
            yield return new WaitForSeconds(0.1f);
            reloadRingImage.gameObject.SetActive(false);
        }

        crosshair.gameObject.SetActive(true);   

        int ammoToFill = maxAmmo - currentAmmo;
        if(spareAmmo >= ammoToFill)
        {
            currentAmmo = maxAmmo;
            spareAmmo -= ammoToFill;
        }
        else
        {
            currentAmmo += spareAmmo;
            spareAmmo =0;
        }

        PlaySound(cocKingSound);
        isReloading = false;

        UpdateAmmoUI();
        Debug.Log("장전 루틴 끝");
    } 

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }

    }
}
