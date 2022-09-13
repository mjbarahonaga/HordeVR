using System.Collections;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UnityEngine;
using UnityEngine.UI;
using MEC;
using UltimateXR.Extensions.Unity.Render;

public class PlayerController : MonoBehaviour
{
    public int HPMax = 100;
    private int _currentHP;
    private bool _isDie = false;
    public Slider HealthBar;

    public AudioClip Hit;
    public AudioClip Dead;

    

    public void TakeDamage(int damage)
    {
        _currentHP -= damage;
        // Call sfx sound
        HealthBar.value = Mathf.Clamp(_currentHP, 0, HPMax);
        if(_currentHP <= 0 && !_isDie)
        {
            _isDie = true;
            StartCoroutine(Dying());
            //DIE
        }
    }

    private IEnumerator Dying()
    {
        // Call sfx sound
        yield return UxrAvatar.LocalAvatar.CameraFade.StartFadeCoroutine(2, 
            UxrManager.Instance.TeleportFadeColor.WithAlpha(0f), 
            UxrManager.Instance.TeleportFadeColor.WithAlpha(1f));


    }

    #region UNITY
    private void Start()
    {
        _currentHP = HPMax;
        HealthBar.minValue = 0;
        HealthBar.maxValue = HPMax;
        _isDie = false;

        EnemyBehaviour.OnSendAttack += TakeDamage;
    }

    private void OnDestroy()
    {
        EnemyBehaviour.OnSendAttack -= TakeDamage;
    }
    #endregion
}
