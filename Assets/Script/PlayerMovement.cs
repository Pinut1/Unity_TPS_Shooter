using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _runSpeed = 6f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private Transform _playerModel;
    [SerializeField] private Animator _animator;
    private CharacterController _controller;
    private Camera _mainCamera;

    // State
    public bool isAiming = false;

    [Header("Spine Rotation")]
    [SerializeField] private Transform _upperBodyBone;
    [SerializeField] private Transform _headBone;
    private PlayerCameraControl _cameraControl;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
        _cameraControl = GetComponent<PlayerCameraControl>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
    }

    void LateUpdate()
    {
        if (isAiming && _upperBodyBone != null && _cameraControl != null)
        {
            float finalAngle = _cameraControl.Pitch;

            // Recoil compensation (optional, if you want the body to shake with recoil)
            // float recoilAngle = _mainCamera.transform.localEulerAngles.x;
            // if (recoilAngle > 180) recoilAngle -= 360;
            // finalAngle += recoilAngle;

            _upperBodyBone.Rotate(Vector3.right * finalAngle);
            if (_headBone != null) _headBone.Rotate(Vector3.up * 50f); // Offset check
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Aiming state is set externally or here?
        // Let's assume input here for now
        isAiming = Input.GetMouseButton(1);
        _animator.SetBool("isAiming", isAiming);

        bool isCombatMode = Input.GetMouseButton(0) || isAiming;

        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? _runSpeed : _walkSpeed;

        if (inputDir.magnitude >= 0.1f)
        {
            // Move relative to Camera
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            _controller.Move(moveDir * currentSpeed * Time.deltaTime);

            // Rotation Logic
            if (!isCombatMode)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir.normalized);
                _playerModel.rotation = Quaternion.Slerp(_playerModel.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
            else
            {
                Quaternion aimRotation = Quaternion.Euler(0, _mainCamera.transform.eulerAngles.y, 0);
                _playerModel.rotation = Quaternion.Slerp(_playerModel.rotation, aimRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        // Animation
        float animationSpeedPercent = 0f;
        if (inputDir.magnitude >= 0.1f)
        {
            animationSpeedPercent = isRunning ? 1f : 0.5f;
        }
        _animator.SetFloat("InputX", h, 0.1f, Time.deltaTime);
        _animator.SetFloat("InputY", v, 0.1f, Time.deltaTime);
        _animator.SetFloat("Speed", animationSpeedPercent, 0.1f, Time.deltaTime);
    }

    public void FlipCharacter(bool isRightSide)
    {
        _playerModel.transform.localScale = isRightSide ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
    }
}
