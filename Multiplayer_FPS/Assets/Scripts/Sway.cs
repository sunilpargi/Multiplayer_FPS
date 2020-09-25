using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Sway : MonoBehaviourPunCallbacks
{
    #region varibles
    public float intensity;
    public float smooth;
    public bool Ismine;

    private Quaternion originRotation;
    #endregion



    #region monobehaviour callbacks
    private void Start()
    {
        originRotation = transform.localRotation;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        UpdateSway();
    }
    #endregion



    #region private method

    void UpdateSway()
    {
        //controls
        float t_x_mouse = Input.GetAxis("Mouse X");
        float t_y_mouse = Input.GetAxis("Mouse Y");

        if (!Ismine)
        {
            t_x_mouse = 0;
            t_y_mouse = 0;
        }

        //calculate target rotation
        Quaternion t_x_axis = Quaternion.AngleAxis(-intensity * t_x_mouse , Vector3.up);
        Quaternion t_y_axis = Quaternion.AngleAxis(intensity * t_y_mouse , Vector3.right);
        Quaternion target_rotation = originRotation * t_x_axis * t_y_axis;

        //rotate towards target rotation
        transform.localRotation = Quaternion.Lerp(transform.localRotation, target_rotation, Time.deltaTime * smooth);

    }

    #endregion
}
