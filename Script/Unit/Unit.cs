

using System.Collections.Generic;
using UnityEngine;

public enum UnitTeam
{
    Player,
    Enemy
}

public abstract class Unit : MonoBehaviour
{
    [SerializeField] private AIPawn m_AiPawn;
    [SerializeField] private List<UnitActionSO> m_Actions = new();
    [SerializeField] private UnitTeam m_Team = UnitTeam.Player;
    [SerializeField, Min(1f)] private float m_MaxHealth = 100f;
    public bool IsMoving;

    public IReadOnlyList<UnitActionSO> Actions => m_Actions;
    public virtual UnitTeam Team => m_Team;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => m_MaxHealth;
    public bool HasPendingMove => m_AiPawn != null && m_AiPawn.Destination.HasValue;
    public bool CanMove => m_AiPawn != null;

    protected Animator m_Animator;

    protected void Awake()
    {
        CurrentHealth = m_MaxHealth;

        if (TryGetComponent<Animator>(out var animator))
        {
            m_Animator = animator;
        }

        if (m_AiPawn == null)
        {
            m_AiPawn = GetComponent<AIPawn>();
        }
    }
    public void MoveTo(Vector3 destination)
    {
        if (m_AiPawn != null)
        {
            m_AiPawn.SetDestination(destination);
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            Debug.LogError($"Damage must be greater than zero. Received {damage}.", this);
            return;
        }

        if (CurrentHealth <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        DamageNumberPopup.Show(transform, damage);

        if (CurrentHealth == 0f)
        {
            Destroy(gameObject);
        }
    }

    public void PlayAnimationTrigger(string triggerName)
    {
        if (string.IsNullOrWhiteSpace(triggerName))
        {
            Debug.LogError($"Unit '{name}' was given an empty animation trigger.", this);
            return;
        }

        if (m_Animator == null)
        {
            Debug.LogError($"Unit '{name}' has no Animator for trigger '{triggerName}'.", this);
            return;
        }

        foreach (AnimatorControllerParameter parameter in m_Animator.parameters)
        {
            if (parameter.name == triggerName &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                m_Animator.SetTrigger(triggerName);
                return;
            }
        }

        Debug.LogError(
            $"Animator on '{name}' has no Trigger parameter named '{triggerName}'.",
            this);
    }
}
