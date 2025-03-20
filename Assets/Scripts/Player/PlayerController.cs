using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Inspector Variables
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;         // �ȱ� �ӵ�
    [SerializeField] private float runSpeed = 10f;         // �޸��� �ӵ�
    [SerializeField] private float jumpSpeed = 8f;         // ���� �ӵ�
    [SerializeField] private float gravity = 20f;          // �߷� ���ӵ�
    [SerializeField] private float rotationSpeed = 10f;    // ȸ�� �ӵ�

    [Header("Audio Settings")]
    [SerializeField] private AudioClip landingAudioClip;   // ���� �Ҹ�
    [SerializeField] private AudioClip[] footstepAudioClips; // �߼Ҹ� �迭
    [Range(0, 1)][SerializeField] private float footstepAudioVolume = 0.5f; // �߼Ҹ� ����

    [Header("Ground Check Settings")]
    [SerializeField] private float groundedOffset = -0.14f; // ���� üũ ������
    [SerializeField] private float groundedRadius = 0.28f;  // ���� üũ �ݰ�
    [SerializeField] private LayerMask groundLayers;        // ���� ���̾�
    #endregion

    #region Private Variables
    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection;
    private float yVelocity;
    private float moveSpeed;              // ���� �̵� �ӵ� (�ȱ�/�޸��� ��ȯ��)
    private bool grounded = true;
    private bool hasAnimator;

    // Animation Variables
    private float animationBlend;         // �ִϸ��̼� ���� �ӵ�
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
        moveSpeed = walkSpeed; // �ʱ� �ӵ��� �ȱ� �ӵ��� ����
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

        // �޸��� ó��
        moveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // ȸ�� ó��
        if (inputDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // ���� ó��
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
            yVelocity = -2f; // ���鿡 ���� �� ���� �ӵ� �ʱ�ȭ
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

        // �Է��� ������ �ӵ��� 0���� ���� (Idle ���·� ��ȯ)
        float targetSpeed = (moveDirection.magnitude > 0.1f) ? moveSpeed : 0f;
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10f);

        // �ִϸ��̼� �Ķ���� ����
        animator.SetFloat(animIDSpeed, animationBlend);
        animator.SetFloat(animIDMotionSpeed, moveDirection.magnitude > 0.1f ? 1f : 0f);
        animator.SetBool(animIDGrounded, grounded);

        // FreeFall ���� �ʱ�ȭ (���߿� ���� ���� Ȱ��ȭ)
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