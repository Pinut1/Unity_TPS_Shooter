using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _mainCamera;
    private Vector3 _originalScale; // 원래 내 크기 (0.005 같은 거) 기억용

    void Start()
    {
        _mainCamera = Camera.main;
        // 게임 시작할 때 내 원래 크기를 기억해둠
        _originalScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (_mainCamera != null)
        {
            // 1. 회전: 일단 무조건 카메라랑 똑같이 바라봄
            transform.rotation = _mainCamera.transform.rotation;

            // 2. 거울 모드 자동 보정
            // 부모(총/몸)의 월드 스케일(lossyScale.x)이 음수면 뒤집힌 상태임
            // 부모가 음수일 때(-1), 나도 음수(-1)로 설정해야 곱해서 양수(+1)가 됨!

            // 부모의 X 스케일 방향(1 또는 -1)을 구함
            float parentDir = Mathf.Sign(transform.parent.lossyScale.x);

            // 내 크기 = 원래크기 * 부모방향
            // 부모가 정상이면 나도 정상, 부모가 반전이면 나도 반전시켜서 '정상화' 함
            transform.localScale = new Vector3(
                _originalScale.x * parentDir,
                _originalScale.y,
                _originalScale.z
            );
        }
    }
}