/// <summary>
/// 플레이어 상태 기본 클래스 (추상)
/// 모든 플레이어 상태가 이 클래스를 상속받습니다.
/// </summary>
public abstract class PlayerBaseState
{
    protected PlayerController player;
    protected PlayerStateMachine stateMachine;

    public PlayerBaseState(PlayerController player, PlayerStateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    /// <summary>상태 진입 시 호출</summary>
    public virtual void Enter() { }

    /// <summary>매 프레임 호출</summary>
    public virtual void Update() { }

    /// <summary>물리 업데이트 시 호출</summary>
    public virtual void FixedUpdate() { }

    /// <summary>상태 종료 시 호출</summary>
    public virtual void Exit() { }
}
