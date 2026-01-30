using UnityEngine;

/// <summary>
/// 무기 데이터 ScriptableObject
/// 무기 스탯을 데이터 파일로 관리
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "TPS/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName;
    public Sprite weaponIcon;
    public GameObject weaponPrefab;

    [Header("Combat Stats")]
    [Range(1, 100)] public float damage = 25f;
    [Range(0.05f, 2f)] public float fireRate = 0.1f;
    [Range(1, 100)] public int magazineSize = 30;
    [Range(0.5f, 5f)] public float reloadTime = 2f;
    [Range(0f, 1f)] public float accuracy = 0.8f;

    [Header("Range")]
    public float effectiveRange = 50f;
    public float maxRange = 100f;
    public float recoilAmount = 2f;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    [Header("Effects")]
    public GameObject muzzleFlashPrefab;
    public GameObject impactEffectPrefab;
}
