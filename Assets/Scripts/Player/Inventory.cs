using System;
using System.Collections;
using System.Collections.Generic;
using UltimateXR.Avatar.Controllers;
using UltimateXR.Core;
using UltimateXR.Devices;
using UnityEngine;
using MEC;

public class Inventory : MonoBehaviour
{
    public UxrStandardAvatarController MyController;
    public Canvas InventoryCanvas;
    public Transform LocationToShow;
    private Camera m_ReferenceCamera;
    private CoroutineHandle m_Coroutine;

    private void LeftGrip_Inventory(object sender, UxrInputButtonEventArgs e)
    {
        if (e.HandSide == UxrHandSide.Right) return;

        if (e.Button == UxrInputButtons.Grip &&
            e.ButtonEventType == UxrButtonEventType.Pressing)
        {
            if (InventoryCanvas.enabled) return;
            InventoryCanvas.enabled = true;
            m_Coroutine = Timing.RunCoroutine(BillboardInventoryCoroutine());
            return;
        }

        if (e.Button == UxrInputButtons.Grip &&
            e.ButtonEventType == UxrButtonEventType.PressUp)
        {
            InventoryCanvas.enabled = false;
            Timing.KillCoroutines(m_Coroutine);
        }
    }

    IEnumerator<float> BillboardInventoryCoroutine()
    {
        while (true)
        {
            InventoryCanvas.transform.position = LocationToShow.position;
            InventoryCanvas.transform.LookAt(m_ReferenceCamera.transform.position);
            InventoryCanvas.transform.forward = -InventoryCanvas.transform.forward;
            yield return Timing.WaitForOneFrame;
        }
    }

    #region UNITY METHODS
    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;

            if (MyController == null) MyController = FindObjectOfType<UxrStandardAvatarController>();
            if (m_ReferenceCamera == null) m_ReferenceCamera = FindObjectOfType<Camera>();
        });
    }

    public void Start()
    {
        MyController.Avatar.ControllerInput.ButtonStateChanged += LeftGrip_Inventory;
        InventoryCanvas.enabled = false;
    }
    #endregion
}
