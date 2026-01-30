using UnityEngine;

/// <summary>
/// 적 대기 상태
/// </summary>
public class EnemyIdleState : EnemyStateBase
{
    private float idleTimer;
    private float idleDuration = 2f;

    public EnemyIdleState(EnemyAI enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.StopMoving();
        idleTimer = 0f;
        Debug.Log("[EnemyAI] Idle 상태 진입");
    }

    public override void Update()
    {
        idleTimer += Time.deltaTime;

        // 플레이어 감지 시 추격
        if (enemy.EnemyBase.PlayerDetected)
        {
            if (enemy.EnemyBase.IsPlayerInAttackRange())
            {
                stateMachine.ChangeState(enemy.AttackState);
            }
            else
            {
                stateMachine.ChangeState(enemy.ChaseState);
            }
            return;
        }

        // 일정 시간 후 순찰로 전환
        if (idleTimer >= idleDuration && enemy.PatrolPoints != null && enemy.PatrolPoints.Length > 0)
        {
            stateMachine.ChangeState(enemy.PatrolState);
        }
    }

    public override void Exit()
    {
    }
}
