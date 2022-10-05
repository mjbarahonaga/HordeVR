public abstract class EnemyBaseState
{

    private EnemyStateMachine _ctx;
    private EnemyStateFactory _factory;
    private EnemyBaseState _currentSubState;
    private EnemyBaseState _currentSuperState;


    protected EnemyStateMachine Ctx { get => _ctx; }
    protected EnemyStateFactory Factory { get => _factory; }

    public EnemyBaseState(EnemyStateMachine ctx, EnemyStateFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public abstract void EnterState();
    public abstract void UpdateState(float deltaTime);
    public abstract void ExitState();
    public abstract void CheckSwitchState();

    public void UpdateStates(float deltaTime) 
    {
        UpdateState(deltaTime);
    }

    public void SwitchState(EnemyBaseState newState) 
    { 
        // current state exits state
        ExitState();

        // new state enters state
        newState.EnterState();

        // switch current state of context
        _ctx.CurrentState = newState;
    }
}
