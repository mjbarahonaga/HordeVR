using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRunState : EnemyBaseState
{
    public EnemyRunState(EnemyStateMachine currentContext, EnemyStateFactory enemyStateFactory)
    : base(currentContext, enemyStateFactory) 
    { 

    }

    public override void UpdateState(float deltaTime)
    {
        CheckSwitchState();
    }
    public override void EnterState()
    {
        Ctx.TriggetAnimation(EnemyStates.Run);
        Ctx.GetEnemyBehaviour.ResetTarget();
        Ctx.GetAgent.SetDestination(GameManager.Instance.EnemyGoal.position);
    }

    public override void CheckSwitchState()
    {
        if(Ctx.GetEnemyBehaviour.PlayerInRange())
        {
            Ctx.GetEnemyBehaviour.SetTarget(GameManager.Instance.Player.gameObject, Targets.Player);
            SwitchState(Factory.Chase());
            return;
        }
        var agent = Ctx.GetAgent;
        if (!agent.pathPending)
        {
            if(agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial && !agent.hasPath)
            {
                var collider = Ctx.GetEnemyBehaviour.CheckForwardDirection();
                if(collider != null && collider.gameObject.CompareTag("Obstacle"))
                {
                    Ctx.GetEnemyBehaviour.SetTarget(collider.gameObject, Targets.Obstacle);
                    SwitchState(Factory.Attack());
                }
            }
        }
    }


    public override void ExitState()
    {
        
    }
    
}
