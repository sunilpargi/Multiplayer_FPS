using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{
    #region varibles
    public List<Gun> loadout;
    private int currentIndex;

    public Transform weaponParent;
    private GameObject currentWeapon;

    private bool gunEnabled;

    public GameObject bulletPrefab;
    public LayerMask canBeShoot;
    public float currentCoolDown;

    public bool isAiming;
    private bool isReloading;
    [HideInInspector] public Gun currentGunData;

   
    public AudioSource sfx;
    public AudioClip hitmarkerSound;

    private Image hitmarkerImage;
    private float hitmarkerWait;
    private Color CLEARWHITE = new Color(1, 1, 1, 0);
    #endregion


    #region monobehaviour callbacks
    private void Start()
    {
        foreach (Gun a in loadout) a.Initialise();

        hitmarkerImage = GameObject.Find("HUD/HitMarker/Image").GetComponent<Image>();
        hitmarkerImage.color = CLEARWHITE;
        Equip(0);
        
        
    }
    void Update()
    {
        if (Pause.paused && photonView.IsMine) return;
     
        if (photonView.IsMine &&Input.GetKeyDown(KeyCode.Alpha1))
        {
            photonView.RPC("Equip", RpcTarget.All, 0);
            gunEnabled = true;
        }
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2))
        {
            photonView.RPC("Equip", RpcTarget.All, 1);
            gunEnabled = true;
        }

        if (gunEnabled)
        {
            if (photonView.IsMine)
            {

                if (loadout[currentIndex].burst != 1)
                {
                    if (Input.GetMouseButtonDown(0) && currentCoolDown <= 0)
                    {
                        if (loadout[currentIndex].fireBullet()) photonView.RPC("Shoot", RpcTarget.All); // if have bullet then shoot

                        else StartCoroutine(Reload(loadout[currentIndex].reloadTime));

                    }
                }
                else
                {
                    if (Input.GetMouseButton(0) && currentCoolDown <= 0)
                    {
                        if (loadout[currentIndex].fireBullet()) photonView.RPC("Shoot", RpcTarget.All); // if have bullet then shoot

                        else StartCoroutine(Reload(loadout[currentIndex].reloadTime));

                    }
                }

                //cooldown
                if (currentCoolDown > 0) currentCoolDown -= Time.deltaTime;

                if (Input.GetKeyDown(KeyCode.R))
                {
                    photonView.RPC("ReloadRPC", RpcTarget.All);
                }
              
            }

            //weapon position elasticity
            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);

          
        }
        if (photonView.IsMine)
        {
            if (hitmarkerWait > 0)
            {
                hitmarkerWait -= Time.deltaTime;
            }
            else if (hitmarkerImage.color.a > 0)
            {
                hitmarkerImage.color = Color.Lerp(hitmarkerImage.color, CLEARWHITE, Time.deltaTime * 2f);
            }
        }


    }
    #endregion

    #region private method

    [PunRPC]
    private void ReloadRPC()
    {
        StartCoroutine(Reload(loadout[currentIndex].reloadTime));
    }
    IEnumerator Reload(float p_wait)
    {
        isReloading = true;

        if (currentWeapon.GetComponent<Animator>()) 
        {
            currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);
        }
        else { currentWeapon.SetActive(false); }
       

        yield return new WaitForSeconds(p_wait);
        loadout[currentIndex].Relaod();
        currentWeapon.SetActive(true);

        isReloading = false; 

    }


    [PunRPC]
    void Equip(int p_ind)
    {
        if (currentWeapon != null)
        {
            if (isReloading) StopCoroutine("Reload");
            Destroy(currentWeapon);
        }

        currentIndex = p_ind;

        GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newWeapon.transform.localPosition = Vector3.zero;
        t_newWeapon.transform.localEulerAngles = Vector3.zero;
        t_newWeapon.GetComponent<Sway>().Ismine = photonView.IsMine;

        if (photonView.IsMine) ChangeLayersRecursively(t_newWeapon, 10);
        else ChangeLayersRecursively(t_newWeapon, 0);

        t_newWeapon.GetComponent<Animator>().Play("Equip", 0, 0);

        currentWeapon = t_newWeapon;
        currentGunData = loadout[p_ind];

    }

    [PunRPC]
    void PickupWeapon(string name)
    {
        Gun newWeapon = GunLibrary.FindGun(name);
        newWeapon.Initialise();

        if (loadout.Count >= 2)
        {
            loadout[currentIndex] = newWeapon;
            Equip(currentIndex);
        }
        else
        {
            loadout.Add(newWeapon);
            Equip(loadout.Count - 1);
        }
    }
    private void ChangeLayersRecursively(GameObject p_target, int p_layer)
    {
        p_target.layer = p_layer;
        foreach (Transform a in p_target.transform) ChangeLayersRecursively(a.gameObject, p_layer);
    }
    public bool Aim(bool p_Aiming)
    {
        if (!currentWeapon) return false;
        if (isReloading) p_Aiming = false;

        isAiming = p_Aiming;

        Transform t_Anchor     = currentWeapon.transform.Find("Root");
        Transform t_states_hip = currentWeapon.transform.Find("States/Hip");
        Transform t_states_ADS = currentWeapon.transform.Find("States/ADS");

        if (p_Aiming)
        {
            //aim
            t_Anchor.position = Vector3.Lerp(t_Anchor.position, t_states_ADS.position, Time.deltaTime * loadout[currentIndex].aimRate);

        }
        else
        {
            //hip
            t_Anchor.position = Vector3.Lerp(t_Anchor.position, t_states_hip.position, Time.deltaTime * loadout[currentIndex].aimRate);
        }
        return p_Aiming;
    }

    [PunRPC] // other player can call the function
    void Shoot()
    {
        //bloom
        Transform t_spawn = transform.Find("Cameras/Normal Camera").transform;

        //cooldown fire rate
        currentCoolDown += loadout[currentIndex].fireRate;

        for (int i = 0; i < Mathf.Max(1, currentGunData.pellets); i++)
        {
            

            Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
            t_bloom -= t_spawn.position;
            t_bloom.Normalize();

            //Raycast
            RaycastHit t_hit = new RaycastHit();
            if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, canBeShoot))
            {
                GameObject t_newHole = Instantiate(bulletPrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity);
                t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
                Destroy(t_newHole, 3f);

                if (photonView.IsMine)
                {
                    //shotting player 
                    if (t_hit.collider.gameObject.layer == 11)
                    {
                        t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentIndex].damage, PhotonNetwork.LocalPlayer.ActorNumber);

                        //show hitmarker
                        hitmarkerImage.color = Color.white;
                        sfx.PlayOneShot(hitmarkerSound);
                        hitmarkerWait = 1f;
                    }

                    //shooting target
                    if (t_hit.collider.gameObject.layer == 12)
                    {
                        //destroy
                        Destroy(t_hit.collider.gameObject);

                        //show hitmarker
                        hitmarkerImage.color = Color.white;
                        sfx.PlayOneShot(hitmarkerSound);
                        hitmarkerWait = 1f;
                    }
                }
            }

            //sound
            sfx.Stop();
            sfx.clip = currentGunData.gunshotSound;
            sfx.volume = currentGunData.shotVolume;
            sfx.pitch = 1 - currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
            sfx.Play();

            //gun fx
            currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
            currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;

            if (currentGunData.recovery) currentWeapon.GetComponent<Animator>().Play("Recovery", 0, 0);

        }
    }

    [PunRPC]
    private void TakeDamge(int p_damage,int p_actor)
    {
        GetComponent<PlayerMove>().Takedamage(p_damage, p_actor);
    }
    #endregion

    #region public method
   public void RefreshAmmo(Text p_text)
    {
        int t_clip = loadout[currentIndex].GetClip();
        int t_stash = loadout[currentIndex].GetStash();

        p_text.text = t_clip.ToString("D2") + "/" + t_stash.ToString("D2"); 
    }

    #endregion
}
