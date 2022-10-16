using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

[RequireComponent(typeof(BoxCollider))]
public class BulletBehaviour : MonoBehaviour
{
    public DataBullet Data;
    public PoolBullet PoolReference;
    
    [SerializeField] private float _lifeTime = 5f;
     private float _startTime = 0f;
    [SerializeField, ReadOnly] private Vector3 _direction;
    [SerializeField, ReadOnly] private float _speed;
    [SerializeField, ReadOnly] private int _damage;

    [SerializeField, ReadOnly] private BoxCollider _collider;
    [SerializeField, ReadOnly] private Transform _myTransform;

    CoroutineHandle _updateCoroutine;

    public void Init(Vector3 pos, Vector3 dir)
    {
        _direction = dir.normalized;
        _myTransform.position = pos;
        _myTransform.forward = _direction;
        _startTime = Time.time;
        _updateCoroutine = Timing.RunCoroutine(
            Utils.EmulateUpdate(MyUpdate, this), 
            Segment.LateUpdate
            );
    }
    public void IntanceBullet()
    {
        _collider = GetComponent<BoxCollider>();
        _myTransform = GetComponent<Transform>();

        _collider.enabled = false;

        SetUpBullet();

        gameObject.SetActive(false);
    }
    public void SetUpBullet()
    {
        _speed = Data.Speed;
        _damage = Data.Damage;
    }
    public void TakeFromPool()
    {
        gameObject.SetActive(true);
        _collider.enabled = true;
        SetUpBullet();
    }
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        _collider.enabled = false;
        Timing.KillCoroutines(_updateCoroutine);
    }

    public void MyUpdate()
    {
        _myTransform.Translate(_direction * _speed * Time.deltaTime ,Space.World);

        if(Time.time - _startTime >= _lifeTime) PoolReference.Pool.Release(this);
    }

    // Only enter with layers enemy and wall
    private void OnCollisionEnter(Collision collision)
    {
        // Case Enemy
        if(collision.gameObject.TryGetComponent(out EnemyBehaviour enemy))
        {
            if (enemy.IsDie) return;
            enemy.TakeDamage(_damage);
        }
        // Case Surface
        PoolReference.Pool.Release(this);
    }
}
