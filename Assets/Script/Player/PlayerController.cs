using UnityEngine;

/// <summary>
/// 메인 플레이어 컨트롤러 (FSM 관리자)
/// SRP 원칙: 상태 관리 및 컴포넌트 연결만 담당
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ========================================
    // 컴포넌트 참조 (Inspector에서 연결)
    // ========================================
    [Header("Required Components")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Optional Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private CoverDetector coverDetector;
    [SerializeField] private Transform cameraTransform;

    [Header("Weapon (연결 필요)")]
    [SerializeField] private WeaponController weaponController;

    [Header("Camera Controller (나중에 추가)")]
    [SerializeField] private TPSCameraController cameraController;

    // ========================================
    // 외부 접근용 프로퍼티
    // ========================================
    public PlayerInput Input => playerInput;
    public PlayerMovement Movement => playerMovement;
    public PlayerHealth Health => playerHealth;
    public Animator Animator => animator;
    public CoverDetector CoverDetector => coverDetector;
    public WeaponController WeaponController => weaponController;
    public TPSCameraController CameraController => cameraController;
    public Transform CameraTransform => cameraTransform;

    // ========================================
    // FSM (Finite State Machine)
    // ========================================
    public PlayerStateMachine StateMachine { get; private set; }

    // 모든 상태 인스턴스
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerSprintState SprintState { get; private set; }
    public PlayerAimState AimState { get; private set; }
    public PlayerShootState ShootState { get; private set; }
    public PlayerReloadState ReloadState { get; private set; }
    public PlayerCoverState CoverState { get; private set; }

    // ========================================
    // Unity 생명주기
    // ========================================
    private void Awake()
    {
        InitializeComponents();
        InitializeStateMachine();
    }

    private void Update()
    {
        StateMachine.CurrentState?.Update();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState?.FixedUpdate();
    }

    // ========================================
    // 초기화
    // ========================================
    private void InitializeComponents()
    {
        // 자동 획득
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (coverDetector == null) coverDetector = GetComponentInChildren<CoverDetector>();
        if (weaponController == null) weaponController = GetComponentInChildren<WeaponController>();
        if (cameraTransform == null) cameraTransform = Camera.main?.transform;

        // 검증
        if (playerInput == null) Debug.LogError("[PlayerController] PlayerInput이 없습니다!");
        if (playerMovement == null) Debug.LogError("[PlayerController] PlayerMovement가 없습니다!");
    }

    private void InitializeStateMachine()
    {
        StateMachine = new PlayerStateMachine();

        // 상태 생성
        IdleState = new PlayerIdleState(this, StateMachine);
        MoveState = new PlayerMoveState(this, StateMachine);
        SprintState = new PlayerSprintState(this, StateMachine);
        AimState = new PlayerAimState(this, StateMachine);
        ShootState = new PlayerShootState(this, StateMachine);
        ReloadState = new PlayerReloadState(this, StateMachine);
        CoverState = new PlayerCoverState(this, StateMachine);

        // 초기 상태: Idle
        StateMachine.Initialize(IdleState);
    }

    // ========================================
    // 헬퍼 메서드
    // ========================================

    /// <summary>
    /// 엄폐 진입 가능 여부
    /// </summary>
    public bool CanEnterCover()
    {
        return coverDetector != null && coverDetector.HasNearbyCover();
    }

    // ========================================
    // 디버그 (에디터에서만)
    // ========================================
#if UNITY_EDITOR
    private void OnGUI()
    {
        string stateName = StateMachine?.CurrentState?.GetType().Name ?? "None";
        GUI.Label(new Rect(10, 10, 300, 25), $"State: {stateName}");
    }
#endif
}
