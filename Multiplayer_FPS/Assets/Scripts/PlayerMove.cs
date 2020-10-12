using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerMove : MonoBehaviourPunCallbacks, IPunObservable
{
    #region variables
    public float speed = 10f;
    public float sprintModifier;
    public float slideModifier;
    public float crouchModifier;
    public float lengthOfSlide;

    private Rigidbody rb;
    public float jumpForce;
    public float jetForce;
    public float jetWait;
    public float jetRecovery;
    public int max_fuel;
    private float current_fuel;
    private float current_recovery;

    public int max_health;
    private int current_health;

    private float movementCounter;
    private float idleCounter;

    public float slideAmout;
    public float crouchAmout;
    public GameObject standingCollider;
    public GameObject crouchingCollider;
    public GameObject mesh;
    private bool crouched;

    public Camera normalCam;
    public Camera weaponCam;
    private Vector3 originNormalCamPosition;
    private float baseFOV;
    private float sprintFOVModifier = 1.5f;
    public GameObject cameraParent;

    public Transform groundDetector;
    public LayerMask ground;

    private Vector3 targetWeaponBobPosition;
    private Vector3 weaponOriginPosition;
    private Vector3 weaponParentCurrentPosition;
    public Transform weaponParent;


    private Manager manager;
    private Weapon weapon;

    private Transform ui_healthbar;
    private Transform ui_fuelbar;
    private Text ui_ammo;
    private Text ui_username;

    private bool sliding;
    private float slide_time;
    private Vector3 slide_dir;

    private bool isAiming;
    private bool canJet;

    private Vector3 normalCamTarget;
    private Vector3 weaponCamTarget;

    private Animator anim;
    private float aimAngle;

    [HideInInspector] public ProfileData playerProfile;
    public TextMeshPro playerUsername;
    #endregion

    #region monobehaviour callbacks
    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        weapon = GetComponent<Weapon>();     

        //camera
        cameraParent.SetActive(photonView.IsMine);
        if (Camera.main) Camera.main.enabled = false;
        baseFOV = normalCam.fieldOfView;
        originNormalCamPosition = normalCam.transform.position;

        if (!photonView.IsMine)
        {
            gameObject.layer = 11;
            standingCollider.layer = 11;
            crouchingCollider.layer = 11;
            ChangeLayerRecursively(mesh.transform, 11);
        }

        rb = GetComponent<Rigidbody>();

        //weapon
        weaponOriginPosition = weaponParent.position;
        weaponParentCurrentPosition = weaponOriginPosition;

        //UI
        current_health = max_health;
        current_fuel = max_fuel;

        if (photonView.IsMine)
        {
            ui_healthbar = GameObject.Find("HUD/Health/Bar").transform;
            ui_fuelbar = GameObject.Find("HUD/Fuel/Bar").transform;
            ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
            ui_username = GameObject.Find("HUD/Username/Text").GetComponent<Text>();
            RefreshHealthBar();

            ui_username.text = Luncher.myProfile.username;
            photonView.RPC("SyncProfile", RpcTarget.All, Luncher.myProfile.username, Luncher.myProfile.level, Luncher.myProfile.xp);
            anim = GetComponent<Animator>();
            
        }     
    }

    private void ChangeLayerRecursively(Transform p_trans, int p_layer)
    {
        p_trans.gameObject.layer = p_layer;
        foreach (Transform t in p_trans) ChangeLayerRecursively(t, p_layer);
    }
    private void Update()
    {
        if (!photonView.IsMine) return;

        //Axis
        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");

        //control
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool crouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
        bool pause = Input.GetKeyDown(KeyCode.Escape);
       

        //States
        bool IsGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.15f, ground);
        bool Isjumping = jump && IsGrounded;
        bool IsSprinting = sprint && t_vmove > 0 && !Isjumping && IsGrounded;
        bool IsCrouching = crouch && !IsSprinting && !Isjumping && IsGrounded;
        

        if (Pause.paused)
        {
            t_hmove = 0f;
            t_vmove = 0f;
            sprint = false;
            jump = false;
            crouch = false;
            pause = false;
            IsGrounded = false;
            Isjumping = false;
            IsSprinting = false;
            IsCrouching = false;
        }

        //Crouching
        if (IsCrouching)
        {
            photonView.RPC("SetCrouch", RpcTarget.All, !crouched);
        }


        //Jumping
        if (Isjumping)
        {
            if(crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);

            rb.AddForce(Vector3.up * jumpForce);
            current_recovery = 0;
        }

        if (Input.GetKeyDown(KeyCode.U)) Takedamage(500,-1);


        //HeadBob
        if (!IsGrounded)
        {
            //In Air
            HeadBob(idleCounter, 10.025f, 10.025f);
            idleCounter += 0;
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
        }
       else if(sliding)  // if sliding
        {
            HeadBob(movementCounter, 0.15f, 5.075f);
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
        }

       else if(t_hmove == 0 && t_vmove == 0) // if standing
        {
            HeadBob(idleCounter, 10.025f, 10.025f);
            idleCounter += Time.deltaTime;
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
        }

        else if(!IsSprinting && !IsCrouching)  // if walking
        {
            HeadBob(movementCounter, 5.035f, 5.035f);
            movementCounter += Time.deltaTime * 3f;
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
        }

        else if (crouched)  // if crouching
        {
            HeadBob(movementCounter, 0.02f, 0.02f);
            movementCounter += Time.deltaTime * 3f;
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
        }

        else   // if sprinting
        {
            HeadBob(movementCounter, 0.09f, 0.05f);
            movementCounter += Time.deltaTime * 7f;
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
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
        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");

        //control
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump   = Input.GetKeyDown(KeyCode.Space);
        bool slide = Input.GetKey(KeyCode.LeftControl);
        bool crouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
        bool pause = Input.GetKeyDown(KeyCode.Escape);
        bool aim = Input.GetMouseButton(1);
        bool jet = Input.GetKey(KeyCode.Space);

        //States
        bool IsGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool Isjumping = jump && IsGrounded;
        bool IsSprinting = sprint && t_vmove > 0 && !Isjumping && IsGrounded;
        bool IsSliding = IsSprinting && slide && !sliding; ;
        bool IsCrouching = crouch && !IsSprinting && !Isjumping && IsGrounded;
        isAiming = aim && !IsSliding && !IsSprinting;

        //Pause
        if (pause)
        {
            GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
        } 

        if (Pause.paused)
        {
            t_hmove = 0f;
            t_vmove = 0f;
            sprint = false;
            jump = false;
            crouch = false;
            pause = false;
            IsGrounded = false;
            Isjumping = false;
            IsSprinting = false;
            IsCrouching = false;
            isAiming = false;
        }

        //Movement
        Vector3 t_direction = Vector3.zero;
        float t_adjSpeed = speed;

        if (!IsSliding)
        {
            t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();
            transform.TransformDirection(t_direction);

            if (IsSprinting) 
            {
                if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
                t_adjSpeed *= sprintModifier;
                    
            }
            else if (crouched)
            {
                t_adjSpeed *= crouchModifier;
            }
        }
        else
        {
            t_direction = slide_dir;
            t_adjSpeed *= slideModifier;
            slide_time -= Time.deltaTime;
            if(slide_time < 0)
            {
                sliding = false;
                weaponParentCurrentPosition -= Vector3.down * (slideAmout - crouchAmout);
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
            weaponParentCurrentPosition += Vector3.down * (slideAmout - crouchAmout);
            if(!crouched) photonView.RPC("SetCrouch", RpcTarget.All, true);
        }


        //Jetting
        if (jump && !IsGrounded)
            canJet = true;
        if (IsGrounded)
            canJet = false;

        if (canJet && jet && current_fuel > 0)
        {
            rb.AddForce(Vector3.up * jetForce * Time.fixedDeltaTime, ForceMode.Acceleration);
            current_fuel = Mathf.Max(0, current_fuel - Time.fixedDeltaTime);
        }

        if (IsGrounded)
        {
            if (current_recovery < jetWait)
                current_recovery = Mathf.Min(jetWait, current_recovery + Time.fixedDeltaTime);
            else
                current_fuel = Mathf.Min(max_fuel, current_fuel + Time.fixedDeltaTime * jetRecovery);
        }

      //  ui_fuelbar.localScale = new Vector3(current_fuel / max_fuel, 1, 1);

        //Aiming
      isAiming =   weapon.Aim(isAiming);

        //Camera Stuff
        if (sliding)
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier * 1.15f, Time.deltaTime * 8f);
            weaponCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier * 1.15f, Time.deltaTime * 8f);

            normalCamTarget = Vector3.MoveTowards(normalCam.transform.localPosition, originNormalCamPosition + Vector3.down * slideAmout, Time.deltaTime);
            weaponCamTarget = Vector3.MoveTowards(weaponCam.transform.localPosition, originNormalCamPosition + Vector3.down * slideAmout, Time.deltaTime);
        }
        else
        {
            if (IsSprinting)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
            }
            else if (isAiming)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * weapon.currentGunData.mainFOV, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV * weapon.currentGunData.weaponFOV, Time.deltaTime * 8f);
            }
            else
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
            }

            if (crouched)
            {
                normalCamTarget = Vector3.MoveTowards(normalCam.transform.localPosition, originNormalCamPosition + Vector3.down * crouchAmout, Time.deltaTime);
                weaponCamTarget = Vector3.MoveTowards(weaponCam.transform.localPosition, originNormalCamPosition + Vector3.down * crouchAmout, Time.deltaTime);
            }
            else
            {
                normalCamTarget = Vector3.MoveTowards(normalCam.transform.localPosition, originNormalCamPosition, Time.deltaTime);
                weaponCamTarget = Vector3.MoveTowards(weaponCam.transform.localPosition, originNormalCamPosition, Time.deltaTime);
            }
        }

        //Animations
        float t_anim_horizontal = 0f;
        float t_anim_vertical = 0f;

        if (IsGrounded)
        {
            t_anim_horizontal = t_direction.x;
            t_anim_vertical = t_direction.z;
        }

        anim.SetFloat("Horizontal", t_anim_horizontal);
        anim.SetFloat("Vertical", t_anim_vertical);
    }

    private void LateUpdate()
    {
        normalCam.transform.localPosition = normalCamTarget;
        weaponCam.transform.localPosition = weaponCamTarget;
    }
    #endregion

    #region



    #endregion


    void HeadBob(float x_z, float x_intensity, float y_intensity)
    {
        float t_aim_adjust = 1f;
        if (isAiming) t_aim_adjust = 0.1f;

        Vector3 targetWeapoonBobPosition = weaponParentCurrentPosition + new Vector3(Mathf.Cos(x_z * 5) * x_intensity * t_aim_adjust, Mathf.Sin(x_z * 5) * y_intensity * t_aim_adjust,0); 
    }

    void RefreshHealthBar()
    {
        float t_health_ratio = (float)current_health / (float)max_health;
        ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale,  new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
    }

    [PunRPC]
    private void SyncProfile(string p_username, int p_level, int p_xp)
    {
        playerProfile = new ProfileData(p_username, p_level, p_xp);
        playerUsername.text = playerProfile.username;
    }

    [PunRPC]
    void SetCrouch(bool p_state)
    {
        if (crouched == p_state) return;

        crouched = p_state;

        if (crouched)
        {
            standingCollider.SetActive(false);
            crouchingCollider.SetActive(true);
            weaponParentCurrentPosition += Vector3.down * crouchAmout;
        }
        else
        {
            standingCollider.SetActive(true);
            crouchingCollider.SetActive(false);
            weaponParentCurrentPosition -= Vector3.down * crouchAmout;
        }
    }
    
    #region public method   
    public void Takedamage(int p_damage, int p_actor)
    {
        if (photonView.IsMine)
        {
            current_health -= p_damage;
            RefreshHealthBar();

            if (current_health <= 0)
            {
                manager.Spawn();
                manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

                if (p_actor >= 0)
                    manager.ChangeStat_S(p_actor, 0, 1);

                PhotonNetwork.Destroy(gameObject);
                Debug.Log("Died");
            }
        }
      
    }

    public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
    {
        if (p_stream.IsWriting)
        {
            p_stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
        }
        else
        {
            aimAngle = (int)p_stream.ReceiveNext() / 100f;
        }
    }

    #endregion

}
