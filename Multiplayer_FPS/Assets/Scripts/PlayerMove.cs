using UnityEngine;
using Photon.Pun;

public class PlayerMove : MonoBehaviourPunCallbacks
{
    #region variables
    public float speed = 10f;
    public float sprintModifier;
    public float jumpForce;
    public int max_health;
    private float movementCounter;
    private float idleCounter;
    private int current_health;

    private Rigidbody rb;

    public Camera normalCam;
    public GameObject cameraParent;
    private float baseFOV;
    private float sprintFOVModifier = 1.5f;

    public Transform groundDetector;
    public LayerMask ground;

    private Vector3 targetWeaponBobPosition;
    private Vector3 weaponOriginPosition;
    public Transform weaponParent;

    private Manager manager;
    #endregion

    #region monobehaviour callbacks
    void Start()
    {
        current_health = max_health;
         cameraParent.SetActive(photonView.IsMine);
        if (!photonView.IsMine) gameObject.layer = 11;
        
       if(Camera.main) Camera.main.enabled = false;
        baseFOV = normalCam.fieldOfView;
        rb = GetComponent<Rigidbody>();
        weaponOriginPosition = weaponParent.position;

        manager = GameObject.Find("Manager").GetComponent<Manager>();

    }
    private void Update()
    {
        if (!photonView.IsMine) return;
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
        if (Input.GetKeyDown(KeyCode.U)) Takedamage(500);

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
        if (!photonView.IsMine) return;
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
       
        Vector3 targetWeapoonBobPosition = weaponOriginPosition+ new Vector3(weaponOriginPosition.x,weaponOriginPosition.y + 1f, weaponOriginPosition.z) + new Vector3(Mathf.Cos(x_z * 5) * x_intensity, Mathf.Sin(x_z * 5) * y_intensity,0); 
    }

    #endregion

    #region public method

   
    public void Takedamage(int p_damage)
    {
        if (photonView.IsMine)
        {
            current_health -= p_damage;
            Debug.Log(current_health);

            if(current_health <= 0)
            {
                manager.Spawn();
                PhotonNetwork.Destroy(gameObject);
                Debug.Log("Died");
            }
        }
      
    }


    #endregion

}
