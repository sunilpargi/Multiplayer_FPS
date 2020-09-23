using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;
    public float sprintModifier;
    public float jumpForce;

    private Rigidbody rb;

    public Camera normalCam;
    private float baseFOV;
    private float sprintFOVModifier = 1.5f;

    public Transform groundDetector;
    public LayerMask ground;
    void Start()
    {
        Camera.main.enabled = false;
        baseFOV = normalCam.fieldOfView;
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
   
    private void FixedUpdate()
    {
        //Axis
        float h_Move = Input.GetAxisRaw("Horizontal");
        float v_Move = Input.GetAxisRaw("Vertical");

        //control
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump   = Input.GetKeyDown(KeyCode.Space);

        //States
        bool IsGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool Isjumping = jump && IsGrounded;
        bool IsSprinting = sprint && v_Move > 0 && !Isjumping && IsGrounded;

        //Jumping
        if (Isjumping)
        {
            rb.AddForce(Vector3.up * jumpForce);
        }

        //Movement
        Vector3 direction = new Vector3(h_Move, 0, v_Move);
        direction.Normalize();

        float t_adjSpeed = speed;
        if (IsSprinting) t_adjSpeed *= sprintModifier;

        Vector3 t_targetVelocity = transform.TransformDirection(direction) * t_adjSpeed * Time.deltaTime;
        t_targetVelocity.y = rb.velocity.y;
        rb.velocity = t_targetVelocity;

        //FOV
        if (IsSprinting)
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
        }
        else
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
        }
    }
}
