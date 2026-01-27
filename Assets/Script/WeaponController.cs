using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Stats")]
    public int maxAmmo = 30;
    public int currentAmmo;
    public int spareAmmo = 180;
    public float fireRate = 0.1f;
    public float range = 100f;
    public float reloadTime = 2.0f;

    [Header("References")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private ParticleSystem muzzleFlash; // Optional
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private RecoilSystem recoilSystem;
    [SerializeField] private DynamicCrosshair crosshair;
    [SerializeField] private HitIndicator hitIndicator;
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip dryFireSound;
    [SerializeField] private AudioClip cocKingSound;

    // Events for UI
    public System.Action<int, int> OnAmmoChanged;
    public System.Action OnFire;
    public System.Action OnReloadStart;
    public System.Action OnReloadComplete;

    private bool isReloading = false;
    private float nextFireTime = 0f;
    private Camera _mainCamera;

    void Start()
    {
        currentAmmo = maxAmmo;
        _mainCamera = Camera.main;
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

        UpdateAmmo();
    }

    void Update()
    {
        if (isReloading) return;

        // Reload Input
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        // Fire Input
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                nextFireTime = Time.time + fireRate;
                StartCoroutine(FireRoutine());
            }
            else
            {
                nextFireTime = Time.time + fireRate;
                PlaySound(dryFireSound);
            }
        }
    }

    IEnumerator FireRoutine()
    {
        currentAmmo--;
        UpdateAmmo();
        PlaySound(fireSound);
        OnFire?.Invoke();

        if (recoilSystem != null) recoilSystem.FireRecoil();
        if (crosshair != null) crosshair.FireRecoil();

        // Raycast
        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Player"); // Exclude Player

        Vector3 targetPoint = ray.origin + ray.direction * range;

        if (Physics.Raycast(ray, out hit, range, layerMask))
        {
            targetPoint = hit.point;

            // Check Enemy
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(10);
                if (hitIndicator != null) hitIndicator.ShowHit();
            }

            // Effect
            if (hitEffectPrefab != null)
            {
                if (ObjectPool.Instance != null)
                {
                    ObjectPool.Instance.SpawnFromPool(hit.point, Quaternion.LookRotation(hit.normal));
                }
                else
                {
                    GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(effect, 1f);
                }
            }
            else
            {
                Debug.LogWarning("WeaponController: HitEffectPrefab is missing!");
            }
        }

        // Line Renderer
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, muzzlePoint != null ? muzzlePoint.position : transform.position);
            lineRenderer.SetPosition(1, targetPoint);
            yield return new WaitForSeconds(0.01f);
            lineRenderer.enabled = false;
        }
    }

    IEnumerator ReloadRoutine()
    {
        if (spareAmmo <= 0) yield break;

        isReloading = true;
        OnReloadStart?.Invoke();

        // Reload Sound
        // PlaySound(cocKingSound); // Usually at start or end? Original had it at end.

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = maxAmmo - currentAmmo;
        if (spareAmmo >= ammoNeeded)
        {
            currentAmmo = maxAmmo;
            spareAmmo -= ammoNeeded;
        }
        else
        {
            currentAmmo += spareAmmo;
            spareAmmo = 0;
        }

        PlaySound(cocKingSound);
        isReloading = false;
        OnReloadComplete?.Invoke();
        UpdateAmmo();
    }

    private void UpdateAmmo()
    {
        OnAmmoChanged?.Invoke(currentAmmo, spareAmmo);
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
