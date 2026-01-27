using System;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCrosshair : MonoBehaviour
{

    public RectTransform top;
    public RectTransform bottom;
    public RectTransform left;
    public RectTransform right;
    public Image centerDot; 

    [Header("Settings")]
    [SerializeField] private float movingSpread = 25f;
    [SerializeField] private float restingSpread = 10f; 
    [SerializeField] private float fireSpread = 30f;
    [SerializeField] private float speed = 10f;

    [Header("Color")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color enemyColor = Color.red;
    
    private float currentSpread;
    private float targetSpread;
    private CharacterController playerController;
    private Camera mainCamera;

    bool isShooting = false;

    [SerializeField] private Image[] crosshairParts;

    public Image[] CrosshairParts { get => crosshairParts; set => crosshairParts = value; }

    void Start()
    {
        playerController = FindAnyObjectByType<CharacterController>();
        mainCamera = Camera.main;
        currentSpread = restingSpread;

        crosshairParts = GetComponentsInChildren<Image>();
    }

    void Update()
    {
        HandleSpread();
        CheckEnemyTarget();
        ApplyPosition();
    }

    private void ApplyPosition()
    {
        if(top) top.anchoredPosition = new Vector2(0, currentSpread);
        if (bottom) bottom.anchoredPosition = new Vector2(0, -currentSpread);
        if (left) left.anchoredPosition = new Vector2(-currentSpread, 0);
        if (right) right.anchoredPosition = new Vector2(currentSpread , 0);
    }

    void HandleSpread()
    {

        bool isVelocityHigh = playerController.velocity.magnitude > 2f;
        bool hasInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).sqrMagnitude > 0.01f;

        bool isMoving = isVelocityHigh && hasInput;

        float baseSpread = isMoving ? movingSpread : restingSpread;
        targetSpread = Mathf.Lerp(targetSpread, baseSpread, Time.deltaTime * speed);
        currentSpread = Mathf.Lerp(currentSpread, targetSpread, Time.deltaTime * speed);
    }

    void CheckEnemyTarget()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        bool isEnemy = false;

        // Optimization: Use LayerMask if possible, here using tag check first
        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                isEnemy = true;
            }
            else if (hit.collider.GetComponentInParent<Enemy>() != null)
            {
                isEnemy = true;
            }
        }
        Color targetColor = isEnemy ? enemyColor : normalColor;
        foreach(var img in crosshairParts)
        {
            img.color = targetColor;
        }
    }
    
    public void FireRecoil()
    {
        targetSpread += fireSpread;
    }
}
