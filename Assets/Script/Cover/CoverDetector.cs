using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 엄폐물 감지 - 플레이어에 붙이는 컴포넌트
/// </summary>
public class CoverDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask coverLayer;

    private List<CoverPoint> nearbyCoverPoints = new List<CoverPoint>();
    private CoverPoint currentCover;

    public bool IsInCover => currentCover != null;

    private void OnTriggerEnter(Collider other)
    {
        CoverPoint cover = other.GetComponent<CoverPoint>();
        if (cover != null && !nearbyCoverPoints.Contains(cover))
        {
            nearbyCoverPoints.Add(cover);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CoverPoint cover = other.GetComponent<CoverPoint>();
        if (cover != null)
        {
            nearbyCoverPoints.Remove(cover);
        }
    }

    /// <summary>
    /// 근처에 엄폐물 있는지
    /// </summary>
    public bool HasNearbyCover()
    {
        return nearbyCoverPoints.Count > 0;
    }

    /// <summary>
    /// 가장 가까운 엄폐물 반환
    /// </summary>
    public CoverPoint GetNearestCover()
    {
        if (nearbyCoverPoints.Count == 0) return null;

        CoverPoint nearest = null;
        float minDistance = float.MaxValue;

        foreach (var cover in nearbyCoverPoints)
        {
            float distance = Vector3.Distance(transform.position, cover.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = cover;
            }
        }

        currentCover = nearest;
        return nearest;
    }

    /// <summary>
    /// 현재 엄폐 해제
    /// </summary>
    public void LeaveCover()
    {
        currentCover = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
