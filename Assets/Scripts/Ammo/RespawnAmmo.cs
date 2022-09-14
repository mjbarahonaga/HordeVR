using System.Collections;
using System.Collections.Generic;
using UltimateXR.Manipulation;
using UnityEngine;
using MEC;
using System;

[RequireComponent(typeof(UxrGrabbableObjectAnchor))]
public class RespawnAmmo : MonoBehaviour
{
    public UxrGrabbableObject PrefabAmmo;
    public UxrGrabbableObjectAnchor Anchor;
    public float TimeToRespawn = 40f;   //< 40 seconds

    private CoroutineHandle _coroutine;


    private void Anchor_Removed(object sender, UxrManipulationEventArgs e)
    {
        _coroutine = Timing.RunCoroutine(RespawnCoroutine(TimeToRespawn),Segment.SlowUpdate);
    }

    private void Anchor_Placed(object sender, UxrManipulationEventArgs e)
    {
        Timing.KillCoroutines(_coroutine);
    }

    private IEnumerator<float> RespawnCoroutine(float time)
    {
        yield return Timing.WaitForSeconds(time);
        var go = Instantiate(PrefabAmmo, transform);
        go.transform.position = Anchor.transform.position;

        if (go.CanBePlacedOnAnchor(Anchor))
        {
           UxrGrabManager.Instance.PlaceObject(go, Anchor, UxrPlacementType.Immediate, false);
        }
    }


    #region UNITY

    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;

            if(Anchor == null) Anchor = GetComponent<UxrGrabbableObjectAnchor>();
        });
    }

    private void Start()
    {
        Anchor.Removed += Anchor_Removed;
        Anchor.Placed += Anchor_Placed;
    }

    private void OnDestroy()
    {
        Anchor.Removed -= Anchor_Removed;
        Anchor.Placed -= Anchor_Placed;
    }
    #endregion
}
