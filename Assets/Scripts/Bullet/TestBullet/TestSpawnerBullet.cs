using System;
using System.Collections;
using System.Collections.Generic;
using UltimateXR.Avatar.Controllers;
using UltimateXR.Devices;
using UnityEngine;

public class TestSpawnerBullet : MonoBehaviour
{
    public UxrStandardAvatarController MyController;

    private void Start()
    {
        MyController = FindObjectOfType<UxrStandardAvatarController>();
        MyController.Avatar.ControllerInput.ButtonStateChanged += Execute;
    }

    private void Execute(object sender, UxrInputButtonEventArgs e)
    {
        if(e.HandSide == UltimateXR.Core.UxrHandSide.Right 
            && e.Button == UxrInputButtons.Trigger 
            && e.ButtonEventType == UxrButtonEventType.PressDown)
        {
            PoolBullet_Manager.Instance.SpawnBullet(Bullet.Default, transform.position, transform.forward);
        }
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        PoolBullet_Manager.Instance.SpawnBullet(Bullet.Default, transform.position, transform.forward);
    //    }
    //}
}
