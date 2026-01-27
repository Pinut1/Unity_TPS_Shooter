using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private TMP_Text currentAmmoText;
    [SerializeField] private TMP_Text spareAmmoText;
    [SerializeField] private Image reloadRingImage;
    [SerializeField] private GameObject crosshairObject; // To toggle during reload

    void Start()
    {
        if (weaponController == null) weaponController = FindAnyObjectByType<WeaponController>();

        if (weaponController != null)
        {
            weaponController.OnAmmoChanged += UpdateAmmoUI;
            weaponController.OnReloadStart += HandleReloadStart;
            weaponController.OnReloadComplete += HandleReloadComplete;
        }

        if (reloadRingImage != null) reloadRingImage.gameObject.SetActive(false);
    }

    void UpdateAmmoUI(int current, int spare)
    {
        if (currentAmmoText != null)
        {
            currentAmmoText.text = current.ToString();
            // Warning color if low ammo (30%)
            currentAmmoText.color = (current <= weaponController.maxAmmo * 0.3f) ? Color.red : Color.white;
        }
        if (spareAmmoText != null)
        {
            spareAmmoText.text = spare.ToString();
        }
    }

    void HandleReloadStart()
    {
        if (crosshairObject != null) crosshairObject.SetActive(false);
        if (reloadRingImage != null)
        {
            reloadRingImage.gameObject.SetActive(true);
            reloadRingImage.fillAmount = 0;
            StartCoroutine(ReloadAnimationRoutine());
        }
    }

    void HandleReloadComplete()
    {
        if (crosshairObject != null) crosshairObject.SetActive(true);
        if (reloadRingImage != null)
        {
            reloadRingImage.gameObject.SetActive(false);
        }
    }

    System.Collections.IEnumerator ReloadAnimationRoutine()
    {
        float duration = weaponController.reloadTime;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            reloadRingImage.fillAmount = elapsed / duration;
            yield return null;
        }
        reloadRingImage.fillAmount = 1f;
    }
}
