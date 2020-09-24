﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{
    #region varibles
    public Gun[] loadout;
    public Transform weaponParent;

    private GameObject currentWeapon;

    private int currentIndex;


    private bool gunEnabled;

    public GameObject bulletPrefab;
    public LayerMask canBeShoot;
    public float currentCoolDown;
    #endregion


    #region monobehaviour callbacks
    void Update()
    {
        if (!photonView.IsMine) return;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            photonView.RPC("Equip", RpcTarget.All, 0);
            gunEnabled = true;
        }

        if (gunEnabled)
        {
            Aim(Input.GetMouseButton(1));

            if (Input.GetMouseButtonDown(0) && currentCoolDown <= 0)
            {
                photonView.RPC("Shoot", RpcTarget.All);
            }

            //weapon position elasticity
            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);

            //cooldown
           if(currentCoolDown > 0) currentCoolDown -= Time.deltaTime;
        }

        
    }
    #endregion

    #region private method


    [PunRPC]
    void Equip(int p_ind)
    {
        if (currentWeapon != null) Destroy(currentWeapon);

        currentIndex = p_ind;
        GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newWeapon.transform.localPosition = Vector3.zero;
        t_newWeapon.transform.localEulerAngles = Vector3.zero;
        t_newWeapon.GetComponent<Sway>().enabled = photonView.IsMine;

        currentWeapon = t_newWeapon;

    }
    void Aim(bool p_Aiming)
    {
        Transform t_Anchor     = currentWeapon.transform.Find("Anchor");
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
    }

    [PunRPC] // other player can call the function
    void Shoot()
    {
        //bloom
        Transform t_spawn = transform.Find("Cameras/Normal Camera").transform;

        Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
        t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
        t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
        t_bloom -= t_spawn.position;
        t_bloom.Normalize();

        //Raycast
        RaycastHit t_hit = new RaycastHit();
        if(Physics.Raycast(t_spawn.position, t_bloom, out t_hit, canBeShoot))
        {
            GameObject t_newHole = Instantiate(bulletPrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity);
            t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
            Destroy(t_newHole, 3f);

            if (photonView.IsMine)
            {
                if(t_hit.collider.gameObject.layer == 11)
                {
                    //
                }
            }
        }

        //gun fx
        currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
        currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;

        //cooldown fire rate
        currentCoolDown += loadout[currentIndex].fireRate;
    }
    #endregion
}