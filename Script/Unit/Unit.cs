

using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    [SerializeField] private AIPawn m_AiPawn;
    public bool IsMoving;

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
