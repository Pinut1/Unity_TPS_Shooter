# 개선 및 리팩토링 가이드 (Refactoring & Improvement Guide)

사용자의 요청에 따라 `CodeReview.md`의 제안 사항을 바탕으로 구체적인 코드 개선안을 정리하였습니다. 이 문서는 바로 적용 가능한 **버그 수정**, 성능을 위한 **구조 개선(Object Pooling)**, 그리고 유지보수를 위한 **스크립트 분할(Refactoring)** 코드를 포함합니다.

> [!WARNING]
> **스크립트 분할 및 이름 변경 시 주의사항**
> 기존 스크립트(`TacticalController`, `WeaponSystem`)를 분할하거나 이름을 변경하면, Unity 에디터에서 해당 스크립트가 연결된 `GameObject`의 **컴포넌트 연결이 해제되거나(Missing Script), 설정된 변수 값(Inspector 값)들이 초기화**될 수 있습니다. 적용 전에 반드시 백업을 하거나, 값을 메모해두시기 바랍니다.

---

## 1. 버그 수정 및 간단한 개선 (Bug Fixes & Simple Improvements)

### 1.1 `Zombie.cs` 타겟 탐색 로직 수정
기존 코드에서는 `player`가 없을 때 접근하여 에러가 발생할 수 있는 부분이 있었습니다.

```csharp
// 수정된 Zombie.cs의 Start 로직
protected override void Start()
{
    base.Start();
    agent = GetComponent<NavMeshAgent>();

    // agent가 없거나, 타겟이 설정되지 않았을 때 플레이어를 찾음
    if (target == null)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }
}
```

### 1.2 `DynamicCrosshair.cs` 오타 및 최적화
매 프레임 `Raycast`와 `GetComponent`를 하는 비효율을 개선하고 오타를 수정합니다.

```csharp
// 오타 수정: isEnenmy -> isEnemy
// 최적화: LayerMask 활용 (Player 레이어 제외 등)
void CheckEnemyTarget()
{
    int layerMask = ~LayerMask.GetMask("Player"); // 플레이어 제외
    Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    RaycastHit hit;
    bool isEnemy = false;

    if (Physics.Raycast(ray, out hit, 100f, layerMask))
    {
        // 태그 비교가 GetComponent보다 훨씬 빠름 ('Enemy' 태그 설정 권장)
        if (hit.collider.CompareTag("Enemy")) 
        {
            isEnemy = true;
        }
        else if (hit.collider.GetComponentInParent<Enemy>() != null)
        {
            isEnemy = true;
        }
    }
    // ... 색상 변경 로직 ...
}
```

---

## 2. 성능 최적화: 오브젝트 풀링 (Object Pooling)
총알 이펙트(`hitEffectPrefab`) 생성/파괴 부하를 줄이기 위한 유틸리티 클래스입니다.

**[NEW] `ObjectPool.cs`**
```csharp
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    public GameObject prefab;
    public int poolSize = 20;

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    public GameObject SpawnFromPool(Vector3 position, Quaternion rotation)
    {
        if (poolQueue.Count == 0) InitializePool(); // 부족하면 추가 생성

        GameObject objectToSpawn = poolQueue.Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolQueue.Enqueue(objectToSpawn); // 다시 큐에 넣어서 순환 (혹은 반환 로직 별도 구현)

        return objectToSpawn;
    }
}
```

---

## 3. 스크립트 분할 및 리팩토링 (Script Splitting)

`WeaponSystem.cs`는 **로직(데이터), UI, 사운드, 입력**이 섞여 있어 복잡합니다. 이를 역할별로 분리하는 것을 제안합니다.

### 3.1 무기 데이터 및 로직 (`WeaponController.cs`)
핵심 기능만 담당합니다.

```csharp
using UnityEngine;
using System.Collections;

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
    [SerializeField] private ParticleSystem muzzleFlash;

    // 이벤트: UI나 오디오 쪽에서 구독하여 처리
    public System.Action<int, int> OnAmmoChanged; 
    public System.Action OnFire;
    public System.Action OnReloadStart;
    public System.Action OnReloadComplete;

    private bool isReloading = false;
    private float nextFireTime = 0f;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmo();
    }

    public void TryFire()
    {
        if (isReloading || Time.time < nextFireTime) return;

        if (currentAmmo > 0)
        {
            nextFireTime = Time.time + fireRate;
            Fire();
        }
        else
        {
            // Dry Fire (탄약 없음) 처리
        }
    }

    private void Fire()
    {
        currentAmmo--;
        UpdateAmmo();
        OnFire?.Invoke();
        
        // Raycast 로직 ...
        // Hit 처리 로직 ...
    }

    public void Reload()
    {
        if (isReloading || currentAmmo >= maxAmmo || spareAmmo <= 0) return;
        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        OnReloadStart?.Invoke();

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = maxAmmo - currentAmmo;
        if (spareAmmo >= ammoNeeded)
        {
            currentAmmo += ammoNeeded;
            spareAmmo -= ammoNeeded;
        }
        else
        {
            currentAmmo += spareAmmo;
            spareAmmo = 0;
        }

        isReloading = false;
        OnReloadComplete?.Invoke();
        UpdateAmmo();
    }

    private void UpdateAmmo()
    {
        OnAmmoChanged?.Invoke(currentAmmo, spareAmmo);
    }
}
```

### 3.2 무기 UI (`WeaponUI.cs`)
`WeaponController`의 이벤트를 받아 UI만 갱신합니다.

```csharp
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private TMP_Text currentAmmoText;
    [SerializeField] private TMP_Text spareAmmoText;
    [SerializeField] private Image reloadRing;

    void Start()
    {
        // 이벤트 구독
        weaponController.OnAmmoChanged += UpdateAmmoDisplay;
        weaponController.OnReloadStart += () => reloadRing.gameObject.SetActive(true);
        weaponController.OnReloadComplete += () => reloadRing.gameObject.SetActive(false);
    }

    void UpdateAmmoDisplay(int current, int spare)
    {
        currentAmmoText.text = current.ToString();
        spareAmmoText.text = spare.ToString();
        currentAmmoText.color = (current <= 10) ? Color.red : Color.white;
    }
    
    // Update에서 reloadRing fillAmount 처리 가능
}
```

---

## 4. 같이 고민해볼 점 (Discussion Points)

### 4.1 Input System 도입
현재 `Input.GetMouseButton(0)`, `Input.GetKey(KeyCode.R)` 등이 코드 곳곳에 산재해 있습니다. 키 변경 설정(Key Remapping)을 지원하려면 Unity의 **New Input System**을 도입하거나, 입력을 관리하는 `InputManager` 클래스를 따로 만드는 것이 좋습니다.

### 4.2 네임스페이스 (Namespace) 사용
프로젝트 규모가 커지면 다른 에셋의 클래스(예: `Enemy`, `Weapon`)와 이름이 겹칠 수 있습니다.
- 예: `namespace MyGame.Combat`, `namespace MyGame.UI` 등으로 감싸서 관리하는 것을 추천합니다.

### 4.3 하드코딩 제거
`"Player"`, `"Enemy"` 같은 태그 문자열이나 `animator.SetFloat("InputX", ...)` 파라미터 이름은 오타가 나기 쉽습니다.
`static class Constants { public const string TAG_PLAYER = "Player"; }` 와 같이 상수로 관리하면 실수를 줄일 수 있습니다.

---
**작성자**: Antigravity AI
