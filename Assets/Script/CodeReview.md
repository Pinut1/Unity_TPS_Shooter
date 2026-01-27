# Code Review Report

## 1. 개요 (Overview)
현재 프로젝트의 `Assets\Script` 폴더에 있는 C# 스크립트들을 분석하였습니다. 전반적으로 기능 구현에 초점이 맞춰져 있으며, 구조적인 개선과 최적화가 필요한 부분들이 확인되었습니다.

## 2. 공통 사항 (General Issues)

### 2.1 네임스페이스 (Namespaces) 누락
모든 스크립트가 전역 네임스페이스를 사용하고 있습니다. 프로젝트 규모가 커질 경우 클래스 이름 충돌이 발생할 수 있으므로, 적절한 네임스페이스(예: `TPSShooter.Scripts`)로 감싸는 것을 권장합니다.

### 2.2 디버그 로그 (Debug.Log) 잔재
`Enemy.cs`, `LinearHealthBar.cs`, `PlayerHealth.cs`, `WeaponSystem.cs` 등 다수의 파일에 `Debug.Log`가 남아있습니다. 빌드 시 성능 저하의 원인이 될 수 있으므로, 제거하거나 `Debug.Assert` 혹은 조건부 컴파일(`[Conditional("DEBUG")]`)을 사용하는 것이 좋습니다.

### 2.3 문자열 인코딩 (String Encoding)
주석이 일부 깨져 보이는 현상이 있습니다. 소스 파일의 인코딩을 **UTF-8 with BOM** 또는 **UTF-8**로 통일하여 주석 가독성을 확보해야 합니다.

### 2.4 하드코딩된 입력 및 설정
`Input.GetMouseButton(1)`이나 태그 문자열("Player") 등이 하드코딩되어 있습니다. Unity의 새로운 Input System을 도입하거나, 상수 클래스(`Colsntants.cs`)를 만들어 관리하는 것이 유지보수에 좋습니다.

## 3. 파일별 상세 분석 (Detailed Analysis)

### `Billboard.cs`
- **클래스명 불일치**: 파일명은 `Billboard.cs`인데 클래스명은 `UIBillboard`입니다. 이래도 동작은 하지만, Unity에서는 파일명과 클래스명을 일치시키는 것이 관례입니다.
- **최적화**: `Camera.main`은 내부적으로 `FindGameObjectsWithTag`와 유사한 비용이 발생할 수 있으므로, `Start`에서 캐싱하여 사용하는 현재 방식은 좋습니다. 다만, 런타임에 메인 카메라가 변경될 경우 갱신되지 않는 점은 유의해야 합니다.

### `DynamicCrosshair.cs`
- **비효율적 Raycast**: `CheckEnemyTarget` 함수에서 매 프레임 `Physics.Raycast`를 호출하고 `GetComponentInParent<Enemy>()`를 수행합니다.
    - **개선**: Raycast의 `LayerMask`를 활용하여 연산 대상을 줄이고, `GetComponent` 호출을 최소화해야 합니다.
- **오타**: `bool isEnenmy` -> `isEnemy`.

### `Enemy.cs` / `SandBag_Enemy.cs` / `Zombie.cs`
- **구조**: `Enemy`를 상속받아 구현한 구조는 좋습니다.
- **`Zombie.cs` 잠재적 버그**:
  ```csharp
  if (agent == null) // agent가 없으면?
  {
      GameObject player = GameObject.FindGameObjectWithTag("Player");
      if (player == null) // player가 없으면?
      {
          target = player.transform; // NullReferenceException 발생 가능!
      }
  }
  ```
  - 위 로직은 `player`가 `null`일 때 `transform`에 접근하려 하므로 오류가 발생합니다. `if (player != null)`이어야 하며, `agent`가 null인지 체크하는 의도가 `target`을 찾기 위함인지 불분명합니다. 아마 `if (target == null)` 의도였을 가능성이 높습니다.
- **`SandBag_Enemy.cs`**: `Update` 메서드가 비어있는데 `base.Update()`를 호출하고 있습니다. 불필요한 호출이므로 제거해도 됩니다.

### `LinearHealthBar.cs`
- **Object Management**: `CreateSegments`에서 기존 오브젝트를 파괴하고 다시 생성합니다. 체력이 자주 변하거나 최대 체력이 자주 바뀌지 않는다면 초기화 시에만 호출하는 것이 좋습니다.
- **오타**: `foreach (Transform chlid ...)` -> `child`.

### `TPScameraView.cs`
- **클래스명 불일치**: 파일명 `TPScameraView.cs`, 클래스명 `TPS`. 일치시키는 것이 좋습니다.
- **Input 처리**: `Input.GetMouseButton(1)`이 하드코딩되어 있어 키 변경이 어렵습니다.

### `WeaponSystem.cs`
- **UI 업데이트 빈도**: `UpdateAmmoUI`가 `Update` 문 안에서 매 프레임 호출될 가능성이 있습니다(물론 조건문이 없으면). 탄약 수가 변경될 때만(이벤트 방식이나 setter 활용) UI를 갱신하는 것이 성능상 유리합니다.
- **메모리 할당**: `Instantiate(hitEffectPrefab, ...)`로 이펙트를 생성하고 `Destroy` 합니다. 총알 발사가 빈번하다면 **Object Pooling**을 적용하여 가비지 컬렉션(GC) 부하를 줄여야 합니다.

### `PlayerRayCast.cs`
- **빈 클래스**: 내용이 비어있습니다. 불필요하다면 삭제하세요.

## 4. 개선 제안 (Recommendations)

1.  **Object Pooling 적용**: 총알 이펙트(`hitEffectPrefab`)와 같은 빈번한 생성/파괴 객체에 풀링 시스템 도입.
2.  **Manager 클래스 도입**: 사운드, 이펙트 등을 전역에서 관리하는 매니저 클래스를 두어 의존성 분리.
3.  **UI 최적화**: `Update`에서 매 프레임 UI를 갱신하지 말고, 데이터가 변경될 때만 갱신하도록 이벤트 기반으로 변경.
4.  **코드 정리**: 오타 수정, 미사용 변수 제거, 파일명-클래스명 일치 작업.
5.  **버그 수정**: `Zombie.cs`의 타겟 탐색 로직 수정 필수.

---
**작성자**: Antigravity AI
**일시**: 2025-01-27
