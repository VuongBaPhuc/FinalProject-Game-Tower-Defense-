using UnityEngine;

public class WorkerUnit : HumanoidUnit
{
    [Header("Gathering")]
    [SerializeField, Min(0.1f)] private float m_GatherRange = 1f;
    [SerializeField, Min(0.1f)] private float m_ChopDuration = 3f;
    [SerializeField, Min(1)] private int m_WoodPerGather = 1;
    [SerializeField, Min(1)] private int m_CarryCapacity = 5;
    [SerializeField, Min(0.1f)] private float m_CastleDepositRange = 1f;
    [SerializeField] private string m_GatherTrigger = "Chop";

    private TreeResourceNode m_GatherTarget;
    private CastleUnit m_ReturnTarget;
    private float m_NextChopTime;
    private int m_CarriedWood;
    private bool m_IsReturningToCastle;
    private bool m_ReportedMissingCastle;
    private bool m_ReportedMissingGatherTrigger;

    public int Wood => m_CarriedWood;
    public int CarriedWood => m_CarriedWood;

    protected override void Update()
    {
        base.Update();
        UpdateGathering();
    }

    public void StartGathering(TreeResourceNode resourceNode)
    {
        if (resourceNode == null)
        {
            Debug.LogError("Cannot gather from a missing resource node.", this);
            return;
        }

        if (!CanMove)
        {
            Debug.LogError($"Worker '{name}' needs an AIPawn component to gather resources.", this);
            return;
        }

        if (!resourceNode.HasResources)
        {
            Debug.LogWarning($"Resource node '{resourceNode.name}' is depleted.", resourceNode);
            return;
        }

        CancelAutoAttack();
        m_GatherTarget = resourceNode;
        m_ReturnTarget = null;
        m_IsReturningToCastle = false;
        m_NextChopTime = 0f;
        if (m_CarriedWood >= m_CarryCapacity)
        {
            StartReturningToCastle();
        }
        else
        {
            MoveTo(resourceNode.transform.position);
        }
    }

    public void CancelGathering()
    {
        m_GatherTarget = null;
        m_ReturnTarget = null;
        m_IsReturningToCastle = false;
        m_NextChopTime = 0f;
        GetComponent<AIPawn>()?.StopMoving();
    }

    private void UpdateGathering()
    {
        if (m_IsReturningToCastle)
        {
            UpdateReturningToCastle();
            return;
        }

        if (m_GatherTarget == null)
        {
            return;
        }

        if (!m_GatherTarget.HasResources)
        {
            StartReturningToCastle();
            return;
        }

        if (m_CarriedWood >= m_CarryCapacity)
        {
            StartReturningToCastle();
            return;
        }

        float distance = Vector2.Distance(transform.position, m_GatherTarget.transform.position);
        if (distance > m_GatherRange)
        {
            m_NextChopTime = 0f;
            MoveTo(m_GatherTarget.transform.position);
            return;
        }

        GetComponent<AIPawn>().StopMoving();
        if (m_NextChopTime <= 0f)
        {
            PlayGatherAnimation();
            m_NextChopTime = Time.time + m_ChopDuration;
            return;
        }

        if (Time.time < m_NextChopTime)
        {
            return;
        }

        int amountToGather = Mathf.Min(m_WoodPerGather, m_CarryCapacity - m_CarriedWood);
        int gathered = m_GatherTarget.Gather(amountToGather);
        if (gathered <= 0)
        {
            StartReturningToCastle();
            return;
        }

        m_CarriedWood += gathered;
        m_NextChopTime = 0f;
        Debug.Log($"{name} gathered {gathered} wood. Carried wood: {m_CarriedWood}/{m_CarryCapacity}.", this);

        if (m_CarriedWood >= m_CarryCapacity || m_GatherTarget == null || !m_GatherTarget.HasResources)
        {
            StartReturningToCastle();
        }
    }

    private void StartReturningToCastle()
    {
        m_NextChopTime = 0f;
        if (m_CarriedWood <= 0)
        {
            m_GatherTarget = null;
            return;
        }

        m_IsReturningToCastle = true;
        m_ReturnTarget = FindNearestCastle();
        if (m_ReturnTarget == null)
        {
            GetComponent<AIPawn>()?.StopMoving();
            if (!m_ReportedMissingCastle)
            {
                Debug.LogError($"Worker '{name}' is carrying wood but no CastleUnit was found.", this);
                m_ReportedMissingCastle = true;
            }

            return;
        }

        m_ReportedMissingCastle = false;
        MoveTo(m_ReturnTarget.transform.position);
    }

    private void UpdateReturningToCastle()
    {
        if (m_ReturnTarget == null)
        {
            m_ReturnTarget = FindNearestCastle();
            if (m_ReturnTarget == null)
            {
                if (!m_ReportedMissingCastle)
                {
                    Debug.LogError($"Worker '{name}' cannot deliver wood because no CastleUnit is available.", this);
                    m_ReportedMissingCastle = true;
                }

                return;
            }
        }

        float distance = Vector2.Distance(transform.position, m_ReturnTarget.transform.position);
        if (distance > m_CastleDepositRange)
        {
            MoveTo(m_ReturnTarget.transform.position);
            return;
        }

        GetComponent<AIPawn>().StopMoving();
        m_ReturnTarget.DepositWood(m_CarriedWood);
        m_CarriedWood = 0;
        m_ReturnTarget = null;
        m_IsReturningToCastle = false;
        m_ReportedMissingCastle = false;

        if (m_GatherTarget == null || !m_GatherTarget.HasResources)
        {
            m_GatherTarget = null;
            return;
        }

        MoveTo(m_GatherTarget.transform.position);
    }

    private CastleUnit FindNearestCastle()
    {
        CastleUnit nearestCastle = null;
        float nearestDistance = float.PositiveInfinity;

        foreach (CastleUnit castle in FindObjectsByType<CastleUnit>())
        {
            if (castle.Team != Team)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, castle.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestCastle = castle;
            }
        }

        return nearestCastle;
    }

    private void PlayGatherAnimation()
    {
        if (string.IsNullOrWhiteSpace(m_GatherTrigger))
        {
            return;
        }

        if (m_Animator != null)
        {
            foreach (AnimatorControllerParameter parameter in m_Animator.parameters)
            {
                if (parameter.name == m_GatherTrigger &&
                    parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    m_Animator.SetTrigger(m_GatherTrigger);
                    return;
                }
            }
        }

        if (!m_ReportedMissingGatherTrigger)
        {
            Debug.LogError(
                $"Worker '{name}' cannot play the gather animation. Add a Trigger named '{m_GatherTrigger}' to its Animator.",
                this);
            m_ReportedMissingGatherTrigger = true;
        }
    }
}
