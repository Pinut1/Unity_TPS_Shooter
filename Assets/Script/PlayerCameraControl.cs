using UnityEngine;

public class PlayerCameraControl : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _minXAngle = -40f;
    [SerializeField] private float _maxXAngle = 60f;

    [Header("Switch Shoulder")]
    [SerializeField] private float _rightOffset = 0.45f;
    [SerializeField] private float _leftOffset = -0.45f;
    [SerializeField] private float _switchSpeed = 5f;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    private Camera _mainCamera;
    private float _currentXRotation = 0f;
    public float Pitch => _currentYRotation; // Expose for Spine Rotation
    private float _currentYRotation = 0f;
    private bool _isRightSide = true;

    public event System.Action<bool> OnShoulderSwitch;

    void Start()
    {
        _mainCamera = Camera.main;
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            _isRightSide = !_isRightSide;
            if (playerMovement != null) playerMovement.FlipCharacter(_isRightSide);
            OnShoulderSwitch?.Invoke(_isRightSide);
        }

        HandleCameraLook();
    }

    void HandleCameraLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;
        float _smoothingSpeed = 10f;

        // 1. Horizontal Rotation (Rotate Body)
        // Note: Usually transform.Rotate is on the root object. 
        // If this script is on the Player Root, transform.Rotate is correct.
        transform.Rotate(Vector3.up * mouseX);

        // 2. Vertical Rotation (Rotate Pivot)
        _currentYRotation -= mouseY;
        _currentYRotation = Mathf.Clamp(_currentYRotation, _minXAngle, _maxXAngle);
        if (_cameraPivot != null)
        {
            _cameraPivot.localRotation = Quaternion.Euler(_currentYRotation, 0f, 0f);
        }

        // 3. FOV Zoom
        // Dependent on PlayerMovement's aiming state?
        bool isAiming = (playerMovement != null && playerMovement.isAiming);
        float targetFOV = isAiming ? 40f : 60f;
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFOV, Time.deltaTime * _smoothingSpeed);

        // 4. Shoulder Switch
        float targetX = _isRightSide ? _rightOffset : _leftOffset;
        Vector3 newPos = _mainCamera.transform.localPosition;
        newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * _switchSpeed);
        _mainCamera.transform.localPosition = newPos;
    }
}
