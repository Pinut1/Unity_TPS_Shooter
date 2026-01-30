using UnityEngine;

/// <summary>
/// Shoot 상태 - 사격 상태
/// </summary>
public class PlayerShootState : PlayerBaseState
{
    private float shootTimer;
    private float fireRate;

    public PlayerShootState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator?.SetTrigger("Shoot");
        player.Animator?.SetBool("IsAiming", true);

        // 카메라 조준 상태 유지!
        player.CameraController?.SetAiming(true);

        // 첫 발사
        player.WeaponController?.Fire();
        shootTimer = 0f;
        fireRate = player.WeaponController?.GetFireRate() ?? 0.1f;

        Debug.Log("[State] Shoot 진입");
    }

    public override void Update()
    {
        // 카메라 조준 상태 계속 유지
        player.CameraController?.SetAiming(true);

        shootTimer += Time.deltaTime;

        // 1. 발사 버튼 유지 + 탄약 있음 → 연사
        if (player.Input.FireInputHeld && player.WeaponController.HasAmmo())
        {
            if (shootTimer >= fireRate)
            {
                player.WeaponController.Fire();
                shootTimer = 0f;
            }
        }
        // 2. 발사 버튼 해제 또는 탄약 없음
        else
        {
            // 탄약 없으면 재장전
            if (!player.WeaponController.HasAmmo())
            {
                stateMachine.ChangeState(player.ReloadState);
                return;
            }

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

        // 사격 중에도 느린 이동 가능
        if (player.Input.MoveInput.magnitude > 0.1f)
        {
            player.Movement.Move(player.Input.MoveInput, player.Movement.AimSpeed * 0.5f, player.CameraTransform);
        }

        // 카메라 방향으로 회전 유지
        player.Movement.RotateTowardsCamera(player.CameraTransform);
    }

    public override void Exit()
    {
    }
}
