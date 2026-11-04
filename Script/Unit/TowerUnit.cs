using UnityEngine;

// Represents a ranged defensive structure that automatically attacks nearby enemies.
public class TowerUnit : Unit
{
    [Header("Ranged Defense")]
    [SerializeField, Min(0.1f)] private float m_AttackRange = 7f;
    [SerializeField, Min(0.1f)] private float m_Damage = 5f;
    [SerializeField, Min(0.1f)] private float m_AttackInterval = 1.25f;
    [SerializeField] private LayerMask m_TargetLayers = Physics2D.AllLayers;

    private float m_NextAttackTime;

    // Selects the nearest enemy in range and attacks it on the configured cooldown.
    private void Update()
    {
        if (Team != UnitTeam.Player || Time.time < m_NextAttackTime)
        {
            return;
        }

        GoblinUnit target = null;
        float nearestDistance = m_AttackRange;
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(
                     transform.position,
                     m_AttackRange,
                     m_TargetLayers))
        {
            GoblinUnit candidate = hit.GetComponentInParent<GoblinUnit>();
            if (candidate == null || candidate.Team == Team)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, candidate.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                target = candidate;
            }
        }

        if (target == null)
        {
            return;
        }

        target.TakeDamage(m_Damage);
        m_NextAttackTime = Time.time + m_AttackInterval;
    }

    // Draws the tower's long-range attack radius in red in the Scene view.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_AttackRange);
    }
}
