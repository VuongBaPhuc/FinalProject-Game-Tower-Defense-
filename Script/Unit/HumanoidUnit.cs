using UnityEngine;

public class HumanoidUnit : Unit
{
    protected Vector2 m_Velocity;
    protected Vector3 m_LastPosition;
    private bool m_HasSpeedParameter;
    private bool m_HasAttack1Parameter;
    private bool m_HasAttack2Parameter;
    private SpriteRenderer m_SpriteRenderer;
    private bool m_AutoAttackEnabled;
    private float m_DetectionRadius;
    private float m_AttackRadius;
    private float m_AttackDamage;
    private float m_AttackInterval;
    private float m_NextAttackTime;
    private string m_AttackTrigger;
    private AIPawn m_AutoAttackPawn;
    private Unit m_Target;

    public float CurrentSpeed => m_Velocity.magnitude;

    public void EnableAutoAttack(
        float detectionRadius,
        float attackRadius,
        float damage,
        float attackInterval,
        string attackTrigger)
    {
        if (!CanMove)
        {
            Debug.LogError($"Unit '{name}' needs an AIPawn to use automatic attack.", this);
            return;
        }

        if (detectionRadius <= 0f || attackRadius <= 0f ||
            damage <= 0f || attackInterval <= 0f)
        {
            Debug.LogError("Auto-attack radii, damage, and interval must be greater than zero.", this);
            return;
        }

        m_DetectionRadius = detectionRadius;
        m_AttackRadius = Mathf.Min(attackRadius, detectionRadius);
        m_AttackDamage = damage;
        m_AttackInterval = attackInterval;
        m_AttackTrigger = attackTrigger;
        m_NextAttackTime = 0f;
        m_Target = null;
        m_AutoAttackPawn = GetComponent<AIPawn>();
        m_AutoAttackEnabled = true;
    }

    public void CancelAutoAttack()
    {
        m_AutoAttackEnabled = false;
        m_Target = null;
        m_AutoAttackPawn?.StopMoving();
    }

    void Start()
    {
        m_LastPosition = transform.position;
        m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (m_Animator != null)
        {
            foreach (var parameter in m_Animator.parameters)
            {
                if (parameter.name == "Speed" && parameter.type == AnimatorControllerParameterType.Float)
                {
                    m_HasSpeedParameter = true;
                }
                else if (parameter.name == "Attack_1" && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    m_HasAttack1Parameter = true;
                }
                else if (parameter.name == "Attack_2" && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    m_HasAttack2Parameter = true;
                }
            }
        }
    }    

    protected virtual void Update()
    {
        UpdateAutoAttack();

        m_Velocity = new Vector2(
            (transform.position.x - m_LastPosition.x),
            (transform.position.y - m_LastPosition.y)
        ) / Time.deltaTime;

        m_LastPosition = transform.position;
        IsMoving = m_Velocity.magnitude > 0;

        if (m_SpriteRenderer != null && Mathf.Abs(m_Velocity.x) > 0.01f)
        {
            m_SpriteRenderer.flipX = m_Velocity.x < 0f;
        }

        if (m_HasSpeedParameter)
        {
            m_Animator.SetFloat("Speed", Mathf.Clamp01(CurrentSpeed));
        }
    }

    private void UpdateAutoAttack()
    {
        if (!m_AutoAttackEnabled || Team != UnitTeam.Player || m_AutoAttackPawn == null)
        {
            return;
        }

        if (m_Target == null ||
            m_Target.Team == Team ||
            Vector2.Distance(transform.position, m_Target.transform.position) > m_DetectionRadius)
        {
            m_Target = FindTarget();
        }

        if (m_Target == null)
        {
            m_AutoAttackPawn.StopMoving();
            return;
        }

        float distance = Vector2.Distance(transform.position, m_Target.transform.position);
        if (distance <= m_AttackRadius)
        {
            m_AutoAttackPawn.StopMoving();
            if (Time.time >= m_NextAttackTime)
            {
                m_Target.TakeDamage(m_AttackDamage);
                PlayAnimationTrigger(m_AttackTrigger);
                m_NextAttackTime = Time.time + m_AttackInterval;
            }

            return;
        }

        Vector2 direction = (m_Target.transform.position - transform.position).normalized;
        Vector3 stopPosition =
            m_Target.transform.position - (Vector3)(direction * m_AttackRadius * 0.9f);
        MoveTo(stopPosition);
    }

    private Unit FindTarget()
    {
        Unit nearestTarget = null;
        float nearestDistance = m_DetectionRadius;

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, m_DetectionRadius))
        {
            Unit candidate = hit.GetComponentInParent<Unit>();
            if (candidate == null || candidate == this || candidate.Team == Team)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, candidate.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = candidate;
            }
        }

        return nearestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        if (!m_AutoAttackEnabled)
        {
            return;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, m_DetectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_AttackRadius);
    }

    public void Attack1()
    {
        if (m_HasAttack1Parameter)
        {
            m_Animator.SetTrigger("Attack_1");
        }
    }

    public void Attack2()
    {
        if (m_HasAttack2Parameter)
        {
            m_Animator.SetTrigger("Attack_2");
        }
    }
}
