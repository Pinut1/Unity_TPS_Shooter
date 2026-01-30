# UI 회전 버그 수정 리포트 (UI Rotation Bug Fix Report)

## 1. 문제 현상 (Issue)
**설명**: 정조준(ADS) 시에는 UI가 정상적으로 고정되어 보이지만, 정조준을 해제하고 이동하거나 시점을 돌리면 UI가 플레이어 주변을 빙빙 돌거나 흔들리는 현상이 발생함.

## 2. 원인 분석 (Root Cause)
이전 코드에서는 UI의 회전을 `Camera.main.transform.rotation` (카메라 회전)에 동기화했습니다.

1.  **정조준 (ADS) 상태**:
    *   플레이어 캐릭터가 카메라가 보는 방향을 바라봅니다. (`Rotation` 동기화됨)
    *   따라서 카메라와 플레이어의 회전값이 거의 일치하여 UI가 정상적으로 보입니다.

2.  **정조준 해제 (Free Look) 상태**:
    *   플레이어 캐릭터는 **이동 방향**을 바라보거나 제자리에 멈춰있습니다.
    *   반면 **카메라**는 마우스 입력에 따라 자유롭게 회전합니다.
    *   UI는 카메라 회전을 따라가지만, 위치는 플레이어(부모 오브젝트)에 종속되어 있습니다.
    *   결과적으로 **"몸체(위치)"는 가만히 있는데 "고개(회전)"만 카메라를 따라 억지로 돌리려다 보니**, UI가 캐릭터 주변을 공전하는 것처럼 보이게 됩니다.

## 3. 해결 방법 (Solution)
**수정 내용**: UI의 회전 기준을 `Camera`가 아닌 **`PlayerMovement` (플레이어 몸체)**로 변경했습니다.

```csharp
// WorldUIManager.cs 수정 코드
void LateUpdate()
{
    if (playerMovement != null)
    {
        // Fix: UI가 카메라 대신 플레이어의 몸체 회전을 따라가도록 변경
        // 이렇게 하면 캐릭터가 도는 대로 UI도 자연스럽게 따라옵니다.
        if (ammoCanvasRect != null)
        {
            ammoCanvasRect.rotation = playerMovement.transform.rotation;
        }
        if (healthUIRect != null)
        {
            healthUIRect.rotation = playerMovement.transform.rotation;
        }
    }
}
```

## 4. 적용 결과 (Result)
*   이제 UI는 항상 **플레이어 캐릭터의 등(Back)**을 기준으로 정렬됩니다.
*   플레이어가 몸을 돌리면 UI도 같이 돕니다.
*   카메라만 돌릴 때는 UI가 플레이어와 함께 화면에서 자연스럽게 멀어지거나 가까워지며, 억지스러운 회전 현상이 사라집니다.

---
**작성자**: Antigravity AI
**수정일**: 2026-01-28
