using UnityEngine;

/// <summary>
/// Idle 상태 - 대기 상태
/// </summary>
public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        // 애니메이션: Idle
        player.Animator?.SetFloat("Speed", 0f);
        player.Animator?.SetBool("IsAiming", false);

        Debug.Log("[State] Idle 진입");
    }

    public override void Update()
    {
        // Input이 없으면 리턴
        if (player.Input == null) return;

        // 1. 엄폐 체크
        if (player.Input.CoverInputDown && player.CanEnterCover())
        {
            stateMachine.ChangeState(player.CoverState);
            return;
        }

        // 2. 조준 체크
        if (player.Input.AimInputHeld)
        {
            stateMachine.ChangeState(player.AimState);
            return;
        }

        // 3. 이동 체크
        if (player.Input.MoveInput.magnitude > 0.1f)
        {
            // 달리기 체크
            if (player.Input.SprintInputHeld)
            {
                stateMachine.ChangeState(player.SprintState);
            }
            else
            {
                stateMachine.ChangeState(player.MoveState);
            }
            return;
        }
    }

    public override void FixedUpdate()
    {
        // 대기 중에도 중력 적용 (PlayerMovement에서 처리)
    }

    public override void Exit()
    {
    }
}
