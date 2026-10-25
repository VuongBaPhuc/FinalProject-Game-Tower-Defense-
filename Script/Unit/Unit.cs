

using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    [SerializeField] private AIPawn m_AiPawn;
    [SerializeField] private List<UnitActionSO> m_Actions = new();
    public bool IsMoving;

    public IReadOnlyList<UnitActionSO> Actions => m_Actions;

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
}
