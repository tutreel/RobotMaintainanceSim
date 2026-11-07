
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public Transform playerCamera;
    public float moveSpeed = 4f;
    public float sprintSpeed = 7f;
    public float mouseSensitivity = 2f;
    public float gravity = -20f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mouse look ONLY when cursor is locked
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);

            playerCamera.localRotation =
                Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        // Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move =
            transform.right * x +
            transform.forward * z;

        float speed = Input.GetKey(KeyCode.LeftShift)
            ? sprintSpeed
            : moveSpeed;

        controller.Move(move.normalized * speed * Time.deltaTime);

        // Gravity and jumping
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
            verticalVelocity = 7f;

        verticalVelocity += gravity * Time.deltaTime;

        controller.Move(
            Vector3.up * verticalVelocity * Time.deltaTime
        );
    }
}
