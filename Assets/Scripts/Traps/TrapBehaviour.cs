using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BoxCollider))]
public class TrapBehaviour : MonoBehaviour
{
    public DataTrap Data;

    #region Private variables
    private int _damage;
    private float _coolDown;
    private float _price;

    private bool _isReadyToAttack = true;

    private CoroutineHandle _updateCoroutine;

    [SerializeField, ReadOnly]
    private Animator _animator;
    private int _idAttack;

    // list of enemies inside of trap
    private List<EnemyBehaviour> _enemiesInside = new List<EnemyBehaviour>();
    #endregion

    #region Getter and Setters
    [ShowInInspector, ReadOnly]
    public int Damage { get => _damage; private set => _damage = value; }
    [ShowInInspector, ReadOnly]
    public float CoolDown { get => _coolDown; private set => _coolDown = value; }
    [ShowInInspector, ReadOnly]
    public float Price { get => _price; private set => _price = value; }
    #endregion

    public void SetUp(DataTrap data)
    {
        _damage = data.Damage;
        _coolDown = data.CoolDown;
        _price = data.Price;
    }
    
    private IEnumerator<float> MyUpdateCoroutine()
    {
        yield return Timing.WaitForSeconds(_coolDown);
        if(_enemiesInside.Count == 0)
        {
            _isReadyToAttack = true;
        }
        else
        {
            Attack(_enemiesInside);
        }
        yield return 0f;
    }

    public void Attack(List<EnemyBehaviour> enemies)
    {
        // Call animation
        _animator.SetTrigger(_idAttack);

        _isReadyToAttack = false;
        int length = _enemiesInside.Count;
        for (int i = 0; i < length; ++i)
        {
            enemies[i].TakeDamage(_damage);
            if (enemies[i].IsDie)
            {
                enemies.Remove(enemies[i]);
            }
        }

        _updateCoroutine = Timing.RunCoroutine(MyUpdateCoroutine());
    }

    #region Unity Methods

    private void Start()
    {
        _idAttack = Animator.StringToHash("Attack");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out EnemyBehaviour enemy))
        {
            if (!_enemiesInside.Contains(enemy))
            {
                _enemiesInside.Add(enemy);
            }
        }

        if (_isReadyToAttack)
        {
            Attack(_enemiesInside);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out EnemyBehaviour enemy))
        {
            if (_enemiesInside.Contains(enemy))
            {
                _enemiesInside.Remove(enemy);
            }
        }
    }

    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;
            if (Data != null) SetUp(Data);
            if(_animator == null) _animator = GetComponent<Animator>();
        });
    }
    #endregion
}
