using UnityEngine;

public class TPScameraView : MonoBehaviour
{
    Camera _mainCamera;

    [Header("Settings")]
    float _defaultFOV = 60f;
    float _zoomFOV = 40f;
    float _zoomSpeed = 10f;

    [SerializeField] Vector3 _aimingPos = new Vector3(0.8f, 0f, -1.5f);
    private Vector3 _defaultPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        bool isAiming = Input.GetMouseButton(1);

        Vector3 targetPos = isAiming ? _aimingPos : _defaultPos;
        float targetFOV = isAiming ? _zoomFOV : _defaultFOV;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * _zoomSpeed);
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFOV, Time.deltaTime * _zoomSpeed);
    }
}
