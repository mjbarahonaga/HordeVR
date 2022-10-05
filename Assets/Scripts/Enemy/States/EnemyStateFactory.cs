
using System.Collections.Generic;

public enum EnemyStates
{
    Run,
    Chase,
    Attack,
    Hit,
    Die
}

public class EnemyStateFactory 
{
    private EnemyStateMachine _context;
    private Dictionary<EnemyStates, EnemyBaseState> _states = new Dictionary<EnemyStates, EnemyBaseState>();

    public EnemyStateFactory(EnemyStateMachine context)
    {
        _context = context;
        _states[EnemyStates.Run] = new EnemyRunState(_context, this);
        _states[EnemyStates.Chase] = new EnemyChaseState(_context, this);
        _states[EnemyStates.Attack] = new EnemyAttackState(_context, this);
        _states[EnemyStates.Hit] = new EnemyHitState(_context, this);
        _states[EnemyStates.Die] = new EnemyDieState(_context, this);
    }

    public EnemyBaseState Run() => _states[EnemyStates.Run];
    public EnemyBaseState Chase() => _states[EnemyStates.Chase];
    public EnemyBaseState Attack() => _states[EnemyStates.Attack];
    public EnemyBaseState Hit() => _states[EnemyStates.Hit];
    public EnemyBaseState Die() => _states[EnemyStates.Die];
}
