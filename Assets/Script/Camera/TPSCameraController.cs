using UnityEngine;

/// <summary>
/// 디비전 스타일 TPS 카메라 컨트롤러
/// 어깨 밀착, 벽 충돌, 부드러운 추적
/// </summary>
public class TPSCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform shoulderPivot;  // 어깨 피봇 (선택)

    [Header("Normal Camera Position")]
    [Tooltip("어깨 가까이 붙는 오프셋")]
    [SerializeField] private Vector3 offset = new Vector3(0.5f, 0.3f, -1.8f);
    [SerializeField] private float followHeight = 1.5f;  // 어깨 높이

    [Header("Aim Camera Position")]
    [SerializeField] private Vector3 aimOffset = new Vector3(0.6f, 0.2f, -1.0f);
    [SerializeField] private float aimFollowHeight = 1.4f;

    [Header("Rotation")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private float rotationSmoothTime = 0.05f;

    [Header("Follow Smoothing")]
    [SerializeField] private float followSmoothTime = 0.1f;
    [SerializeField] private float aimFollowSmoothTime = 0.05f;  // 조준 시 더 빠르게

    [Header("FOV")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float aimFOV = 45f;
    [SerializeField] private float fovSmoothSpeed = 10f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionLayers = ~0;
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float collisionSmoothTime = 0.1f;

    [Header("Shoulder Switch")]
    [SerializeField] private KeyCode shoulderSwitchKey = KeyCode.Q;
    [SerializeField] private float shoulderSwitchSpeed = 8f;

    // 상태
    private float yaw;
    private float pitch;
    private bool isAiming;
    private bool isRightShoulder = true;
    private float currentShoulderSign = 1f;

    private Camera mainCamera;
    private Vector3 currentOffset;
    private float currentHeight;
    private float currentFOV;
    private float currentDistance;

    // 스무딩용
    private Vector3 velocityPosition;
    private float velocityDistance;
    private Vector2 rotationVelocity;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null) mainCamera = Camera.main;

        currentOffset = offset;
        currentHeight = followHeight;
        currentFOV = normalFOV;
        currentDistance = Mathf.Abs(offset.z);

        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 초기 회전
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        HandleRotation();
        HandlePosition();
        HandleCollision();
        HandleFOV();
    }

    private void HandleInput()
    {
        // 숄더 전환
        if (Input.GetKeyDown(shoulderSwitchKey))
        {
            SwitchShoulder();
        }
    }

    private void HandleRotation()
    {
        // 마우스 입력
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 목표 회전값
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 부드러운 회전 적용
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandlePosition()
    {
        // 목표 오프셋 계산
        Vector3 targetOffset = isAiming ? aimOffset : offset;
        float targetHeight = isAiming ? aimFollowHeight : followHeight;
        float smoothTime = isAiming ? aimFollowSmoothTime : followSmoothTime;

        // 숄더 방향 부드럽게 전환
        float targetShoulderSign = isRightShoulder ? 1f : -1f;
        currentShoulderSign = Mathf.Lerp(currentShoulderSign, targetShoulderSign, shoulderSwitchSpeed * Time.deltaTime);

        // 오프셋에 숄더 적용
        targetOffset.x = Mathf.Abs(targetOffset.x) * currentShoulderSign;

        // 현재 오프셋 부드럽게 이동
        currentOffset = Vector3.SmoothDamp(currentOffset, targetOffset, ref velocityPosition, smoothTime);
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, shoulderSwitchSpeed * Time.deltaTime);

        // 피봇 위치 계산 (어깨 높이)
        Vector3 pivotPosition = target.position + Vector3.up * currentHeight;

        // 카메라 위치 = 피봇 + 회전된 오프셋
        Vector3 rotatedOffset = transform.rotation * currentOffset;
        transform.position = pivotPosition + rotatedOffset;
    }

    private void HandleCollision()
    {
        if (target == null) return;

        // 피봇에서 카메라까지 레이캐스트
        Vector3 pivotPosition = target.position + Vector3.up * currentHeight;
        Vector3 directionToCamera = transform.position - pivotPosition;
        float desiredDistance = directionToCamera.magnitude;

        RaycastHit hit;
        if (Physics.SphereCast(pivotPosition, collisionRadius, directionToCamera.normalized,
            out hit, desiredDistance, collisionLayers))
        {
            // 벽에 닿으면 카메라를 앞으로 당김
            float hitDistance = hit.distance - collisionRadius;
            currentDistance = Mathf.SmoothDamp(currentDistance, hitDistance, ref velocityDistance, collisionSmoothTime);

            // 카메라 위치 조정
            transform.position = pivotPosition + directionToCamera.normalized * currentDistance;
        }
        else
        {
            // 충돌 없으면 원래 거리로 복귀
            currentDistance = Mathf.SmoothDamp(currentDistance, desiredDistance, ref velocityDistance, collisionSmoothTime);
        }
    }

    private void HandleFOV()
    {
        float targetFOV = isAiming ? aimFOV : normalFOV;
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, fovSmoothSpeed * Time.deltaTime);

        if (mainCamera != null)
        {
            mainCamera.fieldOfView = currentFOV;
        }
    }

    // ========================================
    // 공개 메서드
    // ========================================

    /// <summary>
    /// 조준 상태 설정
    /// </summary>
    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    /// <summary>
    /// 숄더 전환 (Q키)
    /// </summary>
    public void SwitchShoulder()
    {
        isRightShoulder = !isRightShoulder;
        Debug.Log($"[Camera] Shoulder: {(isRightShoulder ? "Right" : "Left")}");
    }

    /// <summary>
    /// 타겟 설정
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 현재 조준 상태
    /// </summary>
    public bool IsAiming => isAiming;

    /// <summary>
    /// 현재 숄더 방향 (true = 오른쪽)
    /// </summary>
    public bool IsRightShoulder => isRightShoulder;

    /// <summary>
    /// 카메라 Yaw (수평 회전)
    /// </summary>
    public float Yaw => yaw;

    /// <summary>
    /// 카메라 Pitch (수직 회전)
    /// </summary>
    public float Pitch => pitch;
}
