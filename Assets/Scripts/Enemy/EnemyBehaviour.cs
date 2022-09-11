using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Sirenix.OdinInspector;
using UnityEngine.AI;
using MEC;
using UltimateXR.Core;
using UltimateXR.Avatar;
using System;

public class EnemyBehaviour : MonoBehaviour
{
    public DataEnemy Data;
    public PoolEnemy PoolReference;
    public float DistanceAttacking = 1f;
    [SerializeField] private Transform _myTransform;
    [SerializeField, ReadOnly] public GameObject InstantiatePrefab;
    [SerializeField, ReadOnly] private IObjectPool<EnemyBehaviour> _pool;
    [SerializeField, ReadOnly] private Animator _animator;
    [SerializeField, ReadOnly] private NavMeshAgent _navMeshAgent;
    [SerializeField, ReadOnly] private Transform _myParentTransform;

    [SerializeField, ReadOnly] private int _idSpawn;
    [SerializeField, ReadOnly] private int _idWalk;
    [SerializeField, ReadOnly] private int _idDie;
    [SerializeField, ReadOnly] private int _idAttack;
    [SerializeField, ReadOnly] private int _idHit;

    [SerializeField, ReadOnly] private Vector3 _targetPos;
    [SerializeField, ReadOnly] private int _currentHP;
    [SerializeField, ReadOnly] private int _reward;
    [SerializeField, ReadOnly] private bool _isDie = false;
    [SerializeField, ReadOnly] private bool _isAttacking = false;
    [SerializeField, ReadOnly] private bool _isWalking = false;

    private CoroutineHandle _updateCoroutine;

    public void Init(in Vector3 target, in Transform startPos)
    {
        _targetPos = target;
        
        _myParentTransform.localPosition = startPos.position;
        _myTransform.localPosition = Vector3.zero;
        _navMeshAgent.Warp(_myParentTransform.position);
        _myTransform.LookAt(target);

        Timing.RunCoroutine(Spawn());
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
        _navMeshAgent.stoppingDistance = DistanceAttacking;
        _reward = Data.Reward;
        _currentHP = Data.HP;
    }

    public bool TakeDamage(int damage)
    {
        if (_isDie) return false;

        _currentHP -= damage;

        if (_currentHP <= 0)
        {
            _isDie = true;
            Timing.RunCoroutine(Dying());
            return true;
        }
        Timing.RunCoroutine(Hit());
        return true;
    }

    public void MyUpdate()
    {
        if (_isDie) return;
        if (ReachedDestinationOrGaveUp())
        {
            if (!_isAttacking)
            {
                Attack();
            }
        }
        else if(!_isAttacking && !_isWalking)
        {
            Timing.RunCoroutine(Walking());
        }else
        {
            _isWalking = false;
            _isAttacking = false;
        }
    }

    public bool ReachedDestinationOrGaveUp()
    {

        if (!_navMeshAgent.pathPending)
        {
            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                //if (_navMeshAgent.velocity.sqrMagnitude == 0f)
                if ((_targetPos - _myTransform.position).sqrMagnitude <= (DistanceAttacking + 1.5f))
                {
                    //if ((_targetPos - _myTransform.position).sqrMagnitude < DistanceAttacking)
                        return true;
                }
            }
        }

        return false;
    }

    public void Attack()
    {
        Timing.RunCoroutine(Attacking());
    }

    public void CheckAttack()
    {
        if((_targetPos - _myTransform.position).sqrMagnitude < DistanceAttacking)
        {
            Debug.Log("successful attack");
        }
    }

    public IEnumerator<float> Spawn()
    {
        _isDie = false;
        _animator.SetTrigger(_idSpawn);
        float length = _animator.GetCurrentAnimatorStateInfo(0).length;
        if (length == float.PositiveInfinity) length = 1f;
        yield return Timing.WaitForSeconds(length);
        _updateCoroutine = Timing.RunCoroutine(Utils.EmulateUpdate(MyUpdate, this), Segment.LateUpdate);
        _isWalking = false;
    }

    public IEnumerator<float> Hit()
    {
        _isWalking = false;
        _isAttacking = false;
        _navMeshAgent.isStopped = true;
        _animator.SetTrigger(_idHit);
        yield return Timing.WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
    }

    public IEnumerator<float> Dying()
    {
        Timing.KillCoroutines(_updateCoroutine);
        GameManager.Instance.EnemyDie(Data.EnemyType, _reward);
        
        _navMeshAgent.isStopped = true;
        _animator.SetTrigger(_idDie);
        yield return Timing.WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        PoolReference.Pool.Release(this);
        _isWalking = false;
    }

    public IEnumerator<float> Attacking()
    {
        _isWalking = false;
        _isAttacking = true;
        _navMeshAgent.isStopped = true;
        _animator.SetTrigger(_idAttack);
        float length = _animator.GetCurrentAnimatorStateInfo(0).length / 2f;
        // in the middle of animation, we'll check if near the player to be impacted
        yield return Timing.WaitForSeconds(length);
        CheckAttack();
        yield return Timing.WaitForSeconds(length);
        _isAttacking = false;
    }

    public IEnumerator<float> Walking()
    {
        _isWalking = true;
        _isAttacking = false;
        _animator.SetTrigger(_idWalk);
        yield return Timing.WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(_targetPos);
        yield return 0f;
    }

    private void UpdateTargetLocation(object sender, UxrAvatarMoveEventArgs e)
    {
        _targetPos = e.NewPosition;
        _navMeshAgent.SetDestination(_targetPos);
        if (_isDie) return;
        if(!ReachedDestinationOrGaveUp() && !_isWalking)
        {
            Timing.RunCoroutine(Walking());
        }
    }

    #region UNITY
    private void OnEnable()
    {
        UxrManager.AvatarMoved += UpdateTargetLocation;
    }

    private void OnDisable()
    {
        UxrManager.AvatarMoved -= UpdateTargetLocation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_myTransform.position, _myTransform.position + _myTransform.forward * DistanceAttacking);
    }
    #endregion

#if UNITY_EDITOR
    [Button("To Die")]
    public void ToDie() => Timing.RunCoroutine(Dying());

    [Button("Hit")]
    public void ToHit() => Timing.RunCoroutine(Hit());
#endif
}
