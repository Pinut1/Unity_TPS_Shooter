using UnityEngine;
public class Player : MonoBehaviour
{
    [SerializeField]
    private Transform cameraTransform;

    private float moveSpeed = 3.0f;

    private CharacterController controller;

    private Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");  // A/D 또는 ←/→
        float v = Input.GetAxis("Vertical");    // W/S 또는 ↑/↓


        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 moveDir = (forward * v) + (right * h);

        float inputMagnitude = new Vector2(h, v).magnitude;

        if(!controller.isGrounded)
        {
            moveDir.y += -9.81f;
        }

        float speedMultiplier = Input.GetKey(KeyCode.LeftShift) ? 1.0f : 0.5f;

        float currentSpeed = inputMagnitude * speedMultiplier;

        animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
        animator.SetFloat("InputX", h);
        animator.SetFloat("InputY", v);
        controller.Move(moveDir * Time.deltaTime * moveSpeed * speedMultiplier);
        
        // 마우스 좌우 입력으로 플레이어 몸통 회전 (Y축)
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up * mouseX * 2.0f); // 감도 조절 필요 시 변수로 분리
    }
}
