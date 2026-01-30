using UnityEngine;

/// <summary>
/// 엄폐 포인트 - 엄폐물에 붙이는 컴포넌트
/// </summary>
public class CoverPoint : MonoBehaviour
{
    [Header("Cover Settings")]
    [SerializeField] private CoverType coverType = CoverType.Half;
    [SerializeField] private float coverLength = 2f;

    public enum CoverType
    {
        Half,   // 반 엄폐 (앉아서)
        Full    // 전체 엄폐 (서서)
    }

    // 프로퍼티
    public Vector3 CoverNormal => -transform.forward;
    public CoverType Type => coverType;
    public float Length => coverLength;

    /// <summary>
    /// 플레이어가 붙을 위치 계산
    /// </summary>
    public Vector3 GetSnapPosition(Vector3 playerPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(playerPosition);
        localPos.z = -0.5f;  // 엄폐물 뒤쪽
        localPos.x = Mathf.Clamp(localPos.x, -coverLength / 2, coverLength / 2);
        return transform.TransformPoint(localPos);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = coverType == CoverType.Full ? Color.green : Color.yellow;
        float height = coverType == CoverType.Full ? 2f : 1f;
        Gizmos.DrawWireCube(transform.position, new Vector3(coverLength, height, 0.2f));

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, CoverNormal * 1.5f);
    }
}
