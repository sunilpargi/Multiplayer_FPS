using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public string player_prefab;
    public Transform spawn;
    void Start()
    {
        
    }

    // Update is called once per frame
   public void Spawn()
    {

        PhotonNetwork.Instantiate(player_prefab, spawn.position, spawn.rotation);
    }
}
