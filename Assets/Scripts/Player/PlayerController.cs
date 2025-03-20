using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Inspector Variables
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;         // 걷기 속도
    [SerializeField] private float runSpeed = 10f;         // 달리기 속도
    [SerializeField] private float jumpSpeed = 8f;         // 점프 속도
    [SerializeField] private float gravity = 20f;          // 중력 가속도
    [SerializeField] private float rotationSpeed = 10f;    // 회전 속도

    [Header("Audio Settings")]
    [SerializeField] private AudioClip landingAudioClip;   // 착지 소리
    [SerializeField] private AudioClip[] footstepAudioClips; // 발소리 배열
    [Range(0, 1)][SerializeField] private float footstepAudioVolume = 0.5f; // 발소리 볼륨

    [Header("Ground Check Settings")]
    [SerializeField] private float groundedOffset = -0.14f; // 지면 체크 오프셋
    [SerializeField] private float groundedRadius = 0.28f;  // 지면 체크 반경
    [SerializeField] private LayerMask groundLayers;        // 지면 레이어
    #endregion

    #region Private Variables
    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection;
    private float yVelocity;
    private float moveSpeed;              // 현재 이동 속도 (걷기/달리기 전환용)
    private bool grounded = true;
    private bool hasAnimator;

    // Animation Variables
    private float animationBlend;         // 애니메이션 블렌딩 속도
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;
    #endregion

    #region Unity Methods
    void Start()
    {
        InitializeComponents();
        moveSpeed = walkSpeed; // 초기 속도는 걷기 속도로 설정
    }

    void Update()
    {
        GroundedCheck();
        HandleInput();
        ApplyGravity();
        MoveCharacter();
        UpdateAnimation();
    }

    void OnDrawGizmosSelected()
    {
        DrawGroundCheckGizmo();
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        if (!TryGetComponent(out controller))
        {
            Debug.LogError("Player must have a CharacterController component!");
        }

        hasAnimator = TryGetComponent(out animator);
        if (hasAnimator)
        {
            AssignAnimationIDs();
        }
    }

    private void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }
    #endregion

    #region Movement Logic
    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 달리기 처리
        moveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // 회전 처리
        if (inputDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 점프 처리
        if (grounded && Input.GetButtonDown("Jump"))
        {
            yVelocity = jumpSpeed;
            if (hasAnimator) animator.SetBool(animIDJump, true);
        }
        else if (hasAnimator)
        {
            animator.SetBool(animIDJump, false);
        }

        moveDirection = transform.TransformDirection(inputDirection) * moveSpeed;
    }

    private void ApplyGravity()
    {
        if (grounded)
        {
            yVelocity = -2f; // 지면에 있을 때 수직 속도 초기화
        }
        else if (hasAnimator)
        {
            animator.SetBool(animIDFreeFall, true);
        }

        yVelocity -= gravity * Time.deltaTime;
        moveDirection.y = yVelocity;
    }

    private void MoveCharacter()
    {
        controller.Move(moveDirection * Time.deltaTime);
    }
    #endregion

    #region Ground Check
    private void GroundedCheck()
    {
        Vector3 spherePosition = transform.position - new Vector3(0, groundedOffset, 0);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        if (hasAnimator)
        {
            animator.SetBool(animIDGrounded, grounded);
        }
    }

    private void DrawGroundCheckGizmo()
    {
        Gizmos.color = grounded ? new Color(0f, 1f, 0f, 0.35f) : new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawSphere(transform.position - new Vector3(0, groundedOffset, 0), groundedRadius);
    }
    #endregion

    #region Animation
    private void UpdateAnimation()
    {
        if (!hasAnimator) return;

        // 입력이 없으면 속도를 0으로 설정 (Idle 상태로 전환)
        float targetSpeed = (moveDirection.magnitude > 0.1f) ? moveSpeed : 0f;
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10f);

        // 애니메이션 파라미터 설정
        animator.SetFloat(animIDSpeed, animationBlend);
        animator.SetFloat(animIDMotionSpeed, moveDirection.magnitude > 0.1f ? 1f : 0f);
        animator.SetBool(animIDGrounded, grounded);

        // FreeFall 상태 초기화 (공중에 있을 때만 활성화)
        if (grounded && animator.GetBool(animIDFreeFall))
        {
            animator.SetBool(animIDFreeFall, false);
        }
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f || footstepAudioClips.Length == 0) return;

        int index = Random.Range(0, footstepAudioClips.Length);
        AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.TransformPoint(controller.center), footstepAudioVolume);
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f || landingAudioClip == null) return;

        AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(controller.center), footstepAudioVolume);
    }
    #endregion
}