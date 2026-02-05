using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // 카메라와 같은 방향을 보게 함 (UI가 뒤집히지 않음)
            transform.rotation = mainCameraTransform.rotation;
        }
    }
}
