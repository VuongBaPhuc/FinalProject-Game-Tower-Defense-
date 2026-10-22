using UnityEngine;

public class HumanoidUnit : Unit
{
    protected Vector2 m_Velocity;
    protected Vector3 m_LastPosition;
    private bool m_HasSpeedParameter;
    private bool m_HasAttack1Parameter;
    private bool m_HasAttack2Parameter;
    private SpriteRenderer m_SpriteRenderer;

    public float CurrentSpeed => m_Velocity.magnitude;

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

    protected void Update()
    {
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

