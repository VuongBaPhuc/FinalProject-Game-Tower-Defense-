using UnityEngine;

public class EnemyUnit : HumanoidUnit
{
    [Header("Targeting")]
    [SerializeField, Min(0f)] private float m_DetectionRadius = 5f;
    [SerializeField, Min(0f)] private float m_AttackRadius = 1.5f;
    [SerializeField] private LayerMask m_TargetLayers = Physics2D.AllLayers;

    [Header("Combat")]
    [SerializeField, Min(0.1f)] private float m_Damage = 10f;
    [SerializeField, Min(0.1f)] private float m_AttackInterval = 1f;
    [SerializeField] private string m_AttackTrigger;

    private Unit m_Target;
    private float m_NextAttackTime;

    public override UnitTeam Team => UnitTeam.Enemy;

    private void LateUpdate()
    {
        if (Team != UnitTeam.Enemy)
        {
            return;
        }

        UpdateTarget();
        if (m_Target == null)
        {
            if (TryGetComponent<AIPawn>(out var pawn))
            {
                pawn.StopMoving();
            }

            return;
        }

        float distance = Vector2.Distance(transform.position, m_Target.transform.position);
        if (distance <= m_AttackRadius)
        {
            if (TryGetComponent<AIPawn>(out var pawn))
            {
                pawn.StopMoving();
            }

            AttackTarget();
            return;
        }

        if (CanMove)
        {
            Vector2 direction = (m_Target.transform.position - transform.position).normalized;
            Vector3 destination = m_Target.transform.position - (Vector3)(direction * m_AttackRadius * 0.9f);
            MoveTo(destination);
        }
    }

    private void UpdateTarget()
    {
        if (IsValidTarget(m_Target) &&
            Vector2.Distance(transform.position, m_Target.transform.position) <= m_DetectionRadius)
        {
            return;
        }

        m_Target = null;
        float nearestDistance = m_DetectionRadius;

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, m_DetectionRadius, m_TargetLayers))
        {
            Unit candidate = hit.GetComponentInParent<Unit>();
            if (!IsValidTarget(candidate))
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, candidate.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                m_Target = candidate;
            }
        }
    }

    private bool IsValidTarget(Unit candidate)
    {
        return candidate != null && candidate != this && candidate.Team != Team;
    }

    private void AttackTarget()
    {
        if (Time.time < m_NextAttackTime)
        {
            return;
        }

        m_Target.TakeDamage(m_Damage);
        m_NextAttackTime = Time.time + m_AttackInterval;

        if (!string.IsNullOrWhiteSpace(m_AttackTrigger))
        {
            PlayAnimationTrigger(m_AttackTrigger);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, m_DetectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_AttackRadius);
    }
}
