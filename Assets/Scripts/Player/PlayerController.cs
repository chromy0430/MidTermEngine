using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    // 이동 관련 변수
    public float walkSpeed = 5f;  // 걷기 속도 (Walk_N에 맞춤)
    public float runSpeed = 10f;  // 달리기 속도 (Run_N에 맞춤)
    private float currentSpeed;   // 현재 속도
    public float gravity = -9.81f;
    public float jumpForce = 5f;
    private float yVelocity = 0f;
    Vector3 movement;

    // 회전 관련 변수
    public float rotationSpeed = 10f;
    public float mouseSensitivity = 1000f;

    // 애니메이션 관련 변수
    private bool isGrounded;
    private bool isFreeFalling;
    private float lastMotionSpeed;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip landingAudioClip;   // 착지 소리
    [SerializeField] private AudioClip[] footstepAudioClips; // 발소리 배열
    [Range(0, 1)][SerializeField] private float footstepAudioVolume = 0.5f; // 발소리 볼륨

    void Start()
    {
        TryGetComponent<CharacterController>(out controller);
        TryGetComponent<Animator>(out animator);
        // 커서 잠금 (선택사항)
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        KeyInput();
        MouseInput();
        MotionSpeed();
        Jump();
        GroundedCheck();

        // 이동 적용
        if (movement.magnitude >= 0.1f)
        {
            Vector3 moveDirection = transform.TransformDirection(movement) * currentSpeed;
            moveDirection.y = yVelocity;
            controller.Move(moveDirection * Time.deltaTime);
            animator.SetBool("Moving", true);
        }
        else
        {
            controller.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
            animator.SetBool("Moving", false);
        }
    }

    private void KeyInput()
    {
        // 이동 처리
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movement = new Vector3(horizontal, 0f, vertical).normalized;

        // Shift 키로 달리기 여부 확인
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;
    }

    private void MouseInput()
    {
        if (Input.GetButtonDown("Fire1")) animator.SetTrigger("Attack");

        // 마우스로 회전 처리
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    private void MotionSpeed()
    {
        // 애니메이션 파라미터 설정: Speed
        float motionSpeed = movement.magnitude * currentSpeed;
        if (motionSpeed > 0)
        {
            lastMotionSpeed = motionSpeed;
        }
        else
        {
            lastMotionSpeed = 0f; // 가만히 있을 때 Speed를 0으로 설정
        }
        animator.SetFloat("Speed", lastMotionSpeed);
    }
    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            yVelocity = jumpForce;
            animator.SetBool("Jump", true); // JumpStart로 전환
        }
        else
        {
            animator.SetBool("Jump", false);
        }
    }

    private void GroundedCheck()
    {
        // 중력 및 착지 확인
        isGrounded = controller.isGrounded;
        animator.SetBool("Grounded", isGrounded);

        if (isGrounded && yVelocity < 0)
        {
            yVelocity = -2f; // 약간의 마진
        }
        yVelocity += gravity * Time.deltaTime;

        // FreeFall 확인
        isFreeFalling = !isGrounded && yVelocity < 0; // 하강 중일 때
        animator.SetBool("FreeFall", isFreeFalling);
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f || landingAudioClip == null) return;

        AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(controller.center), footstepAudioVolume);
    }
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f || footstepAudioClips.Length == 0) return;

        int index = Random.Range(0, footstepAudioClips.Length);
    AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.TransformPoint(controller.center), footstepAudioVolume);
    }
}