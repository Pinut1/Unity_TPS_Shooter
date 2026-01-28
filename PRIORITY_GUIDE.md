# Google Antigravity 개발 우선순위 가이드 (Priority Guide)

Jules로서 분석한 결과, **Google Antigravity** 프로젝트(AI 에이전트 개발)를 성공적으로 진행하기 위해 권장하는 개발 우선순위는 다음과 같습니다.

## 1단계: 환경 안정성 확보 (Environment Stability)
**목표**: 에이전트가 학습하는 동안 게임이 멈추거나 크래시가 나지 않도록 합니다.
- **이유**: 강화학습(RL)은 수백만 번의 시뮬레이션을 반복합니다. 간헐적인 버그(예: `Zombie.cs`의 Null Reference)는 학습 전체를 중단시킬 수 있어 치명적입니다.
- **액션 아이템**:
    - `Zombie.cs`, `DynamicCrosshair.cs` 등 `CodeReview.md`에서 지적된 잠재적 오류 수정.
    - 메모리 누수 방지 (Object Pooling 적용).

## 2단계: 제어 인터페이스 구축 (Control Interface / Input Abstraction)
**목표**: AI가 사람처럼 게임을 조작할 수 있는 '손'을 만듭니다.
- **이유**: `InputManager`가 없으면 AI 모델의 출력을 게임 캐릭터의 움직임으로 연결하기 매우 어렵습니다.
- **액션 아이템**:
    - `InputManager` 완성 및 모든 캐릭터 컨트롤러(`PlayerMovement`, `WeaponController`)가 이를 참조하도록 리팩토링.
    - `SetAIInputs(vector, bool)` 메서드 검증.

## 3단계: 관측 API 정의 (Observation API)
**목표**: AI가 게임 상황을 이해할 수 있는 '눈'을 만듭니다.
- **이유**: 화면 픽셀(CNN)만으로 학습하는 것보다, 내부 변수(체력, 탄약, 적의 상대 위치)를 직접 제공하면 학습 속도가 획기적으로 빨라집니다.
- **액션 아이템**:
    - `IAgentInterface` 정의.
    - 레이캐스트(Raycast) 결과, 플레이어 상태, 적 리스트 정보를 반환하는 함수 구현.

## 4단계: 에피소드 관리 및 고속 시뮬레이션 (Episode Management)
**목표**: 학습 효율을 극대화합니다.
- **이유**: 에이전트가 죽거나 목표를 달성했을 때, 씬(Scene) 전체를 로딩하지 않고 즉시 상태를 초기화(Reset)해야 학습 시간을 단축할 수 있습니다.
- **액션 아이템**:
    - `GameManager`에 `ResetEpisode()` 기능 구현 (플레이어/적 위치 원위치, 스탯 초기화).
    - `Time.timeScale`을 높여도 물리 연산이 깨지지 않도록 `FixedUpdate` 로직 점검.

## 5단계: 보상 함수 설계 (Reward Function Design)
**목표**: AI가 올바른 행동을 하도록 유도합니다.
- **이유**: 적을 맞췄을 때(+), 총을 빗맞췄을 때(-), 피격당했을 때(-) 등 명확한 신호가 있어야 똑똑한 에이전트가 탄생합니다.
- **액션 아이템**:
    - 행동 별 보상 체계 기획 및 구현.

---
**요약**: "손(입력)과 눈(관측)을 먼저 만들고, 튼튼한 체력(안정성)을 기른 뒤, 빠르게 달리기(고속 시뮬레이션) 연습을 시키세요."
