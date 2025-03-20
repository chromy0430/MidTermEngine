using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;      // 이동 속도
    [SerializeField] private float jumpSpeed = 8f;      // 점프 속도
    [SerializeField] private float gravity = 20f;       // 중력 가속도
    [SerializeField] private float rotationSpeed = 10f; // 회전 속도

    private CharacterController controller;
    private Vector3 moveDirection;
    private float yVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("Player must have a CharacterController component!");
        }
    }

    void Update()
    {
        // 입력 처리
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 이동 방향 계산
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 캐릭터 회전
        if (inputDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 이동 처리
        if (controller.isGrounded)
        {
            yVelocity = 0f; // 착지 시 수직 속도 초기화
            if (Input.GetButtonDown("Jump"))
            {
                yVelocity = jumpSpeed; // 점프
            }
        }

        // 중력 적용
        yVelocity -= gravity * Time.deltaTime;
        moveDirection = transform.TransformDirection(inputDirection) * moveSpeed;
        moveDirection.y = yVelocity;

        // 이동 실행
        controller.Move(moveDirection * Time.deltaTime);
    }
}