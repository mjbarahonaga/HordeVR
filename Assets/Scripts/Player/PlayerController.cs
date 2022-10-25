using System.Collections;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UnityEngine;
using UnityEngine.UI;
using MEC;
using UltimateXR.Extensions.Unity.Render;
using Sirenix.OdinInspector;
using UltimateXR.Animation.UI;
using System;

public class PlayerController : MonoBehaviour
{
    public static Action KilledPlayer;
    public static Action ResurrectedPlayer;

    public int HPMax = 100;
    private int _currentHP;
    private bool _isDie = false;
    public Slider HealthBar;

    public AudioClip Hit;
    public AudioClip Dead;

    [Title("Hit")]
    public Color ColorHit;
    public float IntensityColorHit = 0.3f;
    public float TimeFadeHit = 0.5f;
    [Title("Dead")]
    public Color ColorDead;
    public float IntensityColorDead = 0.5f;
    public float TimeFadeDead = 15f;

    private bool _isHitted;
    CoroutineHandle _coroutineHit;
    CoroutineHandle _coroutineDied;
    public bool IsDie { get { return _isDie; } }

    public void TakeDamage(GameObject obj, int damage)
    {
        if (obj != this.gameObject) return;
        if (_isDie) return;
        _currentHP -= damage;

        // Call sfx sound
        HealthBar.value = Mathf.Clamp(_currentHP, 0, HPMax);
        if(_currentHP <= 0 && !_isDie)
        {
            _isDie = true;
            KilledPlayer?.Invoke();
            StartCoroutine(Dying());
            return;
        }

        if (_isHitted) Timing.KillCoroutines(_coroutineHit);
        _coroutineHit = Timing.RunCoroutine(HitCoroutine());
    }

    private IEnumerator Dying()
    {
        float quality = IntensityColorDead;
        UxrAvatar.LocalAvatar.CameraFade.EnableFadeColor(ColorDead, quality);
        float currentTime = 0f;
        float div = 0f;
        while (currentTime <= TimeFadeDead)
        {
            currentTime += Time.deltaTime;
            div = currentTime / TimeFadeDead;
            quality = Mathf.Lerp(IntensityColorDead, 0f, div);
            HealthBar.value = Mathf.Lerp(0, HPMax, div);
            UxrAvatar.LocalAvatar.CameraFade.EnableFadeColor(ColorDead, quality);
            yield return Timing.WaitForOneFrame;
        }
        _currentHP = HPMax;
        UxrAvatar.LocalAvatar.CameraFade.DisableFadeColor();
        _isDie = false;
        ResurrectedPlayer?.Invoke();
        // Call sfx sound
        //yield return UxrAvatar.LocalAvatar.CameraFade.StartFadeCoroutine(2, 
        //    UxrManager.Instance.TeleportFadeColor.WithAlpha(0f), 
        //    UxrManager.Instance.TeleportFadeColor.WithAlpha(1f));

        //GameManager.Instance.EndGame();
    }

    private IEnumerator<float> HitCoroutine()
    {
        _isHitted = true;
        float quality = IntensityColorHit;
        UxrAvatar.LocalAvatar.CameraFade.EnableFadeColor(ColorHit, quality);
        float currentTime = 0f;
        while(currentTime <= TimeFadeHit)
        {
            currentTime += Time.deltaTime;
            quality = Mathf.Lerp(IntensityColorHit, 0f, currentTime / TimeFadeHit);
            UxrAvatar.LocalAvatar.CameraFade.EnableFadeColor(ColorHit, quality);
            yield return Timing.WaitForOneFrame;
        }
        UxrAvatar.LocalAvatar.CameraFade.DisableFadeColor();
        _isHitted = false;
        yield return 0f;
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

    #region TEST
#if UNITY_EDITOR
    [Button("TestDie")]
    public void TestDie() => StartCoroutine(Dying());

    public int TestHitDamage = 100;
    [Button("TestHit")]
    public void TestHit() => TakeDamage(this.gameObject, TestHitDamage);
#endif
#endregion
}
