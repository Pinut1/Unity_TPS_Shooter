using UnityEngine;

/// <summary>
/// Sprint 상태 - 달리기
/// </summary>
public class PlayerSprintState : PlayerBaseState
{
    public PlayerSprintState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator?.SetFloat("Speed", 1f);
        Debug.Log("[State] Sprint 진입");
    }

    public override void Update()
    {
        // 애니메이션
        player.Animator?.SetFloat("InputX", player.Input.MoveInput.x, 0.1f, Time.deltaTime);
        player.Animator?.SetFloat("InputY", player.Input.MoveInput.y, 0.1f, Time.deltaTime);

        // 1. 이동 정지 → Idle
        if (player.Input.MoveInput.magnitude < 0.1f)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        // 2. Shift 해제 → Move
        if (!player.Input.SprintInputHeld)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
        }

        // 3. 조준 → Aim (달리기 중단)
        if (player.Input.AimInputHeld)
        {
            stateMachine.ChangeState(player.AimState);
            return;
        }

        // 이동 처리 (빠른 속도)
        player.Movement.Move(player.Input.MoveInput, player.Movement.SprintSpeed, player.CameraTransform);
        player.Movement.RotateTowardsMoveDirection(player.Input.MoveInput, player.CameraTransform);
    }

    public override void Exit()
    {
        player.Animator?.SetFloat("Speed", 0f);
    }
}
