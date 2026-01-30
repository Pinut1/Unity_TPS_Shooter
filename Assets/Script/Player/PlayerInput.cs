using UnityEngine;

/// <summary>
/// 플레이어 입력 처리 (SRP - 입력만 담당)
/// 기존 Input Manager 사용
/// </summary>
public class PlayerInput : MonoBehaviour
{
    // ========================================
    // 입력 값 프로퍼티 (읽기 전용)
    // ========================================

    /// <summary>이동 입력 (WASD)</summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>마우스 이동</summary>
    public Vector2 LookInput { get; private set; }

    /// <summary>발사 버튼 (좌클릭)</summary>
    public bool FireInputDown { get; private set; }
    public bool FireInputHeld { get; private set; }
    public bool FireInputUp { get; private set; }

    /// <summary>조준 버튼 (우클릭)</summary>
    public bool AimInputHeld { get; private set; }

    /// <summary>달리기 (Shift)</summary>
    public bool SprintInputHeld { get; private set; }

    /// <summary>엄폐 (Space)</summary>
    public bool CoverInputDown { get; private set; }

    /// <summary>재장전 (R)</summary>
    public bool ReloadInputDown { get; private set; }

    /// <summary>숄더 전환 (X)</summary>
    public bool ShoulderSwitchDown { get; private set; }

    /// <summary>무기 교체 (1, 2, 3...)</summary>
    public int WeaponSwitchIndex { get; private set; } = -1;

    // ========================================
    // 설정
    // ========================================
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;

    public float MouseSensitivity => mouseSensitivity;

    // ========================================
    // Unity 생명주기
    // ========================================
    private void Awake()
    {
        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // 이동 입력
        MoveInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        // 마우스 입력
        LookInput = new Vector2(
            Input.GetAxis("Mouse X") * mouseSensitivity,
            Input.GetAxis("Mouse Y") * mouseSensitivity
        );

        // 발사 (좌클릭)
        FireInputDown = Input.GetMouseButtonDown(0);
        FireInputHeld = Input.GetMouseButton(0);
        FireInputUp = Input.GetMouseButtonUp(0);

        // 조준 (우클릭)
        AimInputHeld = Input.GetMouseButton(1);

        // 달리기 (Shift)
        SprintInputHeld = Input.GetKey(KeyCode.LeftShift);

        // 엄폐 (Space)
        CoverInputDown = Input.GetKeyDown(KeyCode.Space);

        // 재장전 (R)
        ReloadInputDown = Input.GetKeyDown(KeyCode.R);

        // 숄더 전환 (X)
        ShoulderSwitchDown = Input.GetKeyDown(KeyCode.X);

        // 무기 교체 (1-4)
        WeaponSwitchIndex = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1)) WeaponSwitchIndex = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) WeaponSwitchIndex = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) WeaponSwitchIndex = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) WeaponSwitchIndex = 3;
    }

    /// <summary>
    /// 커서 잠금 상태 설정
    /// </summary>
    public void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
