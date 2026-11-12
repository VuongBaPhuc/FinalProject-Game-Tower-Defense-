

using System.Collections.Generic;
using UnityEngine;

public enum UnitTeam
{
    // Units controlled by the player.
    Player,
    // Units controlled by the opposing side.
    Enemy
}

// Shared health, movement, team, action, and animation behavior for game units.
public abstract class Unit : MonoBehaviour
{
    // Optional movement component assigned in the Inspector or found on this GameObject.
    [SerializeField] private AIPawn m_AiPawn;
    // Actions shown when this unit is selected.
    [SerializeField] private List<UnitActionSO> m_Actions = new();
    // Team used by combat and target selection.
    [SerializeField] private UnitTeam m_Team = UnitTeam.Player;
    // Health value restored when this unit is created.
    [SerializeField, Min(1f)] private float m_MaxHealth = 100f;
    public bool IsMoving;

    // Read-only view of the actions configured for this unit.
    public IReadOnlyList<UnitActionSO> Actions => m_Actions;
    // Can be overridden by specialized units with a fixed team.
    public virtual UnitTeam Team => m_Team;
    // Health remaining after damage has been applied.
    public float CurrentHealth { get; private set; }
    public float MaxHealth => m_MaxHealth;
    // True while the movement component has an active destination.
    public bool HasPendingMove => m_AiPawn != null && m_AiPawn.Destination.HasValue;
    public virtual bool CanMove => m_AiPawn != null;

    protected Animator m_Animator;

    // Initializes health and caches optional Animator and movement components.
    protected virtual void Awake()
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
    // Sends a movement request when this unit has an AIPawn component.
    public void MoveTo(Vector3 destination)
    {
        if (CanMove && m_AiPawn != null)
        {
            m_AiPawn.SetDestination(destination);
        }
    }

    // Reduces health, shows a damage popup, and destroys the unit when health reaches zero.
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
            Die();
        }
    }

    // Allows specialized units to react before their GameObject is destroyed.
    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    // Activates a named Animator Trigger after validating the Animator and parameter.
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
