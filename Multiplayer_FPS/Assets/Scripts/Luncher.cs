using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
public class Luncher : MonoBehaviourPunCallbacks
{

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        connect();
        
    }// Start is called before the first frame update
    void connect()
    {
        Debug.Log("tring to connect");
        PhotonNetwork.GameVersion = "0,0,0";
        PhotonNetwork.ConnectUsingSettings();
    }
    void Join()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnConnectedToMaster()
    {

        PhotonNetwork.JoinLobby();
        base.OnConnectedToMaster();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Create();

        base.OnJoinRandomFailed(returnCode, message);
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Connected");
        StartGame();

        base.OnJoinedRoom();
    }

  
  
    void Create()
    {
        PhotonNetwork.CreateRoom("");
    }
    void StartGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }

    // Update is called once per frame
    
}
