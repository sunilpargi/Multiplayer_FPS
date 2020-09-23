using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Look : MonoBehaviour
{
    public Transform player;
    public Transform cams;

    public float xSensitivity;
    public float ySensitivity;
    public float maxAngle;

    public static bool cursorLocked;


    Quaternion camsCenter;
    void Start()
    {
        camsCenter = cams.localRotation;
        cursorLocked = true;
    }

    // Update is called once per frame
    void Update()
    {
        SetY();
        SetX();
        UpdateCursorState();
    }

    void SetY()
    {
        float t_Input = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
        Quaternion t_adj = Quaternion.AngleAxis(t_Input, -Vector3.right);
        Quaternion t_Delta = cams.localRotation * t_adj; // to add the quaternion, multiple the quaternions

        if(Quaternion.Angle(camsCenter, t_Delta) < maxAngle) cams.localRotation = t_Delta;

    }

    void SetX()
    {
        float t_Input = Input.GetAxis("Mouse X") * ySensitivity * Time.deltaTime;
        Quaternion t_adj = Quaternion.AngleAxis(t_Input, Vector3.up);
        Quaternion t_Delta = player.localRotation * t_adj; // to add the quaternion, multiple the quaternions

         player.localRotation = t_Delta;

    }

    void UpdateCursorState()
    {
        if (cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLocked = false;
            }
        }

        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLocked = true;
            }
        }
    }
}//class











































