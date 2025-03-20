using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;      // 이동 속도
    [SerializeField] private float jumpSpeed = 8f;      // 점프 속도
    [SerializeField] private float gravity = 20f;       // 중력 가속도
    [SerializeField] private float rotationSpeed = 10f; // 회전 속도

    // [애니메이션 관련 추가]
    [SerializeField] private AudioClip LandingAudioClip;
    [SerializeField] private AudioClip[] FootstepAudioClips;
    [Range(0, 1)][SerializeField] private float FootstepAudioVolume = 0.5f;

    // [지면 체크 관련 추가]
    [SerializeField] private float groundedOffset = -0.14f; // 지면 체크 오프셋
    [SerializeField] private float groundedRadius = 0.28f;  // 지면 체크 반경
    [SerializeField] private LayerMask groundLayers;        // 지면으로 인식할 레이어

    private CharacterController controller;
    private Vector3 moveDirection;
    private float yVelocity;

    // [애니메이션 관련 추가]
    private Animator animator;
    private bool hasAnimator;
    private float animationBlend; // 애니메이션 블렌딩 속도
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;

    // [지면 체크 관련 추가]
    private bool grounded = true;

    void Start()
    {
        if (!TryGetComponent<CharacterController>(out controller))
        {
            Debug.LogError("Player must have a CharacterController component!");
        }

        // [애니메이션 관련 초기화]
        hasAnimator = TryGetComponent(out animator);
        if (hasAnimator)
        {
            AssignAnimationIDs();
        }
    }

    void Update()
    {
        // [지면 체크 호출]
        GroundedCheck();

        // 입력 처리
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 이동 방향 계산
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        float inputMagnitude = inputDirection.magnitude; // 애니메이션 속도 계산용

        // 캐릭터 회전
        if (inputDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 이동 및 점프 처리
        if (grounded)
        {
            yVelocity = -2f; // 착지 시 수직 속도 초기화 (ThirdPersonController에서 가져옴)
            if (Input.GetButtonDown("Jump"))
            {
                yVelocity = jumpSpeed; // 점프
                if (hasAnimator)
                {
                    animator.SetBool(animIDJump, true);
                }
            }
            else if (hasAnimator)
            {
                animator.SetBool(animIDJump, false);
            }
        }
        else
        {
            // 낙하 중일 때 FreeFall 애니메이션 설정
            if (hasAnimator)
            {
                animator.SetBool(animIDFreeFall, true);
            }
        }

        // 중력 적용
        yVelocity -= gravity * Time.deltaTime;
        moveDirection = transform.TransformDirection(inputDirection) * moveSpeed;
        moveDirection.y = yVelocity;

        // 이동 실행
        controller.Move(moveDirection * Time.deltaTime);

        // [애니메이션 업데이트]
        if (hasAnimator)
        {
            // 속도에 따라 애니메이션 블렌딩
            float targetSpeed = inputDirection == Vector3.zero ? 0f : moveSpeed;
            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10f); // SpeedChangeRate 기본값 10
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, inputMagnitude);
            animator.SetBool(animIDGrounded, grounded);
        }
    }

    // [지면 체크 함수 - ThirdPersonController에서 가져옴]
    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        if (hasAnimator)
        {
            animator.SetBool(animIDGrounded, grounded);
        }
    }

    // [애니메이션 ID 할당 - ThirdPersonController에서 가져옴]
    private void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    // [발소리 재생 - ThirdPersonController에서 가져옴]
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && FootstepAudioClips.Length > 0)
        {
            var index = Random.Range(0, FootstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(controller.center), FootstepAudioVolume);
        }
    }

    // [착지 소리 재생 - ThirdPersonController에서 가져옴]
    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && LandingAudioClip != null)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(controller.center), FootstepAudioVolume);
        }
    }

    // [지면 체크 디버깅용 기즈모 - 선택적 추가]
    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = grounded ? transparentGreen : transparentRed;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
    }
}