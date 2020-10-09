using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Luncher launcher;

   private void Start()
    {
        Pause.paused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void JoinMatch()
    {
        launcher.Join();
    }

    public void CreateMatch()
    {
        launcher.Create();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
