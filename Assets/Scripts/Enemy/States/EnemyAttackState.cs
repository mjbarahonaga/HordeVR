using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    private float _timeToExit;
    private float _currentTime;

    public EnemyAttackState(EnemyStateMachine currentContext, EnemyStateFactory enemyStateFactory)
    : base(currentContext, enemyStateFactory) { }

    public override void CheckSwitchState()
    {
        var type = Ctx.GetEnemyBehaviour.GetTypeTarget;
        var target = Ctx.GetEnemyBehaviour.GetTarget;

        switch (type)
        {
            case Targets.Obstacle:
                if (!target.activeInHierarchy)
                {
                    SwitchState(Factory.Run());
                }
                // Current alive
                else
                {
                    if (Ctx.GetEnemyBehaviour.CurrentTargetInRangeOfAttack())
                    {
                        Ctx.TriggetAnimation(EnemyStates.Attack); 
                        return;
                    }
                    else if (Ctx.GetEnemyBehaviour.GetTarget != null)
                    {
                        SwitchState(Factory.Chase());
                    }
                }
                break;
            case Targets.Player:
                if (GameManager.Instance.Player.IsDie)
                {
                    SwitchState(Factory.Run());
                }
                else
                {
                    if (Ctx.GetEnemyBehaviour.CurrentTargetInRangeOfAttack())
                    {
                        Ctx.TriggetAnimation(EnemyStates.Attack); 
                        return;
                    }
                    else if (Ctx.GetEnemyBehaviour.PlayerInRange())
                    {
                        SwitchState(Factory.Chase());
                    }
                    else
                    {
                        SwitchState(Factory.Run());
                    }
                }
                break;
            case Targets.None:
                SwitchState(Factory.Run());
                break;
            default:
                break;
        }
    }

    public override void EnterState()
    {
        Ctx.GetAgent.isStopped = true;
        Ctx.TriggetAnimation(EnemyStates.Attack);
        _timeToExit = Ctx.GetAnimator.GetCurrentAnimatorStateInfo(0).length;
        _currentTime = 0f;
    }

    public override void ExitState()
    {
        Ctx.GetAgent.isStopped = false;
    }

    public override void UpdateState(float deltaTime)
    {
        _currentTime += deltaTime;

        if (_currentTime < _timeToExit) return;

        _currentTime = 0f;

        CheckSwitchState();
    }
}
