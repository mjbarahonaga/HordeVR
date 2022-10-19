using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDieState : EnemyBaseState
{
    private float _timeToExit;
    private float _currentTime;

    public EnemyDieState(EnemyStateMachine currentContext, EnemyStateFactory enemyStateFactory)
    : base(currentContext, enemyStateFactory) { }

    public override void CheckSwitchState()
    {
        GameManager.Instance.EnemyDie(Enemy.Ghoul, Ctx.GetEnemyBehaviour.Data.Reward);
        Ctx.GetEnemyBehaviour.PoolReference.OnReturnToPool(Ctx.GetEnemyBehaviour);
    }

    public override void EnterState()
    {
        Ctx.GetAgent.isStopped = true;
        Ctx.TriggetAnimation(EnemyStates.Die);
        _timeToExit = Ctx.GetAnimator.GetCurrentAnimatorStateInfo(0).length + 1;
        _currentTime = 0f;
    }

    public override void ExitState()
    {
        
    }

    public override void UpdateState(float deltaTime)
    {
        _currentTime += deltaTime;

        if (_currentTime < _timeToExit) return;

        CheckSwitchState();
    }
}
