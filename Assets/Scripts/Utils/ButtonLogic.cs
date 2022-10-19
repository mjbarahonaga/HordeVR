using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class ButtonLogic : MonoBehaviour
{
    [SerializeField, ReadOnly] private Rigidbody _rb;

    public Vector3 Axis;

    [Tooltip("How far the button must travel to become pressed.")]
    public float Threshold;
    [Tooltip("Threshold to hit on the return to allow the button to be pressed again.")]
    public float UpThreshold;
    [Tooltip("The resting position of the button")]
    public Vector3 StartPosition;
    public bool IsPressed = false;

    public UnityEvent OnButtonDown;
    public UnityEvent OnButtonUp;

    private void Start()
    {
        if (StartPosition == Vector3.zero)
        {
            StartPosition = transform.localPosition;
        }
    }

    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if(this == null) return;

            if(_rb == null) _rb = GetComponent<Rigidbody>();

        });
    }

    private void FixedUpdate()
    {
        var distance = (StartPosition - transform.localPosition).magnitude;

        if (!IsPressed && distance >= Threshold)
        {
            IsPressed = true;
            OnButtonDown.Invoke();
        }
        else if (IsPressed && distance < UpThreshold)
        {
            IsPressed = false;
            OnButtonUp.Invoke();
        }

        ClampBounds();
    }

    private void ClampBounds()
    {
        var test = new Vector3(transform.localPosition.x * Axis.x, transform.localPosition.y * Axis.y, transform.localPosition.z * Axis.z);

        if (test.x > StartPosition.x || test.y > StartPosition.y || test.z > StartPosition.z)
        {
            transform.localPosition = StartPosition;
            _rb.velocity = Vector3.zero;
        }
    }

    [Button("StartPosition")]
    public void SaveStartPosition() => StartPosition = transform.localPosition;

    [Button("Threshold")]
    public void SaveThreshold() => Threshold = (StartPosition - transform.localPosition).magnitude;

    [Button("UpThreshold")]
    public void SaveUpThreshold()
    {
        var distance = (StartPosition - transform.localPosition).magnitude;
        if (distance < Threshold) 
        {
            UpThreshold = distance;
            return;
        }
        Debug.LogError("UpThreshold must be shorter than Threshold");
    }

    [Button("Return to StartPosition")]
    public void ReturntSP() => transform.localPosition = StartPosition;
}
