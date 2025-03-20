using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;      // �̵� �ӵ�
    [SerializeField] private float jumpSpeed = 8f;      // ���� �ӵ�
    [SerializeField] private float gravity = 20f;       // �߷� ���ӵ�
    [SerializeField] private float rotationSpeed = 10f; // ȸ�� �ӵ�

    // [�ִϸ��̼� ���� �߰�]
    [SerializeField] private AudioClip LandingAudioClip;
    [SerializeField] private AudioClip[] FootstepAudioClips;
    [Range(0, 1)][SerializeField] private float FootstepAudioVolume = 0.5f;

    // [���� üũ ���� �߰�]
    [SerializeField] private float groundedOffset = -0.14f; // ���� üũ ������
    [SerializeField] private float groundedRadius = 0.28f;  // ���� üũ �ݰ�
    [SerializeField] private LayerMask groundLayers;        // �������� �ν��� ���̾�

    private CharacterController controller;
    private Vector3 moveDirection;
    private float yVelocity;

    // [�ִϸ��̼� ���� �߰�]
    private Animator animator;
    private bool hasAnimator;
    private float animationBlend; // �ִϸ��̼� ���� �ӵ�
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;

    // [���� üũ ���� �߰�]
    private bool grounded = true;

    void Start()
    {
        if (!TryGetComponent<CharacterController>(out controller))
        {
            Debug.LogError("Player must have a CharacterController component!");
        }

        // [�ִϸ��̼� ���� �ʱ�ȭ]
        hasAnimator = TryGetComponent(out animator);
        if (hasAnimator)
        {
            AssignAnimationIDs();
        }
    }

    void Update()
    {
        // [���� üũ ȣ��]
        GroundedCheck();

        // �Է� ó��
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // �̵� ���� ���
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        float inputMagnitude = inputDirection.magnitude; // �ִϸ��̼� �ӵ� ����

        // ĳ���� ȸ��
        if (inputDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // �̵� �� ���� ó��
        if (grounded)
        {
            yVelocity = -2f; // ���� �� ���� �ӵ� �ʱ�ȭ (ThirdPersonController���� ������)
            if (Input.GetButtonDown("Jump"))
            {
                yVelocity = jumpSpeed; // ����
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
            // ���� ���� �� FreeFall �ִϸ��̼� ����
            if (hasAnimator)
            {
                animator.SetBool(animIDFreeFall, true);
            }
        }

        // �߷� ����
        yVelocity -= gravity * Time.deltaTime;
        moveDirection = transform.TransformDirection(inputDirection) * moveSpeed;
        moveDirection.y = yVelocity;

        // �̵� ����
        controller.Move(moveDirection * Time.deltaTime);

        // [�ִϸ��̼� ������Ʈ]
        if (hasAnimator)
        {
            // �ӵ��� ���� �ִϸ��̼� ����
            float targetSpeed = inputDirection == Vector3.zero ? 0f : moveSpeed;
            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10f); // SpeedChangeRate �⺻�� 10
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, inputMagnitude);
            animator.SetBool(animIDGrounded, grounded);
        }
    }

    // [���� üũ �Լ� - ThirdPersonController���� ������]
    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        if (hasAnimator)
        {
            animator.SetBool(animIDGrounded, grounded);
        }
    }

    // [�ִϸ��̼� ID �Ҵ� - ThirdPersonController���� ������]
    private void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    // [�߼Ҹ� ��� - ThirdPersonController���� ������]
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && FootstepAudioClips.Length > 0)
        {
            var index = Random.Range(0, FootstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(controller.center), FootstepAudioVolume);
        }
    }

    // [���� �Ҹ� ��� - ThirdPersonController���� ������]
    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && LandingAudioClip != null)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(controller.center), FootstepAudioVolume);
        }
    }

    // [���� üũ ������ ����� - ������ �߰�]
    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = grounded ? transparentGreen : transparentRed;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
    }
}