using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    #region variables
    public float speed = 10f;
    public float sprintModifier;
    public float jumpForce;
    private float movementCounter;
    private float idleCounter;

    private Rigidbody rb;

    public Camera normalCam;
    private float baseFOV;
    private float sprintFOVModifier = 1.5f;

    public Transform groundDetector;
    public LayerMask ground;

    private Vector3 targetWeaponBobPosition;
    private Vector3 weaponOriginPosition;
    public Transform weaponParent;
    #endregion

    #region monobehaviour callbacks
    void Start()
    {
        Camera.main.enabled = false;
        baseFOV = normalCam.fieldOfView;
        rb = GetComponent<Rigidbody>();
        weaponOriginPosition = weaponParent.position;

    }
    private void Update()
    {
        //Axis
        float h_Move = Input.GetAxisRaw("Horizontal");
        float v_Move = Input.GetAxisRaw("Vertical");

        //control
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);

        //States
        bool IsGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool Isjumping = jump && IsGrounded;
        bool IsSprinting = sprint && v_Move > 0 && !Isjumping && IsGrounded;

        //Jumping
        if (Isjumping)
        {
            rb.AddForce(Vector3.up * jumpForce);
        }

        //HeadBob
        if(h_Move == 0 && v_Move == 0)
        {
            HeadBob(idleCounter, 10.025f, 10.025f);
            idleCounter += Time.deltaTime;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            Debug.Log("working");

        }
        else if(!IsSprinting)
        {
            HeadBob(movementCounter, 5.035f, 5.035f);
            movementCounter += Time.deltaTime * 3f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
        }
        else 
        {
            HeadBob(movementCounter, 0.09f, 0.05f);
            movementCounter += Time.deltaTime * 7f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
        }

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
    #endregion

    #region private method

    void HeadBob(float x_z, float x_intensity, float y_intensity)
    {
       
        Vector3 targetWeapoonBobPosition = weaponOriginPosition + new Vector3(Mathf.Cos(x_z * 5) * x_intensity, Mathf.Sin(x_z * 5) * y_intensity,0); 
    }

    #endregion

}
