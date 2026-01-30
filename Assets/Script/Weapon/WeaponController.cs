using System;
using UnityEngine;

/// <summary>
/// 무기 컨트롤러 - 사격, 재장전 처리
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponData currentWeaponData;

    [Header("Fire Point")]
    [SerializeField] private Transform firePoint;

    [Header("Effects")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject impactEffectPrefab;

    // 상태
    private int currentAmmo;
    private bool isReloading;

    // 이벤트
    public event Action OnFire;
    public event Action OnReload;
    public event Action<int, int> OnAmmoChanged; // current, max

    // 프로퍼티
    public WeaponData CurrentWeapon => currentWeaponData;
    public int CurrentAmmo => currentAmmo;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        if (currentWeaponData != null)
        {
            currentAmmo = currentWeaponData.magazineSize;
        }
    }

    private void Start()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }

        OnAmmoChanged?.Invoke(currentAmmo, currentWeaponData?.magazineSize ?? 30);
    }

    // ========================================
    // 공개 메서드 (State에서 호출)
    // ========================================

    /// <summary>
    /// 발사 가능 여부
    /// </summary>
    public bool CanFire()
    {
        return !isReloading && HasAmmo();
    }

    /// <summary>
    /// 탄약 있는지
    /// </summary>
    public bool HasAmmo()
    {
        return currentAmmo > 0;
    }

    /// <summary>
    /// 재장전 가능 여부
    /// </summary>
    public bool CanReload()
    {
        return !isReloading && currentAmmo < (currentWeaponData?.magazineSize ?? 30);
    }

    /// <summary>
    /// 발사
    /// </summary>
    public void Fire()
    {
        if (!CanFire()) return;

        currentAmmo--;

        // 레이캐스트
        PerformRaycast();

        // 이펙트
        SpawnMuzzleFlash();

        // 사운드
        PlaySound(currentWeaponData?.fireSound);

        // 이벤트
        OnFire?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo, currentWeaponData?.magazineSize ?? 30);

        Debug.Log($"[Weapon] 발사! 남은 탄약: {currentAmmo}");
    }

    /// <summary>
    /// 재장전
    /// </summary>
    public void Reload()
    {
        if (currentWeaponData != null)
        {
            currentAmmo = currentWeaponData.magazineSize;
        }

        isReloading = false;

        PlaySound(currentWeaponData?.reloadSound);

        OnReload?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo, currentWeaponData?.magazineSize ?? 30);

        Debug.Log($"[Weapon] 재장전 완료! 탄약: {currentAmmo}");
    }

    /// <summary>
    /// 발사 속도 가져오기
    /// </summary>
    public float GetFireRate()
    {
        return currentWeaponData?.fireRate ?? 0.1f;
    }

    /// <summary>
    /// 재장전 시간 가져오기
    /// </summary>
    public float GetReloadTime()
    {
        return currentWeaponData?.reloadTime ?? 2f;
    }

    // ========================================
    // 내부 메서드
    // ========================================
    private void PerformRaycast()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float maxRange = currentWeaponData?.maxRange ?? 100f;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
        {
            // ========== 디버그 시각화 ==========
            // 히트된 경우: 녹색 선
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 0.5f);
            Debug.Log($"[Raycast] ✓ HIT! Target: {hit.collider.name}, Distance: {hit.distance:F2}m, Point: {hit.point}");

            // 데미지 적용
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = hit.collider.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                damageable.TakeDamage(currentWeaponData?.damage ?? 10f);
                Debug.Log($"[Raycast] 💥 DAMAGE! {currentWeaponData?.damage ?? 10f} to {hit.collider.name}");
            }
            else
            {
                Debug.Log($"[Raycast] ⚪ No IDamageable on {hit.collider.name}");
            }

            // 임팩트 이펙트
            SpawnImpactEffect(hit.point, hit.normal);
        }
        else
        {
            // 미스된 경우: 빨간 선
            Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 0.5f);
            Debug.Log($"[Raycast] ✗ MISS! No hit within {maxRange}m");
        }
    }

    private void SpawnMuzzleFlash()
    {
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f);
        }
    }

    private void SpawnImpactEffect(Vector3 position, Vector3 normal)
    {
        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
            Destroy(impact, 2f);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
