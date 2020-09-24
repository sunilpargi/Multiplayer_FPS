using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region varibles
    public Gun[] loadout;
    public Transform weaponParent;

    private GameObject currentWeapon;

    private int currentInd;


    private bool gunEnabled;

    public GameObject bulletPrefab;
    public LayerMask canBeShoot;
    #endregion


    #region monobehaviour callbacks
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Equip(0);
            gunEnabled = true;
        }
        if (gunEnabled)
        {
            Aim(Input.GetMouseButton(1));
            if (Input.GetMouseButton(0)) Shoot();
        }
    }
    #endregion

    #region private method
    void Equip(int p_ind)
    {
        if (currentWeapon != null) Destroy(currentWeapon);

        currentInd = p_ind;
        GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newWeapon.transform.localPosition = Vector3.zero;
        t_newWeapon.transform.localEulerAngles = Vector3.zero;

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
            t_Anchor.position = Vector3.Lerp(t_Anchor.position, t_states_ADS.position, Time.deltaTime * loadout[currentInd].aimRate);

        }
        else
        {
            //hip
            t_Anchor.position = Vector3.Lerp(t_Anchor.position, t_states_hip.position, Time.deltaTime * loadout[currentInd].aimRate);
        }
    }

  
    void Shoot()
    {
        Transform t_spawn = transform.Find("Cameras/Normal Camera").transform;
        RaycastHit t_hit = new RaycastHit();
        if(Physics.Raycast(t_spawn.position, t_spawn.forward, out t_hit, canBeShoot))
        {
            GameObject t_newHole = Instantiate(bulletPrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity);
            t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
            Destroy(t_newHole, 3f);
        }
    }
    #endregion
}
