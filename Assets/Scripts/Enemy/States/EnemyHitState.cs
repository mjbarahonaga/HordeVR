using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitState : EnemyBaseState
{
    private float _timeToExit;
    private float _currentTime;
    public EnemyHitState(EnemyStateMachine currentContext, EnemyStateFactory enemyStateFactory)
    : base(currentContext, enemyStateFactory) { }

    public override void CheckSwitchState()
    {

    }

    public override void EnterState()
    {
        Ctx.TriggetAnimation(EnemyStates.Hit);
        _timeToExit = Ctx.GetAnimator.GetCurrentAnimatorClipInfo(0).Length;
        _currentTime = 0f;
        Ctx.GetAgent.isStopped = true;
    }

    public override void ExitState()
    {
        Ctx.GetAgent.isStopped = false;
    }

    public override void UpdateState(float deltaTime)
    {
        _currentTime += deltaTime;
        
        if (_currentTime < _timeToExit) return;

        ExitState();
    }
}
