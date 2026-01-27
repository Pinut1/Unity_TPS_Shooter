using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitIndicator : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image hitMarkerImage;
    [SerializeField] private float duration = 0.1f; // 얼마나 보여줄지 (짧을수록 좋음)

  /*  [Header("사운드")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound; // '틱' 소리
  */
    void Start()
    {
        hitMarkerImage.enabled = false; 
    }

    public void ShowHit()
    {
        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        hitMarkerImage.enabled=true;

        yield return new WaitForSeconds(duration);
        
        hitMarkerImage.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
