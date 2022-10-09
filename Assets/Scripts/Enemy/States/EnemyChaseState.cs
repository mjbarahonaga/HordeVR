using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyStateMachine currentContext, EnemyStateFactory enemyStateFactory)
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
                        return;
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
                        return;
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
        Ctx.TriggetAnimation(EnemyStates.Chase);
    }

    public override void ExitState()
    {
        
    }


    public override void UpdateState(float deltaTime)
    {
        CheckSwitchState();
    }
}
