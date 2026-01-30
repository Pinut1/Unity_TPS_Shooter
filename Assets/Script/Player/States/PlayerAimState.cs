using UnityEngine;

/// <summary>
/// Aim 상태 - 조준 상태
/// </summary>
public class PlayerAimState : PlayerBaseState
{
    public PlayerAimState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator?.SetBool("IsAiming", true);
        player.CameraController?.SetAiming(true);

        Debug.Log("[State] Aim 진입");
    }

    public override void Update()
    {
        // 애니메이션 (느린 이동)
        player.Animator?.SetFloat("Speed", player.Input.MoveInput.magnitude * 0.3f);
        player.Animator?.SetFloat("InputX", player.Input.MoveInput.x, 0.1f, Time.deltaTime);
        player.Animator?.SetFloat("InputY", player.Input.MoveInput.y, 0.1f, Time.deltaTime);

        // 1. 조준 해제 → Idle 또는 Move
        if (!player.Input.AimInputHeld)
        {
            if (player.Input.MoveInput.magnitude > 0.1f)
            {
                stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
            return;
        }

        // 2. 발사 → Shoot
        if (player.Input.FireInputDown || player.Input.FireInputHeld)
        {
            if (player.WeaponController != null && player.WeaponController.CanFire())
            {
                stateMachine.ChangeState(player.ShootState);
                return;
            }
        }

        // 3. 재장전 → Reload
        if (player.Input.ReloadInputDown)
        {
            if (player.WeaponController != null && player.WeaponController.CanReload())
            {
                stateMachine.ChangeState(player.ReloadState);
                return;
            }
        }

        // 조준 중 이동 (느린 속도)
        if (player.Input.MoveInput.magnitude > 0.1f)
        {
            player.Movement.Move(player.Input.MoveInput, player.Movement.AimSpeed, player.CameraTransform);
        }

        // 카메라 방향으로 회전
        player.Movement.RotateTowardsCamera(player.CameraTransform);
    }

    public override void Exit()
    {
        player.Animator?.SetBool("IsAiming", false);
        player.CameraController?.SetAiming(false);
    }
}
