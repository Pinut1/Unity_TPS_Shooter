using UnityEngine;

/// <summary>
/// Move 상태 - 걷기 이동
/// </summary>
public class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("[State] Move 진입");
    }

    public override void Update()
    {
        // 애니메이션 업데이트
        player.Animator?.SetFloat("Speed", 0.5f);
        player.Animator?.SetFloat("InputX", player.Input.MoveInput.x, 0.1f, Time.deltaTime);
        player.Animator?.SetFloat("InputY", player.Input.MoveInput.y, 0.1f, Time.deltaTime);

        // 1. 이동 정지 → Idle
        if (player.Input.MoveInput.magnitude < 0.1f)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        // 2. 달리기 → Sprint
        if (player.Input.SprintInputHeld)
        {
            stateMachine.ChangeState(player.SprintState);
            return;
        }

        // 3. 조준 → Aim
        if (player.Input.AimInputHeld)
        {
            stateMachine.ChangeState(player.AimState);
            return;
        }

        // 4. 엄폐 → Cover
        if (player.Input.CoverInputDown && player.CanEnterCover())
        {
            stateMachine.ChangeState(player.CoverState);
            return;
        }

        // 이동 처리
        player.Movement.Move(player.Input.MoveInput, player.Movement.WalkSpeed, player.CameraTransform);
        player.Movement.RotateTowardsMoveDirection(player.Input.MoveInput, player.CameraTransform);
    }

    public override void Exit()
    {
    }
}
