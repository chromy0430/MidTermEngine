using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;      // �̵� �ӵ�
    [SerializeField] private float jumpSpeed = 8f;      // ���� �ӵ�
    [SerializeField] private float gravity = 20f;       // �߷� ���ӵ�
    [SerializeField] private float rotationSpeed = 10f; // ȸ�� �ӵ�

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
        // �Է� ó��
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // �̵� ���� ���
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // ĳ���� ȸ��
        if (inputDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // �̵� ó��
        if (controller.isGrounded)
        {
            yVelocity = 0f; // ���� �� ���� �ӵ� �ʱ�ȭ
            if (Input.GetButtonDown("Jump"))
            {
                yVelocity = jumpSpeed; // ����
            }
        }

        // �߷� ����
        yVelocity -= gravity * Time.deltaTime;
        moveDirection = transform.TransformDirection(inputDirection) * moveSpeed;
        moveDirection.y = yVelocity;

        // �̵� ����
        controller.Move(moveDirection * Time.deltaTime);
    }
}