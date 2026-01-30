# 엄폐 시스템 (Cover System) 구현 가이드

## 1. 개요 (Overview)

엄폐 시스템은 플레이어가 **스페이스바**를 눌러 벽이나 장애물 뒤에 숨을 수 있는 기능입니다. TPS 게임에서 전투의 전략성을 높이는 핵심 메커니즘입니다.

### 주요 기능
- **엄폐 감지**: 플레이어 주변의 엄폐 가능한 오브젝트 탐지
- **엄폐 진입/해제**: 스페이스바로 엄폐 상태 전환
- **카메라 조정**: 엄폐 시 3인칭 시점 변경
- **애니메이션 연동**: 엄폐 자세 애니메이션 재생

---

## 2. 핵심 개념 (Key Concepts)

### 2.1 엄폐 감지 (Cover Detection)
**방법 1: Raycast 방식**
- 플레이어 전방에 Raycast를 쏴서 벽 감지
- 장점: 동적, 모든 오브젝트에 적용 가능
- 단점: 정확한 엄폐 위치 계산 필요

**방법 2: Trigger Collider 방식** (권장)
- 엄폐 가능한 오브젝트에 `CoverPoint` 컴포넌트 부착
- 플레이어가 범위 내 진입 시 UI 표시
- 장점: 정확한 위치 제어, 디자이너 친화적
- 단점: 수동 배치 필요

### 2.2 엄폐 상태 (Cover State)
```
Normal → InCover → Normal
```
- **Normal**: 일반 이동/전투
- **InCover**: 엄폐 중 (이동 제한, 카메라 변경)

### 2.3 카메라 처리
- 엄폐 시 카메라를 플레이어 어깨 너머로 당김
- FOV 약간 확대하여 주변 시야 확보

---

## 3. 구현 코드 (Implementation)

### 3.1 CoverPoint.cs (엄폐 지점 마커)
```csharp
using UnityEngine;

public class CoverPoint : MonoBehaviour
{
    [Header("Cover Settings")]
    public Transform coverPosition; // 플레이어가 붙을 위치
    public Vector3 coverDirection;  // 엄폐물을 바라보는 방향
    
    [Header("Visual")]
    public bool showGizmo = true;
    
    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        Gizmos.color = Color.green;
        Vector3 pos = coverPosition ? coverPosition.position : transform.position;
        Gizmos.DrawWireSphere(pos, 0.3f);
        Gizmos.DrawRay(pos, coverDirection.normalized, 1f);
    }
}
```

### 3.2 CoverController.cs (플레이어 엄폐 로직)
```csharp
using UnityEngine;
using System.Collections.Generic;

public class CoverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator animator;
    
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask coverLayer;
    
    [Header("Cover State")]
    public bool isInCover = false;
    private CoverPoint currentCoverPoint;
    private List<CoverPoint> nearbyCoverPoints = new List<CoverPoint>();

    void Update()
    {
        DetectNearbyCover();
        HandleCoverInput();
    }

    void DetectNearbyCover()
    {
        nearbyCoverPoints.Clear();
        
        // 주변 CoverPoint 찾기
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, coverLayer);
        foreach (var col in colliders)
        {
            CoverPoint cp = col.GetComponent<CoverPoint>();
            if (cp != null) nearbyCoverPoints.Add(cp);
        }
        
        // 가장 가까운 CoverPoint 선택
        if (nearbyCoverPoints.Count > 0)
        {
            currentCoverPoint = GetClosestCoverPoint();
        }
        else
        {
            currentCoverPoint = null;
        }
    }

    CoverPoint GetClosestCoverPoint()
    {
        CoverPoint closest = null;
        float minDist = Mathf.Infinity;
        
        foreach (var cp in nearbyCoverPoints)
        {
            float dist = Vector3.Distance(transform.position, cp.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = cp;
            }
        }
        return closest;
    }

    void HandleCoverInput()
    {
        // 스페이스바 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isInCover && currentCoverPoint != null)
            {
                EnterCover(currentCoverPoint);
            }
            else if (isInCover)
            {
                ExitCover();
            }
        }
    }

    void EnterCover(CoverPoint coverPoint)
    {
        isInCover = true;
        
        // 위치 이동
        if (coverPoint.coverPosition != null)
        {
            transform.position = coverPoint.coverPosition.position;
        }
        
        // 방향 설정
        transform.forward = coverPoint.coverDirection.normalized;
        
        // 애니메이션
        if (animator != null)
        {
            animator.SetBool("InCover", true);
        }
        
        // 이동 제한
        if (playerMovement != null)
        {
            playerMovement.enabled = false; // 또는 이동 속도 0으로
        }
        
        Debug.Log("Entered Cover");
    }

    void ExitCover()
    {
        isInCover = false;
        
        // 애니메이션
        if (animator != null)
        {
            animator.SetBool("InCover", false);
        }
        
        // 이동 복구
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        Debug.Log("Exited Cover");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
```

---

## 4. Unity 설정 단계 (Setup Steps)

### 4.1 Layer 설정
1.  **Edit → Project Settings → Tags and Layers**
2.  새 Layer 추가: `Cover`
3.  엄폐 가능한 오브젝트(벽, 상자 등)를 `Cover` 레이어로 설정

### 4.2 CoverPoint 배치
1.  엄폐물(벽) 근처에 **빈 GameObject** 생성 (이름: `CoverPoint_01`)
2.  `CoverPoint` 컴포넌트 추가
3.  **Cover Position**: 플레이어가 붙을 위치 (자식 Transform 생성 권장)
4.  **Cover Direction**: 엄폐물을 바라보는 방향 벡터 설정 (예: `Vector3.forward`)
5.  Collider 추가 (Sphere Collider, Trigger 체크, Radius: 1.5)
6.  Layer를 `Cover`로 설정

### 4.3 플레이어 설정
1.  Player GameObject에 `CoverController` 컴포넌트 추가
2.  Inspector에서 설정:
    *   **Character Controller**: 플레이어의 CharacterController
    *   **Player Movement**: PlayerMovement 스크립트
    *   **Animator**: Animator 컴포넌트
    *   **Detection Radius**: `2.0`
    *   **Cover Layer**: `Cover` 선택

### 4.4 애니메이션 설정
1.  Animator Controller에 **Bool 파라미터** `InCover` 추가
2.  **Idle → Cover** Transition 생성
    *   Condition: `InCover == true`
3.  **Cover → Idle** Transition 생성
    *   Condition: `InCover == false`

---

## 5. 이해해야 할 핵심 개념 (Key Takeaways)

### 5.1 Physics.OverlapSphere
```csharp
Collider[] colliders = Physics.OverlapSphere(center, radius, layerMask);
```
- **역할**: 특정 위치 주변의 Collider들을 탐지
- **사용 이유**: 플레이어 근처의 엄폐 지점을 찾기 위해
- **LayerMask**: 특정 레이어만 검색하여 성능 최적화

### 5.2 Transform.forward와 방향 설정
```csharp
transform.forward = coverPoint.coverDirection.normalized;
```
- **역할**: 오브젝트가 바라보는 방향 설정
- **normalized**: 벡터를 단위 벡터로 변환 (크기 1)
- **사용 이유**: 엄폐 시 플레이어가 벽을 향하도록

### 5.3 State 관리
```csharp
public bool isInCover = false;
```
- **역할**: 현재 엄폐 상태 추적
- **사용처**: 다른 스크립트에서 참조 (예: 이동 제한, 사격 제한)
- **확장**: Enum으로 변경 가능 (`Normal`, `InCover`, `Peeking`)

### 5.4 Component 간 통신
```csharp
playerMovement.enabled = false;
```
- **역할**: 다른 컴포넌트의 동작 제어
- **주의**: `enabled = false`는 Update 등을 멈추지만, 변수는 접근 가능
- **대안**: Public 메서드 호출 (예: `playerMovement.SetCoverMode(true)`)

---

## 6. 추가 개선 사항 (Advanced Features)

### 6.1 엄폐 중 이동
- 엄폐 상태에서 좌우로 슬라이딩 이동 가능
- `Input.GetAxis("Horizontal")`로 감지
- 엄폐물을 따라 이동 (Raycast로 벽 추적)

### 6.2 Peek (엿보기)
- 마우스 우클릭으로 엄폐물 위로 고개 내밀기
- 카메라 Y축 올리고, 상체만 노출

### 6.3 UI 표시
- 엄폐 가능한 지점 근처에 "Press SPACE to Take Cover" UI 표시
- `currentCoverPoint != null`일 때 활성화

### 6.4 엄폐 중 사격
- 엄폐 상태에서도 조준/사격 가능하도록
- `WeaponController`에서 `CoverController.isInCover` 체크

---

## 7. 디버깅 팁 (Debugging Tips)

### 7.1 Gizmo 활용
- Scene View에서 `CoverPoint`의 위치와 방향을 시각적으로 확인
- `OnDrawGizmos`로 감지 범위 표시

### 7.2 Console 로그
```csharp
Debug.Log($"Nearby Cover Points: {nearbyCoverPoints.Count}");
```
- 엄폐 지점이 제대로 감지되는지 확인

### 7.3 Inspector 확인
- Play Mode에서 `CoverController`의 `isInCover` 값 실시간 확인
- `currentCoverPoint` 참조가 올바른지 체크

---

## 8. 참고 자료 (References)

- **Unity Physics.OverlapSphere**: [공식 문서](https://docs.unity3d.com/ScriptReference/Physics.OverlapSphere.html)
- **CharacterController**: [공식 문서](https://docs.unity3d.com/ScriptReference/CharacterController.html)
- **Animator Parameters**: [공식 문서](https://docs.unity3d.com/Manual/AnimationParameters.html)

---

**작성자**: Antigravity AI  
**작성일**: 2026-01-27
