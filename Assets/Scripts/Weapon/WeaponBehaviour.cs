using System;
using System.Collections;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Devices;
using UltimateXR.Manipulation;
using UltimateXR.Mechanics.Weapons;
using UnityEngine;

public class WeaponBehaviour : MonoBehaviour
{
    public Bullet BulletType;
    public bool InfiniteBullets = false;

    public Transform LocationToShoot;

    public UxrGrabbableObject MyWeapon;
    public UxrGrabbableObjectAnchor MyAmmoAnchor;

    public UxrFirearmMag MyAmmo;

    public AudioSource MyAudioSource;
    public AudioClip ShootAudio;
    public AudioClip NoBulletsAudio;

    private void AmmoTarget_Removed(object sender, UxrManipulationEventArgs e)
    {
        MyAmmo = null;
    }

    private void AmmoTarget_Placed(object sender, UxrManipulationEventArgs e)
    {
        if (e.GrabbableAnchor == MyAmmoAnchor)
        {
            MyAmmo = e.GrabbableObject.GetComponentInChildren<UxrFirearmMag>();
        }
    }

    private void UxrManager_AvatarsUpdated()
    {
        if(MyWeapon && UxrGrabManager.Instance.GetGrabbingHand(MyWeapon,0, out UxrGrabber grabber))
        {
            bool triggerPressDown = UxrAvatar.LocalAvatarInput.GetButtonsPressDown(grabber.Side, UxrInputButtons.Trigger);

            if (triggerPressDown)
            {
                Shoot();
            }
        }
    }

    private void FirstTimeGrabbed()
    {
        if(MyWeapon && UxrGrabManager.Instance.GetGrabbingHand(MyWeapon,0, out UxrGrabber grabber))
        {
            GameManager.Instance.StartGame();
            UxrManager.AvatarsUpdated -= FirstTimeGrabbed;
        }
    }

    private void Shoot()
    {
        if (GameManager.Instance.Player.IsDie) return;
        if((MyAmmo && MyAmmo.Rounds > 0) || InfiniteBullets)
        {
            MyAmmo.Rounds--;
            PoolBullet_Manager.Instance.SpawnBullet(BulletType, LocationToShoot.position, LocationToShoot.forward);
            
            MyAudioSource.clip = ShootAudio;
            MyAudioSource.Play();
        }
        else
        {
            MyAudioSource.clip = NoBulletsAudio;
            MyAudioSource.Play();
        }
    }

    //public void FirstTimeGrabbed()

    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;

            if (MyWeapon == null) MyWeapon = GetComponent<UxrGrabbableObject>();
            if (MyAmmoAnchor == null) MyAmmoAnchor = GetComponentInChildren<UxrGrabbableObjectAnchor>();
            if (MyAudioSource == null) MyAudioSource = GetComponent<AudioSource>();
        });
    }

    private void OnEnable()
    {
        UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;
        //UxrManager.AvatarsUpdated += FirstTimeGrabbed;
        MyAmmoAnchor.Placed += AmmoTarget_Placed;
        MyAmmoAnchor.Removed += AmmoTarget_Removed;
    }



    private void OnDisable()
    {
        MyAmmoAnchor.Placed -= AmmoTarget_Placed;
        MyAmmoAnchor.Placed -= AmmoTarget_Removed;
    }
}
