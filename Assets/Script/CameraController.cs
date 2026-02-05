using UnityEngine;
public class CameraController : MonoBehaviour
{

    [SerializeField]
    private float mouseSensitivity = 2.0f;
    private float xRotation = 0f;
    private Camera _mainCamera;


    private float _defaultFov = 60f;
    private float _aimFov = 40f;

    [SerializeField]
    private Animator animator;

    void Start()
    {
        _mainCamera = GetComponentInChildren<Camera>(); 

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;    
    }

    // Update is called once per frame
    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY; 
        xRotation = Mathf.Clamp(xRotation, -30f, 30f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        OnAim();
    }

    public void OnAim()
    {
        float zoomSpeed = 3f;
        if(Input.GetMouseButton(1))
        {
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, _aimFov, Time.deltaTime * zoomSpeed);
            animator.SetBool("IsAiming", true);
        }
        else
        {
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, _defaultFov, Time.deltaTime * zoomSpeed);
            animator.SetBool("IsAiming", false);
        }
    }

}
