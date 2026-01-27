using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinearHealthBar : MonoBehaviour
{

    [Header("설정")]
    public GameObject segmentPrefab; // 아까 만든 블럭 프리팹 (4x15)
    public int totalSegments = 20;   // 전체 칸 개수
    public Color emptyColor = new Color(1, 1, 1, 0.2f); // 비었을 때 (반투명)
    public float spacing = 6f;       // 블럭 사이 간격 (프리팹 너비보다 약간 커야 함)

    [Header("색상")]
    public Color fullColor = Color.white;
    public Color lowHealthColor = Color.red;

    private List<Image> segments = new List<Image>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateSegments()
    {
        Debug.Log("생성 생성");
        foreach (Transform chlid  in transform) Destroy(chlid.gameObject);
        segments.Clear();

        Debug.Log(segments.Count);
        
        float totalWidth = (totalSegments - 1 ) * spacing;
        float startX = -totalSegments / 2f;

        for (int i = 0; i < totalSegments; i++)
        {
            Debug.Log("i의 수는?" + i);
            GameObject go = Instantiate(segmentPrefab, transform);

            float posX = startX + (i * spacing);

            go.transform.localPosition = new Vector3(posX, 0, 0);
            go.transform.localRotation = Quaternion.identity;

            segments.Add(go.GetComponent<Image>());
        }

    }
    public void UpdateHealth(float currentHp, float maxHp)
    {
        float healthPercent = currentHp / maxHp;
        int activeSegment = Mathf.CeilToInt(totalSegments * healthPercent);

        for (int i = 0; i < totalSegments; i++)

        {
            if (i > activeSegment)
            {
                segments[i].color = (healthPercent >= 0.1f) ? emptyColor : fullColor;
            }
            

        }
    }
}
