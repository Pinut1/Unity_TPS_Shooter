using UnityEngine;

/// <summary>
/// 플레이어 상태 머신
/// 현재 상태 관리 및 전환 처리
/// </summary>
public class PlayerStateMachine
{
    public PlayerBaseState CurrentState { get; private set; }
    public PlayerBaseState PreviousState { get; private set; }

    /// <summary>
    /// 초기 상태 설정
    /// </summary>
    public void Initialize(PlayerBaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    /// <summary>
    /// 상태 전환
    /// </summary>
    public void ChangeState(PlayerBaseState newState)
    {
        if (newState == null || newState == CurrentState) return;

        PreviousState = CurrentState;
        CurrentState.Exit();

        CurrentState = newState;
        CurrentState.Enter();

        Debug.Log($"[FSM] {PreviousState?.GetType().Name} → {CurrentState.GetType().Name}");
    }

    /// <summary>
    /// 이전 상태로 복귀
    /// </summary>
    public void RevertToPreviousState()
    {
        if (PreviousState != null)
        {
            ChangeState(PreviousState);
        }
    }
}
