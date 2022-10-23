using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UltimateXR.UI;
using UnityEngine;
using UnityEngine.Assertions;
using MEC;
using UltimateXR.Avatar.Controllers;
using UltimateXR.Devices;
using System;
using UltimateXR.Core;

[RequireComponent(typeof(LineRenderer))]
public class BuildingTrap : MonoBehaviour
{
    public UxrStandardAvatarController MyController;
    public Transform Laser = null;
    public float MaxDistance = 10f;

    [SerializeField, ReadOnly] int m_TrapLayer;
    [ReadOnly, SerializeField] LineRenderer m_LineRenderer = null;

    private bool m_ItWasValid = false;
    private CoroutineHandle m_Coroutine;
    private TypeTrap m_TypeTrap;
    private TypeSurface m_TypeSurface;
    private RaycastHit m_Hit;
    public TypeTrap SelectedTypeTrap
    {
        get { return m_TypeTrap; }
        set { m_TypeTrap = value; }
    }

    private void ReceiveSelection(TypeTrap type)
    {
        if (type == TypeTrap.None)
        {
            UnselectTrap();
            return;
        }
        SelectTypeTrap(type);
    }

    public void SelectTypeTrap(TypeTrap type)
    {
        UnselectTrap(); // to avoid replications

        SelectedTypeTrap = type;
        GhostManager.Instance.SelectedGhost(type);
        m_TypeSurface = GameManager.Instance.GetValidSurfaces(type);
        m_LineRenderer.enabled = true;
        m_Coroutine = Timing.RunCoroutine(CheckingCoroutine(),Segment.FixedUpdate);
    }

    public void UnselectTrap()
    {
        SelectedTypeTrap = TypeTrap.None;
        GhostManager.Instance.UnselectedGhost();
        m_LineRenderer.enabled = false;
        Timing.KillCoroutines(m_Coroutine);
    }

    IEnumerator<float> CheckingCoroutine()
    {
        GhostManager.Instance.SetSelectedVisible(true);
        bool validCollider = m_ItWasValid;
        ToggleState(false);
        while (true)
        {
            if (Physics.Raycast(Laser.position, Laser.forward, out m_Hit, MaxDistance, m_TrapLayer))
            {
                validCollider = false;
                switch (m_TypeSurface)
                {
                    case TypeSurface.None:
                        break;
                    case TypeSurface.Floor:
                        if (m_Hit.collider.CompareTag("Floor"))
                        {
                            validCollider = true;
                            ToggleState(true);
                        }
                        break;
                    case TypeSurface.Ceiling:
                        if (m_Hit.collider.CompareTag("Ceiling"))
                        {
                            validCollider = true;
                            ToggleState(true);
                        }
                        break;
                    case TypeSurface.Wall:
                        if (m_Hit.collider.CompareTag("Wall"))
                        {
                            validCollider = true;
                            ToggleState(true);
                        }
                        break;
                    case TypeSurface.Everything:
                        if (m_Hit.collider.CompareTag("Floor") ||
                            m_Hit.collider.CompareTag("Wall") ||
                            m_Hit.collider.CompareTag("Ceiling"))
                        {
                            validCollider = true;
                            ToggleState(true);
                        }

                        break;
                }

                

                var rot = Quaternion.FromToRotation(Vector3.up, m_Hit.normal);

                UpdatePositionLine(m_Hit.point);
                UpdateTransformGhost(m_Hit.point, rot);
                
                if (GhostManager.Instance.IsCollidingWithOtherTrap()) { validCollider = false; }
                
                if (validCollider == false) ToggleState(false);
            }
            yield return Timing.WaitForOneFrame;
        }
    }

    private void PlaceTrap(object sender, UxrInputButtonEventArgs e)
    {
        if (m_ItWasValid == false)
        {
            // TODO: Feedback
            return;
        }

        if (e.HandSide != UxrHandSide.Right) return;

        if (e.Button == UxrInputButtons.Trigger &&
            e.ButtonEventType == UxrButtonEventType.PressDown)
        {

            m_ItWasValid = false;
            GhostManager.Instance.CurrentSelected.GetPosAndRot(out Vector3 pos, out Quaternion rot);
            GameManager.Instance.CheckToPlace(m_TypeTrap, pos, rot);
        }

    }

    public void UpdateTransformGhost(Vector3 pos, Quaternion rot)
    {
        GhostManager.Instance.SetSelectedPosition(pos);
        GhostManager.Instance.SetSelectedRotation(rot);
    }

    public void UpdateMaterialLine(bool isValid)
    {
        m_LineRenderer.material = isValid ? GhostManager.Instance.Valid : GhostManager.Instance.Invalid;
    }
    public void UpdatePositionLine(Vector3 endPoint)
    {
        m_LineRenderer.SetPosition(0,Laser.position);
        m_LineRenderer.SetPosition(1,endPoint);
    }

    public void ToggleState(bool current)
    {
        if (current == m_ItWasValid) return;    //< It will change just if it changed its state
        UpdateMaterialLine(current);
        GhostManager.Instance.ToggleMaterial(current);
        m_ItWasValid = current;
    }
    
    #region UNITY METHODS
    private void Start()
    {
        Assert.IsNotNull(Laser);    //< We expect to have this reference
        m_TrapLayer = 1 << 10 ;
        m_TrapLayer = ~m_TrapLayer; // Hardcoded, to ignore layer traps
        MyController.Avatar.ControllerInput.ButtonStateChanged += PlaceTrap;
        InventorySelection.OnSendTypeSelected += ReceiveSelection;
        Inventory.CloseInventory += UnselectTrap;
    }

    private void OnDestroy()
    {
        InventorySelection.OnSendTypeSelected -= ReceiveSelection;
        Inventory.CloseInventory -= UnselectTrap;
    }

    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;
            if (m_LineRenderer == null) m_LineRenderer = GetComponent<LineRenderer>();
            if (Laser)
            {
                m_LineRenderer.SetPosition(0, Laser.position);
                m_LineRenderer.SetPosition(1, Laser.position + Laser.forward * 10f);
            }
            m_TrapLayer = 1 << 10;
            m_TrapLayer = ~m_TrapLayer;
            if (MyController == null) MyController = FindObjectOfType<UxrStandardAvatarController>();
        });
    }

    [Button("SelectTrapSpikes")]
    public void TestSelectTrap() => SelectTypeTrap(TypeTrap.Spikes);

    [Button("UnselectTrapSpikes")]
    public void TestUnselectTrap() => UnselectTrap();
#endif

}