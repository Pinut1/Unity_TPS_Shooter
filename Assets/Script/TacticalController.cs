using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TacticalController : MonoBehaviour
{
    [Header("필수 설정")]
    [SerializeField] private Transform _playerModel; // 

    [Header("설정")]
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _runSpeed = 6f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("카메라 설정")]
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _minXAngle = -40f;
    [SerializeField] private float _maxXAngle = 60f;

    [Header("애니메이션")]
    [SerializeField] private Animator _animator;

    [Header("상체 회전 (Spine)")]
    [SerializeField] private Transform _upperBodyBone; // 꺾을 뼈 (Chest)
    [SerializeField] private Transform _headBone;

    public float minSpineAngle = -30f;
    public float maxSpineAngle = 30f;


    [Header("숄더뷰 설정")]
    [SerializeField] private float _rightOffset = 0.45f;
    [SerializeField] private float _leftOffset = -0.45f;
    [SerializeField] private float _switchSpeed = 5f;

    [Header("UI 조정")]
    public Transform _uiCanvas; // 총에 붙어있는 AmmoCanvas
    [SerializeField] private float _rightUiOffset = 0f;
    [SerializeField] private float _leftUiOffset = -3f;


    private bool _isRightSide = true;



    private CharacterController _controller;
    private Camera _mainCamera;
    private float _currentXRotation = 0f;
    private float _currentYRotation = 0f;
    private bool isAiming = false;


    // 초기 상태 모델값 , UI 스케일 값 저장
    private Vector3 _defaultModelScale;
    private Vector3 _defaultUiScale;
    void Start()
    {
        if (_playerModel != null) _defaultModelScale = _playerModel.localScale;
        if (_uiCanvas != null) _defaultUiScale = _uiCanvas.localScale;

        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;

        if (_animator == null) _animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {

        // aiming 상태 체크를 한 번만 깔끔하게 정리
        isAiming = Input.GetMouseButton(1);

        _animator.SetBool("isAiming", isAiming); // 대소문자 주의 (IsAiming)

        if (Input.GetKeyDown(KeyCode.X))
        {
            FlipCharacter();
        }
        HandleCameraLook();
        HandleMovement();
    }

    private void FlipCharacter()
    {
        _isRightSide = !_isRightSide;

        _playerModel.transform.localScale = _isRightSide ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);

        Debug.Log("X눌림");
    }


    void LateUpdate()
    {
        // 조준 중이고, 뼈를 찾았을 때만 실행
        if (isAiming && _upperBodyBone != null)
        {
            // A. 현재 마우스로 조절한 카메라 각도 (위/아래)
            float finalAngle = _currentYRotation;

            // B. [중요] 리코일 시스템이 만든 '반동 각도'도 가져오기
            // (카메라가 덜컹거릴 때 허리도 같이 덜컹거리게 함)
            float recoilAngle = _mainCamera.transform.localEulerAngles.x;

            // 유니티 각도 보정 (360도 넘어가는 것 방지)
            if (recoilAngle > 180) recoilAngle -= 360;

            // C. 최종 각도 합산 (마우스 각도 + 반동 충격)
            finalAngle += recoilAngle;

            // D. 뼈 돌리기
            // Vector3.right(빨간축)을 기준으로 회전
            _upperBodyBone.Rotate(Vector3.right * finalAngle);

            _headBone.Rotate(Vector3.up * 50f);

        }
    }

    // 카메라 핸들러
    void HandleCameraLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;
        float _smoothingSpeed = 10f; // 지역 변수 정의

        // 1. 좌우 회전: 플레이어 전체(Root)를 돌림 -> 카메라가 마우스 따라 돔
        transform.Rotate(Vector3.up * mouseX);


        // 2. 상하 회전: 피벗만 돌림
        _currentYRotation -= mouseY;
        _currentYRotation = Mathf.Clamp(_currentYRotation, _minXAngle, _maxXAngle);
        _cameraPivot.localRotation = Quaternion.Euler(_currentYRotation, 0f, 0f);

        // 3. 줌 로직 (삼항 연산자로 깔끔하게)
        float targetFOV = isAiming ? 40f : 60f;
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFOV, Time.deltaTime * _smoothingSpeed);



        // 카메라 어깨 위치 변경
        // 삼항 연산자로 isRightSide가 참 거짓인지
        // 참이면 rightOffset 으로 
        // 거짓이면 leftOffset 으로

        float targetX = _isRightSide ? _rightOffset : _leftOffset;

        // newPos 에서 현재 카메라의 포지션 받아옴
        Vector3 newPos = _mainCamera.transform.localPosition;

        //newPos.x 에 Lerp로 부드럽게 이동 하도록 연출
        newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * _switchSpeed);

        // _mainCamara local position에 newPos값 넣어줌
        _mainCamera.transform.localPosition = newPos;
    }

    // 움직임 핸들러
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool isCombatMode = Input.GetMouseButton(0) || isAiming;

        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? _runSpeed : _walkSpeed;

        if (inputDir.magnitude >= 0.1f)
        {
            // 1. 이동 방향 계산 (카메라 기준)
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // 2. 실제 이동 (Root를 이동시킴)
            _controller.Move(moveDir * currentSpeed * Time.deltaTime);

            if (!isCombatMode)
            {
                float yawCamera = _mainCamera.transform.eulerAngles.y;
                Quaternion targetRotation = Quaternion.LookRotation(moveDir.normalized);
                _playerModel.rotation = Quaternion.Slerp(_playerModel.rotation, targetRotation, _rotationSpeed * Time.deltaTime);


            }

            Quaternion aimRotation = Quaternion.Euler(0, _mainCamera.transform.eulerAngles.y, 0);
            _playerModel.rotation = Quaternion.Slerp(_playerModel.rotation, aimRotation, _rotationSpeed);


        }
        // 애니메이션 값 전달
        float animationSpeedPercent = 0f;
        if (inputDir.magnitude >= 0.1f)
        {
            animationSpeedPercent = isRunning ? 1f : 0.5f;
        }
        _animator.SetFloat("InputX", h, 0.1f, Time.deltaTime);
        _animator.SetFloat("InputY", v, 0.1f, Time.deltaTime);
        _animator.SetFloat("Speed", animationSpeedPercent, 0.1f, Time.deltaTime);
    }
}