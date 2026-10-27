

using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    [SerializeField] private AIPawn m_AiPawn;
    [SerializeField] private List<UnitActionSO> m_Actions = new();
    public bool IsMoving;

    public IReadOnlyList<UnitActionSO> Actions => m_Actions;
    public bool HasPendingMove => m_AiPawn != null && m_AiPawn.Destination.HasValue;
    public bool CanMove => m_AiPawn != null;

    protected Animator m_Animator;

    protected void Awake()
    {
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
