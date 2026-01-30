using UnityEngine;

/// <summary>
/// 적 공격 상태
/// </summary>
public class EnemyAttackState : EnemyStateBase
{
    public EnemyAttackState(EnemyAI enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.StopMoving();
        Debug.Log("[EnemyAI] Attack 상태 진입");
    }

    public override void Update()
    {
        // 플레이어 확인
        if (enemy.EnemyBase.PlayerTransform == null)
        {
            stateMachine.ChangeState(enemy.IdleState);
            return;
        }

        // 플레이어 방향으로 회전
        enemy.EnemyBase.LookAtPlayer();

        // 플레이어가 공격 범위 밖이면 추격
        if (!enemy.EnemyBase.IsPlayerInAttackRange())
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // 플레이어를 감지 못하면 (벽 뒤 등) 추격
        if (!enemy.EnemyBase.PlayerDetected)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // 공격 실행
        if (enemy.CanAttack())
        {
            enemy.Attack();
        }
    }

    public override void Exit()
    {
    }
}
