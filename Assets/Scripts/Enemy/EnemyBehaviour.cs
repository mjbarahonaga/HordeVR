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
using Unity.Collections;
using Unity.Jobs;


public enum Targets
{
    None,
    Obstacle,
    Player
}

public class EnemyBehaviour : MonoBehaviour
{
    public static Action<GameObject, int> OnSendAttack;
    
    public DataEnemy Data;
    public PoolEnemy PoolReference;

    // Value is the cosine between two vectors 
    public float AngleOfVision = 0.5f;

    public float DistanceChasing = 2f;

    public float DistanceAttacking = 1f;

    [Sirenix.OdinInspector.ReadOnly] 
    public bool IsDie = false;
    [SerializeField, Sirenix.OdinInspector.ReadOnly] 
    private Transform _myTransform;
    [SerializeField, Sirenix.OdinInspector.ReadOnly] 
    public GameObject InstantiatePrefab;

    [SerializeField, Sirenix.OdinInspector.ReadOnly] 
    private NavMeshAgent _navMeshAgent;
    private GameObject _currentTarget;
    private Targets _currentTypeTarget;

    public GameObject GetTarget { get { return _currentTarget; } }
    public Targets GetTypeTarget { get { return _currentTypeTarget; } }
    //[SerializeField, ReadOnly] private Transform _myParentTransform;

    [SerializeField, Sirenix.OdinInspector.ReadOnly]
    private EnemyStateMachine _enemyStateMachine;
    [SerializeField, Sirenix.OdinInspector.ReadOnly] 
    private Vector3 _targetPos;

    [SerializeField, Sirenix.OdinInspector.ReadOnly] 
    private int _currentHP;
    [SerializeField, Sirenix.OdinInspector.ReadOnly] 
    private int _reward;

    #region Sound
    [FoldoutGroup("Sound")] public AudioSource MyAudioSource;
    [FoldoutGroup("Sound")] public List<AudioClip> DyingSound;
    [FoldoutGroup("Sound")] public List<AudioClip> HittingSound;
    [FoldoutGroup("Sound")] public List<AudioClip> RunningSound;
    [FoldoutGroup("Sound")] public List<AudioClip> AttackingSound;
    #endregion

    #region Raycast Variables
    private NativeArray<RaycastCommand> _raycastCommands;
    private NativeArray<RaycastHit> _raycastHits;
    private JobHandle _jobHandle;
    #endregion

    private CoroutineHandle _updateCoroutine;


    #region POOL METHODS
    public void Init(in Vector3 target, in Transform startPos)
    {
        IsDie = false;
        _targetPos = target;
        _myTransform.position = startPos.position;

        if (NavMesh.SamplePosition(_myTransform.position, out NavMeshHit closesthit, 500f, NavMesh.AllAreas))
            _myTransform.position = closesthit.position;
        _navMeshAgent.Warp(_myTransform.position);
        _myTransform.LookAt(target);

        _navMeshAgent.isStopped = false;
        _enemyStateMachine.CurrentState = _enemyStateMachine.StateFactory.Run();
        _enemyStateMachine.CurrentState.EnterState();

    }

    public void LookAtTarget() => _myTransform.LookAt(_targetPos);

    public void InstanceEnemy(GameObject prefab)
    {
        InstantiatePrefab = prefab;

        _navMeshAgent = InstantiatePrefab.GetComponent<NavMeshAgent>();
        _enemyStateMachine = InstantiatePrefab.GetComponent<EnemyStateMachine>();

        MyAudioSource = InstantiatePrefab.GetComponent<AudioSource>();
        _myTransform.localPosition = Vector3.zero;
        SetUpEnemy();

        InstantiatePrefab.SetActive(false);
    }

    public void SetUpEnemy()
    {
        _navMeshAgent.speed = Data.Speed;
        //_navMeshAgent.stoppingDistance = DistanceAttacking - 1f < 0 ? 0 : DistanceAttacking - 1f;
        _reward = Data.Reward;
        _currentHP = Data.HP + Data.IncreasePerLevel;
    }

    public void TakeFromPool()
    {

        InstantiatePrefab.SetActive(true);
        
        SetUpEnemy();
    }

    public void ReturnToPool()
    {
        InstantiatePrefab.SetActive(false);
        if(_enemyStateMachine?.CurrentState != null)
        {
            _enemyStateMachine.CurrentState.ExitState();
            _enemyStateMachine.CurrentState = null;
        }
        
    }

    #endregion

    #region ENEMY METHODS
    public Collider CheckForwardDirection()
    {

        //_raycastHits = new NativeArray<RaycastHit>(1, Allocator.Temp);
        //_raycastCommands = new NativeArray<RaycastCommand>(1, Allocator.Temp);

        //_raycastCommands[0] = new RaycastCommand(
        //    _myTransform.position,
        //    _myTransform.forward * DistanceChasing);

        //_jobHandle = RaycastCommand.ScheduleBatch(_raycastCommands, _raycastHits, 1, default(JobHandle));
        //_jobHandle.Complete();

        //var result = _raycastHits[0];

        RaycastHit result;

        Physics.Raycast(_myTransform.position, _myTransform.forward, out result, DistanceChasing);

        return result.collider;
    }

    public bool TakeDamage(int damage)
    {
        if (IsDie) return false;

        _currentHP -= damage;

        if (_currentHP <= 0)
        {
            IsDie = true;
            _enemyStateMachine.CurrentState.SwitchState(_enemyStateMachine.StateFactory.Die());
            return true;
        }
        _enemyStateMachine.CurrentState.SwitchState(_enemyStateMachine.StateFactory.Hit());
        return true;
    }

    public void SetTarget(GameObject target, Targets type)
    {
        _currentTarget = target;
        _currentTypeTarget = type;
        _targetPos = target.transform.position;
    }

    public void ResetTarget()
    {
        _currentTarget = null;
        _targetPos = Vector3.zero;
        _currentTypeTarget = Targets.None;
    }

    public bool PlayerInRange()
    {
        return (GameManager.Instance.PlayerPosition - _myTransform.position).sqrMagnitude < DistanceChasing;
    }

    public bool CurrentTargetInRangeOfAttack()
    {
        if (_currentTypeTarget == Targets.Player && GameManager.Instance.Player.IsDie) return false; 
        if (_currentTarget == null || !_currentTarget.activeInHierarchy) return false;
        return (_currentTarget.transform.position - _myTransform.position).magnitude < DistanceAttacking;
    }
    //public bool CheckForwardDirection(out string tag, out GameObject target)
    //{
    //    tag = "";
    //    target = null;

    //    _raycastHits = new NativeArray<RaycastHit>(1, Allocator.Temp);
    //    _raycastCommands = new NativeArray<RaycastCommand>(1, Allocator.Temp);

    //    _raycastCommands[0] = new RaycastCommand(
    //        _myTransform.position,
    //        _myTransform.forward * DistanceChasing);

    //    _jobHandle = RaycastCommand.ScheduleBatch(_raycastCommands, _raycastHits, 1, default(JobHandle));
    //    _jobHandle.Complete();

    //    var result = _raycastHits[0];

    //    if (result.collider == null) return false;

    //    target = result.collider.gameObject;
    //    var stringCollider = result.collider.tag;
    //    if (stringCollider.Equals("Obstacle"))
    //    {
    //        tag = "Obstacle";
    //        return true;
    //    }

    //    if (stringCollider.Equals("Player"))
    //    {
    //        tag = "Player";
    //        return true;
    //    }

    //    return false;
    //}

    public void Attack()
    {
        CheckAttack();
    }

    public void CheckAttack()
    {
        if ((_targetPos - _myTransform.position).sqrMagnitude < DistanceAttacking)
        {
            OnSendAttack?.Invoke(_currentTarget, Data.Damage);
        }
    }

    //public bool StopChasing(out StateEnemy state)
    //{
    //    state = StateEnemy.Chasing;

    //    if ((_targetPos - _myTransform.position).sqrMagnitude > DistanceChasing)
    //    {
    //        state = StateEnemy.Walk;
    //        return true;
    //    }
    //    if ((_targetPos - _myTransform.position).sqrMagnitude < DistanceAttacking)
    //    {
    //        state = StateEnemy.Attack;
    //        return true;
    //    }
    //    return false;
    //} 

    private void UpdateTargetLocation(object sender, UxrAvatarMoveEventArgs e)
    {
        if (IsDie) return;
        if (_currentTypeTarget == Targets.Player)
            _targetPos = e.NewPosition;
        //if ((_myTransform.position - GameManager.Instance.PlayerPosition).sqrMagnitude < DistanceChasing)
        //{
        //    var heading = GameManager.Instance.PlayerPosition - _myTransform.position;
        //    var dot = Vector3.Dot(heading, _myTransform.forward);
        //    if (dot < AngleOfVision)
        //    {
        //        _targetPos = e.NewPosition;
        //        _currentTarget = GameManager.Instance.Player.gameObject;
        //        _navMeshAgent.SetDestination(_targetPos);
        //        return;
        //    }
        //}

            //if (!ReachedDestinationOrGaveUp())
            //{
            //    _animator.SetTrigger(_idWalk);
            //}
    }

    public bool ReachedDestinationOrGaveUp()
    {

        if (!_navMeshAgent.pathPending)
        {
            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                //if (_navMeshAgent.velocity.sqrMagnitude == 0f)
                if ((_targetPos - _myTransform.position).sqrMagnitude <= (DistanceAttacking + 2f))
                {
                    //if ((_targetPos - _myTransform.position).sqrMagnitude < DistanceAttacking)
                    return true;
                }
            }
        }

        return false;
    }

    //public IEnumerator<float> Attacking()
    //{
    //    MyAudioSource.loop = false;
    //    MyAudioSource.clip = AttackingSound.GetRandom();
    //    MyAudioSource.Play();

    //    _isWalking = false;
    //    _isAttacking = true;
    //    _navMeshAgent.isStopped = true;
    //    _animator.SetTrigger(_idAttack);
    //    float length = _animator.GetCurrentAnimatorStateInfo(0).length / 2f;
    //    // in the middle of animation, we'll check if near the player to be impacted
    //    yield return Timing.WaitForSeconds(length);
    //    CheckAttack();
    //    yield return Timing.WaitForSeconds(length);
    //    _isAttacking = false;
    //}

    #endregion
    public void MyUpdate()
    {
        //if (_navMeshAgent.isStopped && ReachedDestinationOrGaveUp())
        //{
        //    _animator.SetTrigger(_idAttack);
        //}
    }



    //public void Attack()
    //{
    //    Timing.RunCoroutine(Attacking());
    //}



    //public IEnumerator<float> Spawn()
    //{
    //    IsDie = false;
    //    _animator.SetTrigger(_idSpawn);
    //    float length = _animator.GetCurrentAnimatorStateInfo(0).length;
    //    if (length == float.PositiveInfinity) length = 1f;
    //    yield return Timing.WaitForSeconds(length);
    //    _updateCoroutine = Timing.RunCoroutine(Utils.EmulateUpdate(MyUpdate, this), Segment.LateUpdate);
    //    _isWalking = false;
    //}

    //public IEnumerator<float> Hit()
    //{
    //    MyAudioSource.loop = false;
    //    MyAudioSource.clip = HittingSound.GetRandom();
    //    MyAudioSource.Play();

    //    _isWalking = false;
    //    _isAttacking = false;
    //    _navMeshAgent.isStopped = true;
    //    _animator.SetTrigger(_idHit);
    //    yield return Timing.WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
    //}

    //public IEnumerator<float> Dying()
    //{
    //    Timing.KillCoroutines(_updateCoroutine);
    //    GameManager.Instance.EnemyDie(Data.EnemyType, _reward);

    //    _navMeshAgent.isStopped = true;
    //    _animator.SetTrigger(_idDie);
    //    yield return Timing.WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
    //    PoolReference.Pool.Release(this);
    //}



    //public IEnumerator<float> Walking()
    //{

    //    _isWalking = true;
    //    _isAttacking = false;

    //    yield return Timing.WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
    //    _navMeshAgent.isStopped = false;
    //    _navMeshAgent.SetDestination(_targetPos);
    //    yield return 0f;
    //}



    #region UNITY
    private void OnEnable()
    {
        UxrManager.AvatarMoved += UpdateTargetLocation;
    }

    private void OnDisable()
    {
        UxrManager.AvatarMoved -= UpdateTargetLocation;
    }

    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;
            if(_myTransform == null) _myTransform = GetComponent<Transform>();

        });
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_myTransform.position, _myTransform.position + _myTransform.forward * DistanceAttacking);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_myTransform.position, DistanceChasing);
    }
    #endregion

#if UNITY_EDITOR
    //[Button("To Die")]
    //public void ToDie() => Timing.RunCoroutine(Dying());
    public int testAmount = 10;
    [Button("Hit")]
    public void ToHit() => TakeDamage(testAmount);
#endif
}
