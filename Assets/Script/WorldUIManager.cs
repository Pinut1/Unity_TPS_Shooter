using UnityEngine;

public class WorldUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCameraControl playerCameraControl;
    [SerializeField] private Transform ammoCanvas;
    [SerializeField] private Transform healthUI; // e.g. LinearHealthBar root

    [Header("Settings")]
    [SerializeField] private Vector3 rightAmmoPos;
    [SerializeField] private Vector3 leftAmmoPos;
    [SerializeField] private Vector3 rightHealthPos;
    [SerializeField] private Vector3 leftHealthPos;

    [Header("Flip Settings")]
    [SerializeField] private bool flipAmmoUI = false;
    [SerializeField] private bool flipHealthUI = false;

    void Start()
    {
        if (playerCameraControl == null)
            playerCameraControl = FindAnyObjectByType<PlayerCameraControl>();

        if (playerCameraControl != null)
        {
            playerCameraControl.OnShoulderSwitch += HandleShoulderSwitch;
        }

        // Initialize (Assuming right side default)
        HandleShoulderSwitch(true);
    }

    void OnDestroy()
    {
        if (playerCameraControl != null)
        {
            playerCameraControl.OnShoulderSwitch -= HandleShoulderSwitch;
        }
    }

    void HandleShoulderSwitch(bool isRightSide)
    {
        // Move Ammo UI
        if (ammoCanvas != null)
        {
            ammoCanvas.localPosition = isRightSide ? rightAmmoPos : leftAmmoPos;
            if (flipAmmoUI)
            {
                Vector3 scale = ammoCanvas.localScale;
                scale.x = Mathf.Abs(scale.x) * (isRightSide ? 1 : -1);
                ammoCanvas.localScale = scale;
            }
        }

        // Move Health UI
        if (healthUI != null)
        {
            healthUI.localPosition = isRightSide ? rightHealthPos : leftHealthPos;
            if (flipHealthUI)
            {
                Vector3 scale = healthUI.localScale;
                scale.x = Mathf.Abs(scale.x) * (isRightSide ? 1 : -1);
                healthUI.localScale = scale;
            }
        }
    }
}
