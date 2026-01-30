using UnityEngine;

/// <summary>
/// 적 AI 상태 기본 클래스
/// </summary>
public abstract class EnemyStateBase
{
    protected EnemyAI enemy;
    protected EnemyStateMachine stateMachine;

    public EnemyStateBase(EnemyAI enemy, EnemyStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

/// <summary>
/// 적 상태 머신
/// </summary>
public class EnemyStateMachine
{
    public EnemyStateBase CurrentState { get; private set; }

    public void Initialize(EnemyStateBase startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(EnemyStateBase newState)
    {
        if (newState == null || newState == CurrentState) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
