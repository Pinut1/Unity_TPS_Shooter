using UnityEngine;

/// <summary>
/// 적 추격 상태
/// </summary>
public class EnemyChaseState : EnemyStateBase
{
    private float lostPlayerTimer;
    private float updatePathInterval = 0.2f;
    private float pathUpdateTimer;

    public EnemyChaseState(EnemyAI enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        lostPlayerTimer = 0f;
        pathUpdateTimer = 0f;
        Debug.Log("[EnemyAI] Chase 상태 진입");
    }

    public override void Update()
    {
        // 플레이어가 없으면 리턴
        if (enemy.EnemyBase.PlayerTransform == null)
        {
            stateMachine.ChangeState(enemy.IdleState);
            return;
        }

        // 플레이어 감지 여부
        if (enemy.EnemyBase.PlayerDetected)
        {
            lostPlayerTimer = 0f;

            // 공격 범위 내면 Attack으로 전환
            if (enemy.EnemyBase.IsPlayerInAttackRange())
            {
                stateMachine.ChangeState(enemy.AttackState);
                return;
            }

            // 경로 업데이트 (성능 최적화)
            pathUpdateTimer += Time.deltaTime;
            if (pathUpdateTimer >= updatePathInterval)
            {
                pathUpdateTimer = 0f;
                enemy.MoveTo(enemy.EnemyBase.PlayerTransform.position);
            }
        }
        else
        {
            // 플레이어를 잃은 시간 체크
            lostPlayerTimer += Time.deltaTime;

            // 마지막 위치로 이동
            if (lostPlayerTimer < enemy.ChaseTimeout)
            {
                enemy.MoveTo(enemy.EnemyBase.PlayerTransform.position);
            }
            else
            {
                // 타임아웃 → Idle로 전환
                stateMachine.ChangeState(enemy.IdleState);
                return;
            }
        }

        // 플레이어 방향으로 회전
        enemy.EnemyBase.LookAtPlayer();
    }

    public override void Exit()
    {
        enemy.StopMoving();
    }
}
