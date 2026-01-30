using UnityEngine;

/// <summary>
/// 카메라 Pitch에 따라 Spine 본을 회전시켜 허리를 꺾는 효과
/// LateUpdate에서 애니메이션 후에 적용
/// </summary>
public class SpineIKController : MonoBehaviour
{
    [Header("Spine References")]
    [Tooltip("Spine 또는 Spine1 본을 연결")]
    [SerializeField] private Transform spineTransform;
    [SerializeField] private Transform spine2Transform;  // 선택: 더 자연스러운 꺾임

    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;

    [Header("Settings")]
    [SerializeField] private float maxAngle = 30f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool onlyWhenAiming = true;

    [Header("State")]
    [SerializeField] private bool isAiming;

    private float currentAngle;
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();

        // 카메라 자동 찾기
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
        }

        // Spine 본 자동 찾기 (없으면 수동 설정 필요)
        if (spineTransform == null)
        {
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                spineTransform = animator.GetBoneTransform(HumanBodyBones.Spine);
                spine2Transform = animator.GetBoneTransform(HumanBodyBones.Chest);
            }
        }
    }

    private void LateUpdate()
    {
        if (spineTransform == null || cameraTransform == null) return;

        // 조준 상태 확인
        if (playerController != null)
        {
            isAiming = playerController.Input?.AimInputHeld ?? false;
        }

        // 조준 중에만 적용 옵션
        if (onlyWhenAiming && !isAiming)
        {
            // 원래 상태로 부드럽게 복귀
            currentAngle = Mathf.Lerp(currentAngle, 0f, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // 카메라 Pitch 가져오기
            float cameraPitch = cameraTransform.eulerAngles.x;

            // 0~360 → -180~180 변환
            if (cameraPitch > 180f) cameraPitch -= 360f;

            // 각도 제한 및 반전 (카메라가 아래 보면 허리가 앞으로)
            float targetAngle = Mathf.Clamp(-cameraPitch, -maxAngle, maxAngle);

            // 부드럽게 적용
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        }

        // Spine에 로컬 회전 적용
        ApplySpineRotation();
    }

    private void ApplySpineRotation()
    {
        // 첫 번째 Spine: 전체 각도의 60%
        if (spineTransform != null)
        {
            spineTransform.localRotation *= Quaternion.Euler(currentAngle * 0.6f, 0, 0);
        }

        // 두 번째 Spine(Chest): 나머지 40% (더 자연스러움)
        if (spine2Transform != null)
        {
            spine2Transform.localRotation *= Quaternion.Euler(currentAngle * 0.4f, 0, 0);
        }
    }

    /// <summary>
    /// 외부에서 조준 상태 설정 (PlayerController 없이 사용 시)
    /// </summary>
    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }
}
