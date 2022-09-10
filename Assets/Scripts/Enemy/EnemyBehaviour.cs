using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Sirenix.OdinInspector;
using UnityEngine.AI;
using MEC;

public class EnemyBehaviour : MonoBehaviour
{
    public DataEnemy Data;
    public PoolEnemy PoolReference;
    [SerializeField, ReadOnly] public GameObject InstantiatePrefab;
    [SerializeField, ReadOnly] private IObjectPool<EnemyBehaviour> _pool;
    [SerializeField, ReadOnly] private Animator _animator;
    [SerializeField, ReadOnly] private NavMeshAgent _navMeshAgent;
    [SerializeField] private Transform _myTransform;
    [SerializeField, ReadOnly] private Transform _myParentTransform;

    [SerializeField, ReadOnly] private int _idSpawn;
    [SerializeField, ReadOnly] private int _idWalk;
    [SerializeField, ReadOnly] private int _idDie;
    [SerializeField, ReadOnly] private int _idAttack;
    [SerializeField, ReadOnly] private int _idHit;

    [SerializeField, ReadOnly] private Vector3 _targetPos;
    [SerializeField, ReadOnly] private int _currentHP;
    [SerializeField, ReadOnly] private int _reward;

    public void Init(in Vector3 target, in Transform startPos)
    {
        _targetPos = target;
        //_navMeshAgent.isStopped = true;

        _myParentTransform.localPosition = startPos.position;
        _myTransform.localPosition = Vector3.zero;
        _navMeshAgent.Warp(_myParentTransform.position);
        _myTransform.LookAt(target);
    }
    public void InstanceEnemy(IObjectPool<EnemyBehaviour> pool, GameObject prefab)
    {
        InstantiatePrefab = prefab;
        _pool = pool;

        _navMeshAgent = InstantiatePrefab.GetComponentInChildren<NavMeshAgent>();
        _animator = InstantiatePrefab.GetComponentInChildren<Animator>();
        if (_animator)
        {
            _idSpawn = Animator.StringToHash("Spawn");
            _idWalk = Animator.StringToHash("Walk");
            _idDie = Animator.StringToHash("Die");
            _idAttack = Animator.StringToHash("Attack");
            _idHit = Animator.StringToHash("Hit");
        }
        //_myTransform = GetComponent<Transform>();
        _myParentTransform = InstantiatePrefab.GetComponent<Transform>();
        _myTransform.localPosition = Vector3.zero;
        SetUpEnemy();

        InstantiatePrefab.SetActive(false);
    }

    public void TakeFromPool()
    {

        InstantiatePrefab.SetActive(true);
        _animator.SetTrigger(_idSpawn);
        SetUpEnemy();
    }

    public void ReturnToPool()
    {
        InstantiatePrefab.SetActive(false);
    }

    public void SetUpEnemy()
    {
        _navMeshAgent.speed = Data.Speed;
        _reward = Data.Reward;
        _currentHP = Data.HP;
    }

    public void TakeDamage(int damage)
    {
        _currentHP -= damage;
        if (_currentHP <= 0)
        {
            Timing.RunCoroutine(Dying());
        }
    }

    public IEnumerator<float> Dying()
    {
        //GameManager.Instance.AddPoints(_reward);
        _navMeshAgent.isStopped = true;
        _animator.SetTrigger(_idDie);
        yield return Timing.WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        PoolReference.Pool.Release(this);
    }

    [Button("Release")]
    public void prueba() => PoolReference.Pool.Release(this);
    #region UNITY

    #endregion
}
