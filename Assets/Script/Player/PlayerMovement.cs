using UnityEngine;

/// <summary>
/// 플레이어 이동 처리 (SRP - 이동만 담당)
/// CharacterController 기반 이동 및 중력 처리
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // ========================================
    // 이동 속도 설정
    // ========================================
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float aimSpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // ========================================
    // 프로퍼티
    // ========================================
    public float WalkSpeed => walkSpeed;
    public float SprintSpeed => sprintSpeed;
    public float AimSpeed => aimSpeed;
    public bool IsGrounded => isGrounded;
    public Vector3 Velocity => velocity;

    // ========================================
    // 컴포넌트 및 상태
    // ========================================
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // ========================================
    // Unity 생명주기
    // ========================================
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        CheckGrounded();
        ApplyGravity();
    }

    // ========================================
    // 공개 메서드 (State에서 호출)
    // ========================================

    /// <summary>
    /// 카메라 기준으로 플레이어 이동
    /// </summary>
    public void Move(Vector2 input, float speed, Transform cameraTransform)
    {
        if (input.magnitude < 0.1f) return;

        // 카메라 방향 기준 이동
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * input.y + right * input.x).normalized;
        controller.Move(moveDirection * speed * Time.deltaTime);
    }

    /// <summary>
    /// 이동 방향으로 캐릭터 회전 (일반 이동 시)
    /// </summary>
    public void RotateTowardsMoveDirection(Vector2 input, Transform cameraTransform)
    {
        if (input.magnitude < 0.1f) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * input.y + right * input.x).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 카메라 방향으로 캐릭터 회전 (조준 시)
    /// </summary>
    public void RotateTowardsCamera(Transform cameraTransform)
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        if (cameraForward.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * 2f * Time.deltaTime);
        }
    }

    /// <summary>
    /// 엄폐물 방향으로 즉시 회전
    /// </summary>
    public void RotateToCover(Vector3 coverNormal)
    {
        if (coverNormal.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(-coverNormal);
        }
    }

    /// <summary>
    /// 위치 즉시 이동 (엄폐 진입 시)
    /// </summary>
    public void TeleportTo(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    // ========================================
    // 내부 메서드
    // ========================================
    private void CheckGrounded()
    {
        isGrounded = controller.isGrounded;

        // 추가 체크 (선택적)
        // isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundLayer);
    }

    private void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 약간 아래로 밀어서 확실히 땅에 붙게
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
