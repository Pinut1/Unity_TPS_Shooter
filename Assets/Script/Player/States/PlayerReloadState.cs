using UnityEngine;

/// <summary>
/// Reload 상태 - 재장전 상태
/// </summary>
public class PlayerReloadState : PlayerBaseState
{
    private float reloadTimer;
    private float reloadDuration;

    public PlayerReloadState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator?.SetTrigger("Reload");

        reloadTimer = 0f;
        reloadDuration = player.WeaponController?.GetReloadTime() ?? 2f;

        Debug.Log($"[State] Reload 진입 ({reloadDuration}초)");
    }

    public override void Update()
    {
        reloadTimer += Time.deltaTime;

        // 재장전 완료
        if (reloadTimer >= reloadDuration)
        {
            player.WeaponController?.Reload();

            // 조준 유지 중이면 Aim으로
            if (player.Input.AimInputHeld)
            {
                stateMachine.ChangeState(player.AimState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
            return;
        }

        // 재장전 중 느린 이동 가능
        if (player.Input.MoveInput.magnitude > 0.1f)
        {
            player.Movement.Move(player.Input.MoveInput, player.Movement.WalkSpeed * 0.5f, player.CameraTransform);
            player.Movement.RotateTowardsMoveDirection(player.Input.MoveInput, player.CameraTransform);
        }
    }

    public override void Exit()
    {
        Debug.Log("[State] Reload 완료");
    }
}
