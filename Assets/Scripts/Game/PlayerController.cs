using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float rotationSpeed = 10f;

    [Header("Camera")]
    [SerializeField] float cameraDistance = 18f;
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float minPitch = 15f;
    [SerializeField] float maxPitch = 75f;
    float yaw = 0f;
    float pitch = 55f;
    Transform camTransform;

    [Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    bool isGrounded;

    Rigidbody rb;
    Camera cam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        rb.useGravity = true;
        rb.freezeRotation = true;
        camTransform = cam.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        yaw   += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        camTransform.position = transform.position + rotation * new Vector3(0f, 0f, -cameraDistance);
        camTransform.LookAt(transform.position + Vector3.up * 1f);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h == 0 && v == 0)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        Vector3 camForward = cam.transform.forward;
        Vector3 camRight   = cam.transform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        rb.linearVelocity = new Vector3(
            moveDir.x * moveSpeed,
            rb.linearVelocity.y,
            moveDir.z * moveSpeed
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(moveDir),
            rotationSpeed * Time.deltaTime
        );
    }
}
