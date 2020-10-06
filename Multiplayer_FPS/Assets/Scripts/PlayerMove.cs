using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerMove : MonoBehaviourPunCallbacks
{
    #region variables
    public float speed = 10f;
    public float sprintModifier;
    public float lengthOfSlide;
    public float slideModifier;
    public float jumpForce;
    private float movementCounter;
    private float idleCounter;
    public int max_health;
    private int current_health;

    private Rigidbody rb;

    public Camera normalCam;
    public GameObject cameraParent;
    private float baseFOV;
    private Vector3 originCameraPosition;
    private float sprintFOVModifier = 1.5f;

    public Transform groundDetector;
    public LayerMask ground;

    private Vector3 targetWeaponBobPosition;
    private Vector3 weaponOriginPosition;
    private Vector3 weaponParentCurrentPosition;
    public Transform weaponParent;


    private Manager manager;
    private Weapon weapon;

    private Transform ui_healthbar;
    private Text ui_ammo;

    private bool sliding;
    private float slide_time;
    private Vector3 slide_dir;
    #endregion

    #region monobehaviour callbacks
    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        weapon = GetComponent<Weapon>();

        current_health = max_health;

        cameraParent.SetActive(photonView.IsMine);
        if (!photonView.IsMine) gameObject.layer = 11;
        
       if(Camera.main) Camera.main.enabled = false;
        baseFOV = normalCam.fieldOfView;
        originCameraPosition = normalCam.transform.position;

        rb = GetComponent<Rigidbody>();
        weaponOriginPosition = weaponParent.position;
        weaponParentCurrentPosition = weaponOriginPosition;

        if (photonView.IsMine)
        {
            ui_healthbar = GameObject.Find("HUD /Health/Bar").transform;
            ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
            RefreshHealthBar();
            
        }
       

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

        if(sliding) { return; }

       else if(h_Move == 0 && v_Move == 0)
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


        //UI Refresher
        RefreshHealthBar();
        weapon.RefreshAmmo(ui_ammo);

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
        bool slide = Input.GetKey(KeyCode.LeftControl);

        //States
        bool IsGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool Isjumping = jump && IsGrounded;
        bool IsSprinting = sprint && v_Move > 0 && !Isjumping && IsGrounded;
        bool IsSliding = IsSprinting && slide && !sliding; ;


        Vector3 t_direction = Vector3.zero;
        float t_adjSpeed = speed;

        //Movement
        if (!IsSliding)
        {
            t_direction = new Vector3(h_Move, 0, v_Move);
            t_direction.Normalize();
            transform.TransformDirection(t_direction);

            if (IsSprinting) t_adjSpeed *= sprintModifier;
        }
        else
        {
            t_direction = slide_dir;
            t_adjSpeed *= slideModifier;
            slide_time -= Time.deltaTime;
            if(slide_time < 0)
            {
                IsSliding = false;
                weaponParentCurrentPosition += Vector3.up * 0.5f;
            }
        }

        Vector3 t_targetVelocity = t_direction * t_adjSpeed * Time.deltaTime;
        t_targetVelocity.y = rb.velocity.y;
        rb.velocity = t_targetVelocity;

        //sliding
        if (IsSliding)
        {
            sliding = true;
            slide_dir = t_direction;
            slide_time = lengthOfSlide;
            weaponParentCurrentPosition += Vector3.down * 0.5f;
        }

        //Camera stuff
        if (IsSliding)
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier * 1.25f, Time.deltaTime * 8f);
            normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, originCameraPosition + Vector3.down * 0.5f, Time.deltaTime * 6f);
        }
        else
        {
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
    #endregion

    #region private method

    void HeadBob(float x_z, float x_intensity, float y_intensity)
    {
       
        Vector3 targetWeapoonBobPosition = weaponParentCurrentPosition + new Vector3(Mathf.Cos(x_z * 5) * x_intensity, Mathf.Sin(x_z * 5) * y_intensity,0); 
    }

    void RefreshHealthBar()
    {
        float t_health_ratio = (float)current_health / (float)max_health;
        ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale,  new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
    }
    #endregion

    #region public method

   
    public void Takedamage(int p_damage)
    {
        if (photonView.IsMine)
        {
            current_health -= p_damage;
            RefreshHealthBar();

            if (current_health <= 0)
            {
                manager.Spawn();
                PhotonNetwork.Destroy(gameObject);
                Debug.Log("Died");
            }
        }
      
    }


    #endregion

}
