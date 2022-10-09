using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using UnityEngine.AI;

public class EnemyStateMachine : MonoBehaviour
{
    // Enemy Variables
    private EnemyBehaviour _enemyBehaviour;
    private NavMeshAgent _agent;
    private Animator _animator;

    #region Animation Id
    private int _idRun;
    private int _idDie;
    private int _idAttack;
    private int _idHit;
    private int _idChase;
    #endregion

    // state variables
    private EnemyBaseState _currentState;
    private EnemyStateFactory _states;

    #region Getters and Setters
    public EnemyBaseState CurrentState { 
        get => _currentState; 
        set => _currentState = value;
    }
    public EnemyStateFactory StateFactory { get => _states; }
    public EnemyBehaviour GetEnemyBehaviour { get => _enemyBehaviour; }
    public NavMeshAgent GetAgent { get => _agent; }
    public Animator GetAnimator { get => _animator; }
    #endregion
    private CoroutineHandle _updateCoroutine;

    public void MyUpdate()
    {
        _currentState.UpdateState(Time.deltaTime);
    }

    public void TriggetAnimation(EnemyStates state)
    {
        switch (state)
        {
            case EnemyStates.Run:
                _animator.SetBool(_idRun,true);
                break;
            case EnemyStates.Chase:
                _animator.SetTrigger(_idChase);
                break;
            case EnemyStates.Attack:
                _animator.SetBool(_idAttack, true);
                break;
            case EnemyStates.Hit:
                _animator.SetTrigger(_idHit);
                break;
            case EnemyStates.Die:
                _animator.SetTrigger(_idDie);
                break;
            default:
                break;
        }
    }

    #region UNITY METHODS
    private void Awake()
    {
        _idRun = Animator.StringToHash("IsRunning");
        _idDie = Animator.StringToHash("Die");
        _idAttack = Animator.StringToHash("Attack");
        _idHit = Animator.StringToHash("Hit");
        _idChase = Animator.StringToHash("Chase");

        if (_enemyBehaviour == null) _enemyBehaviour = GetComponent<EnemyBehaviour>();
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_states == null) _states = new EnemyStateFactory(this);
        //_currentState = _states.Run();
        //_currentState.EnterState();
    }

    private void OnEnable()
    {
        _updateCoroutine = Timing.RunCoroutine(Utils.EmulateUpdate(MyUpdate,this),Segment.FixedUpdate);
    }

    private void OnDisable()
    {
        Timing.KillCoroutines(_updateCoroutine);
    }


    #endregion
}
