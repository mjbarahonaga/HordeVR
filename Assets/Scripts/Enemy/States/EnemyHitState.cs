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
                        SwitchState(Factory.Attack());

                    }
                    else
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
                        SwitchState(Factory.Attack());
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
                if (Ctx.GetEnemyBehaviour.PlayerInRange())
                {
                    Ctx.GetEnemyBehaviour.SetTarget(GameManager.Instance.Player.gameObject, Targets.Player);
                    SwitchState(Factory.Chase());
                    return;
                }
                var agent = Ctx.GetAgent;
                if (!agent.pathPending)
                {
                    if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial && !agent.hasPath)
                    {
                        var collider = Ctx.GetEnemyBehaviour.CheckForwardDirection();
                        if (collider != null && collider.gameObject.CompareTag("Obstacle"))
                        {
                            Ctx.GetEnemyBehaviour.SetTarget(collider.gameObject, Targets.Obstacle);
                            SwitchState(Factory.Attack());
                            return;
                        }
                    }
                }
                SwitchState(Factory.Run());

                break;
            default:
                break;
        }
    }

    public override void EnterState()
    {
        Ctx.TriggetAnimation(EnemyStates.Hit);
        _timeToExit = Ctx.GetAnimator.GetCurrentAnimatorStateInfo(0).length;
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

        CheckSwitchState();
    }
}
