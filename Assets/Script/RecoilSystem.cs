using UnityEngine;

public class RecoilSystem : MonoBehaviour
{
    // 1. 설정값 (에디터에서 조절)
    [Header("반동 설정")]
    public float recoilX = -2f;    // 위로 튀는 힘 (음수여야 위로 감!)
    public float recoilY = 0.5f;   // 좌우로 떨리는 힘
    public float recoilZ = 0.35f; // 화면 기울임 

    public float snappiness = 6f;  // 팍! 하고 튀는 속도
    public float returnSpeed = 2f; // 스르륵.. 돌아오는 속도

    // 2. 내부 변수
    private Vector3 currentRotation; // 실제 카메라에 적용될 회전값
    private Vector3 targetRotation;  // 목표 회전값

    void Update()
    {
        // A. 목표값 복구: targetRotation을 0(원위치)으로 서서히 돌려보냄
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);

        // B. 실제 회전 적용: current를 target으로 빠르게 이동 (스프링 효과)
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);

        // C. 카메라 돌리기: 이 스크립트가 x축 회전시킴
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void FireRecoil()
    {
        // 목표 회전값에 반동을 더함 (X는 고정, Y는 랜덤)
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
}