using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player; // 카메라 참조용 오브젝트
    [SerializeField] private float mouseSensitivity = 2f; // 마우스 민감도
    [SerializeField] private float verticalRotationLimit = 80f; // 시야 값 제한

    // 마우스 입력값 저장용 변수
    public float mouseX;
    public float mouseY;

    // 카메라 회전방향
    private float rotationX = 0f;
    private float rotationY = 0f;

    // 카메라 회전 활성화 여부 논리형
    private bool canRotate = true;

    private void Start()
    {
        // 게임 시작 시, 커서 잠금 및 비활성화
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        if (canRotate)
        {
            RotateCamera(); // 카메라 회전            
        }
    }

    private void RotateCamera()
    {
        // 마우스 입력 값 처리
        mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // 수직 회전 제한
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -verticalRotationLimit, verticalRotationLimit);

        // 수평 회전 (기존 코드 유지)
        rotationY += mouseX;

        // 카메라 회전 적용
        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        player.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }

    // 카메라 회전 활성화/비활성화 함수
    public void EnableRotation(bool enable)
    {
        canRotate = enable;
    }

    public Vector3 GetForwardDirection()
    {
        return transform.forward; // 카메라의 앞 방향 반환 
    }

    public Vector3 GetRightDirection()
    {
        return transform.right; // 카메라의 오른쪽 방향 반환
    }

}
