using UnityEngine;

public class GoblinUnit : HumanoidUnit
{
    [Header("Targeting")]
    // Maximum distance at which this enemy searches for opponents.
    [SerializeField, Min(0f)] private float m_DetectionRadius = 5f;
    // Distance at which the enemy stops moving and attacks.
    [SerializeField, Min(0f)] private float m_AttackRadius = 1.5f;
    // Limits target searches to colliders on the selected layers.
    [SerializeField] private LayerMask m_TargetLayers = Physics2D.AllLayers;

    [Header("Combat")]
    // Damage dealt to the target on each attack.
    [SerializeField, Min(0.1f)] private float m_Damage = 10f;
    // Minimum time between consecutive attacks.
    [SerializeField, Min(0.1f)] private float m_AttackInterval = 1f;
    // Optional Animator Trigger played after an attack.
    [SerializeField] private string m_AttackTrigger;

    // Opponent currently being pursued or attacked.
    private Unit m_Target;
    // Time when the next attack is allowed.
    private float m_NextAttackTime;
    // Cached reference used to display the enemy's current health.
    private WorldHealthBar m_HealthBar;

    // Identifies this unit as an enemy for team-based targeting.
    public override UnitTeam Team => UnitTeam.Enemy;

    // Reports this wave Goblin before the shared unit logic destroys its GameObject.
    protected override void Die()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.NotifyGoblinDefeated(this);
        }

        base.Die();
    }

    // Refreshes the health bar, then pursues the nearest valid opponent or attacks it in range.
    private void LateUpdate()
    {
        // Find the health bar once; it is optional on the enemy prefab.
        if (m_HealthBar == null)
        {
            m_HealthBar = GetComponent<WorldHealthBar>();
        }

        if (m_HealthBar != null)
        {
            m_HealthBar.SetHealth(CurrentHealth, MaxHealth);
        }

        // Do not run enemy AI if this unit has been configured as another team.
        if (Team != UnitTeam.Enemy)
        {
            return;
        }

        UpdateTarget();
        if (m_Target == null)
        {
            // Stop the enemy from continuing toward a target that is no longer available.
            if (TryGetComponent<AIPawn>(out var pawn))
            {
                pawn.StopMoving();
            }

            return;
        }

        float distance = Vector2.Distance(transform.position, m_Target.transform.position);
        // Stop within attack range so movement does not carry the enemy through its target.
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
            // Move to a point just outside the target instead of its exact center.
            Vector3 destination = m_Target.transform.position - (Vector3)(direction * m_AttackRadius * 0.9f);
            MoveTo(destination);
        }
    }

    // Keeps the current target while valid and nearby; otherwise searches for the closest opponent.
    private void UpdateTarget()
    {
        // Avoid repeating the overlap search while the existing target is still valid.
        if (IsValidTarget(m_Target) &&
            Vector2.Distance(transform.position, m_Target.transform.position) <= m_DetectionRadius)
        {
            return;
        }

        m_Target = null;
        float nearestDistance = m_DetectionRadius;

        // Inspect colliders in range and select the closest valid opposing unit.
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, m_DetectionRadius, m_TargetLayers))
        {
            // The collider may belong to a child object, so find its Unit in the parent hierarchy.
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

    // Excludes missing objects, this unit, and units on the same team.
    private bool IsValidTarget(Unit candidate)
    {
        return candidate != null && candidate != this && candidate.Team != Team;
    }

    // Applies damage at the configured interval and optionally plays the attack animation.
    private void AttackTarget()
    {
        if (Time.time < m_NextAttackTime)
        {
            return;
        }

        // Apply damage and schedule the next attack using the configured interval.
        m_Target.TakeDamage(m_Damage);
        m_NextAttackTime = Time.time + m_AttackInterval;

        // Play the configured attack animation only when a trigger name is provided.
        if (!string.IsNullOrWhiteSpace(m_AttackTrigger))
        {
            PlayAnimationTrigger(m_AttackTrigger);
        }
    }

    // Shows detection and attack ranges in the Scene view when this enemy is selected.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, m_DetectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_AttackRadius);
    }
}
