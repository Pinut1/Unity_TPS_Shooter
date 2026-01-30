using UnityEngine;

/// <summary>
/// 적 순찰 상태
/// </summary>
public class EnemyPatrolState : EnemyStateBase
{
    private int currentPatrolIndex;
    private float waitTimer;
    private bool isWaiting;

    public EnemyPatrolState(EnemyAI enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        isWaiting = false;
        MoveToNextPatrolPoint();
        Debug.Log("[EnemyAI] Patrol 상태 진입");
    }

    public override void Update()
    {
        // 플레이어 감지 시 추격
        if (enemy.EnemyBase.PlayerDetected)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // 대기 중
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                MoveToNextPatrolPoint();
            }
            return;
        }

        // 목적지 도달 시 대기
        if (enemy.HasReachedDestination())
        {
            isWaiting = true;
            waitTimer = enemy.PatrolWaitTime;
            enemy.StopMoving();
        }
    }

    private void MoveToNextPatrolPoint()
    {
        if (enemy.PatrolPoints == null || enemy.PatrolPoints.Length == 0) return;

        currentPatrolIndex = (currentPatrolIndex + 1) % enemy.PatrolPoints.Length;
        Transform targetPoint = enemy.PatrolPoints[currentPatrolIndex];

        if (targetPoint != null)
        {
            enemy.MoveTo(targetPoint.position);
        }
    }

    public override void Exit()
    {
    }
}
