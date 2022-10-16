using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ghost
{
    public TypeTrap Type;
    public GameObject Trap;
    [ReadOnly] public MeshRenderer[] Renderers;
    [ReadOnly] public Transform CachedTransform;
    [ReadOnly] public BoxCollider CachedCollider;

    public void ChangeMaterial(Material mat)
    {
        int length = Renderers.Length;
        for (int i = 0; i < length; ++i)
        {
            Renderers[i].material = mat;
        }
    }
    public void GetPosAndRot(out Vector3 pos, out Quaternion rot)
    {
        pos = CachedTransform.position;
        rot = CachedTransform.rotation;
    }
    public void SetPosition(Vector3 pos) => CachedTransform.position = pos;
    public void SetRotation(Quaternion rot) => CachedTransform.rotation = rot;
}

public class GhostManager : Singleton<GhostManager>
{
    public Action<TypeTrap> OnTrapSelected;
    public Material Valid;
    public Material Invalid;

    public List<Ghost> Ghosts = new();

    private Ghost m_CurrentSelected;
    [SerializeField, ReadOnly] LayerMask m_TrapLayer;

    [ShowInInspector, ReadOnly]
    public Ghost CurrentSelected 
    { 
        get { return m_CurrentSelected; }
        private set { m_CurrentSelected = value; }
    }


    #region METHODS
    public void SelectedGhost(TypeTrap type)
    {
        int length = Ghosts.Count;
        for (int i = 0; i < length; ++i)
        {
            if (Ghosts[i].Type == type)
            {
                if (m_CurrentSelected != null) UnselectedGhost();

                m_CurrentSelected = Ghosts[i];
                m_CurrentSelected.Trap.SetActive(true);
                break;
            }
        }
    }

    public void UnselectedGhost()
    {
        m_CurrentSelected?.Trap.SetActive(false);
        m_CurrentSelected = null;
    }

    public void SetSelectedVisible(bool isVisible)
    {
        m_CurrentSelected?.Trap.SetActive(isVisible);
    }
    
    public void SetSelectedPosition(Vector3 pos)
    {
        m_CurrentSelected?.SetPosition(pos);
    }
    
    public void SetSelectedRotation(Quaternion rot)
    {
        m_CurrentSelected?.SetRotation(rot);
    }

    public void ToggleMaterial(bool isValid)
    {
        CurrentSelected?.ChangeMaterial(isValid ? Valid : Invalid);
    }

    public bool IsCollidingWithOtherTrap()
    {
        if(m_CurrentSelected == null) return false;
        var center = m_CurrentSelected.CachedTransform.TransformPoint(m_CurrentSelected.CachedCollider.center);
        var halfSize = m_CurrentSelected.CachedTransform.TransformVector(m_CurrentSelected.CachedCollider.size) / 2f;
        var colliders = Physics.OverlapBox(center, halfSize, Quaternion.identity, m_TrapLayer);
        return colliders.Length > 1;
    }
    #endregion


    #region UNITY METHODS

    private void Start()
    {
        m_TrapLayer = LayerMask.GetMask("Traps");
    }

    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;

            int length = Ghosts.Count;
            for (int i = 0; i < length; i++)
            {
                Ghosts[i].Renderers = Ghosts[i].Trap.GetComponentsInChildren<MeshRenderer>();
                Ghosts[i].CachedTransform = Ghosts[i].Trap.GetComponent<Transform>();
                Ghosts[i].CachedCollider = Ghosts[i].Trap.GetComponent<BoxCollider>();
                Ghosts[i].ChangeMaterial(Valid);
            }
            m_TrapLayer = LayerMask.GetMask("Traps");
        });
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Ghosts.Count == 0) return;

        var center = Ghosts[0].CachedTransform.TransformPoint(Ghosts[0].CachedCollider.center);
        var halfSize = Ghosts[0].CachedTransform.TransformVector(Ghosts[0].CachedCollider.size);
        Gizmos.color = Color.red;   
        Gizmos.DrawWireCube(center, halfSize);
    }
#endif
    #endregion
}
