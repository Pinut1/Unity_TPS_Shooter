using UnityEngine;

/// <summary>
/// Cover 상태 - 엄폐 상태
/// </summary>
public class PlayerCoverState : PlayerBaseState
{
    public PlayerCoverState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator?.SetBool("InCover", true);

        // 엄폐물에 붙기
        var cover = player.CoverDetector?.GetNearestCover();
        if (cover != null)
        {
            player.Movement.TeleportTo(cover.GetSnapPosition(player.transform.position));
            player.Movement.RotateToCover(cover.CoverNormal);
        }

        Debug.Log("[State] Cover 진입");
    }

    public override void Update()
    {
        // 1. 엄폐 해제 (Space)
        if (player.Input.CoverInputDown)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        // 2. 엄폐 중 조준 → CoverAim (있다면)
        if (player.Input.AimInputHeld)
        {
            // CoverAimState가 있으면 전환
            // 지금은 일반 Aim으로 전환
            stateMachine.ChangeState(player.AimState);
            return;
        }

        // 3. 엄폐 중 좌우 이동 (엄폐물 따라 이동)
        var cover = player.CoverDetector?.GetNearestCover();
        if (cover != null && Mathf.Abs(player.Input.MoveInput.x) > 0.1f)
        {
            // 엄폐물 따라 좌우 이동
            Vector3 moveDir = Vector3.Cross(cover.CoverNormal, Vector3.up) * player.Input.MoveInput.x;
            player.Movement.Move(new Vector2(player.Input.MoveInput.x, 0), player.Movement.WalkSpeed * 0.5f, player.transform);
        }
    }

    public override void Exit()
    {
        player.Animator?.SetBool("InCover", false);
        player.CoverDetector?.LeaveCover();

        Debug.Log("[State] Cover 해제");
    }
}
