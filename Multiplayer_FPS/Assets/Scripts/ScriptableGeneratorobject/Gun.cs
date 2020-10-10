using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="New Gun",menuName ="Gun")]
public class Gun : ScriptableObject
{
    public string name;
    public float fireRate;
    public int ammo;
    public int clipsize;
    public int burst; // 0 semi / 1 auto/ 2+ burst fire
    public float bloom;
    public float recoil;
    public float kickback;
    public float aimRate;
    public float reloadTime;
    public GameObject prefab;
    public GameObject display;
    public int damage;
   [Range(0,1)] public float mainFOV;
    [Range(0, 1)] public float weaponFOV;
    public AudioClip gunshotSound;
    public float pitchRandomization;
    public float shotVolume;

    private int stash; // current ammo
    private int clip;  // current clip

    public int pellets;
    public bool recovery;


    public void Initialise()
    {
        stash = ammo;
        clip = clipsize;
    }
    public bool fireBullet()
    {
        if(clip > 0)
        {
            clip--;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Relaod()
    {
        stash += clip;
        clip = Mathf.Min(clipsize, stash);
        stash -= clip;
    }

    public int GetStash() { return stash; }
    public int GetClip() { return clip; }
}
