using UnityEngine;

public class WorkerUnit : HumanoidUnit
{
    [Header("Gathering")]
    [SerializeField, Min(0.1f)] private float m_GatherRange = 1f;
    [SerializeField, Min(0.1f)] private float m_ChopDuration = 3f;
    [SerializeField, Min(1)] private int m_WoodPerGather = 1;
    [SerializeField, Min(1)] private int m_CarryCapacity = 5;
    [SerializeField, Min(0.1f)] private float m_CastleDepositRange = 1f;
    [SerializeField, Min(0.1f)] private float m_FoodDamagePerChop = 25f;
    [SerializeField] private string m_GatherTrigger = "Chop";
    [SerializeField] private AnimationClip m_FoodGatherAnimationClip;
    [SerializeField] private string m_IsChoppingParameter = "IsChopping";
    [SerializeField] private string m_IsCarryingParameter = "IsCarrying";
    [SerializeField] private Sprite m_ChoppingIcon;
    [SerializeField] private Vector3 m_ChoppingIconOffsetOnTree = new(0f, 1.65f, 0f);
    [SerializeField, Min(0.1f)] private float m_ChoppingIconScale = 1.4f;
    [SerializeField] private GameObject m_CarriedWoodPrefab;
    [SerializeField] private Vector3 m_CarriedWoodOffset = new(0.28f, 0.1f, 0f);
    [SerializeField] private Vector3 m_CarriedWoodScale = Vector3.one * 0.65f;
    [SerializeField] private Sprite m_CarriedFoodSprite;
    [SerializeField] private Vector3 m_CarriedFoodOffset = new(0.28f, 0.1f, 0f);
    [SerializeField] private Vector3 m_CarriedFoodScale = Vector3.one * 0.65f;

    private TreeResourceNode m_GatherTarget;
    private SheepResourceNode m_FoodGatherTarget;
    private CastleUnit m_ReturnTarget;
    private float m_NextChopTime;
    private int m_CarriedWood;
    private int m_CarriedFood;
    private bool m_IsReturningToCastle;
    private bool m_ReportedMissingCastle;
    private bool m_ReportedMissingGatherTrigger;
    private bool m_ReportedMissingChoppingParameter;
    private bool m_ReportedMissingCarryingParameter;
    private bool m_ReportedMissingWoodPrefab;
    private TreeResourceNode m_IconTree;
    private GameObject m_CarriedWoodVisual;
    private SpriteRenderer m_CarriedWoodRenderer;
    private GameObject m_CarriedFoodVisual;
    private bool m_ReportedMissingFoodSprite;
    private bool m_ReportedMissingFoodAnimation;
    private RuntimeAnimatorController m_OriginalAnimatorController;
    private AnimatorOverrideController m_FoodGatherAnimatorController;
    private AnimationClip m_ChopAnimationClip;

    public int Wood => m_CarriedWood;
    public int CarriedWood => m_CarriedWood;
    public int CarriedFood => m_CarriedFood;

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
        StopChopping();
        SetFoodGatherAnimation(false);
        m_GatherTarget = resourceNode;
        m_FoodGatherTarget = null;
        m_ReturnTarget = null;
        m_IsReturningToCastle = false;
        m_NextChopTime = 0f;
        SetCarryingState(false);
        if (m_CarriedWood + m_CarriedFood >= m_CarryCapacity || m_CarriedFood > 0)
        {
            StartReturningToCastle();
        }
        else
        {
            MoveTo(resourceNode.transform.position);
        }
    }

    public void StartGathering(SheepResourceNode sheep)
    {
        if (sheep == null)
        {
            Debug.LogError("Cannot gather food from a missing Sheep.", this);
            return;
        }

        if (!CanMove)
        {
            Debug.LogError($"Worker '{name}' needs an AIPawn component to gather food.", this);
            return;
        }

        if (!sheep.IsAlive)
        {
            Debug.LogWarning($"Sheep '{sheep.name}' is no longer available.", sheep);
            return;
        }

        CancelAutoAttack();
        StopChopping();
        SetFoodGatherAnimation(true);
        m_GatherTarget = null;
        m_FoodGatherTarget = sheep;
        m_ReturnTarget = null;
        m_IsReturningToCastle = false;
        m_NextChopTime = 0f;
        SetCarryingState(false);

        if (m_CarriedWood + m_CarriedFood >= m_CarryCapacity || m_CarriedWood > 0)
        {
            StartReturningToCastle();
        }
        else
        {
            MoveTo(sheep.transform.position);
        }
    }

    public void CancelGathering()
    {
        m_GatherTarget = null;
        m_FoodGatherTarget = null;
        m_ReturnTarget = null;
        m_IsReturningToCastle = false;
        m_NextChopTime = 0f;
        SetFoodGatherAnimation(false);
        StopChopping();
        GetComponent<AIPawn>()?.StopMoving();
    }

    private void UpdateGathering()
    {
        if (m_IsReturningToCastle)
        {
            UpdateReturningToCastle();
            return;
        }

        if (m_FoodGatherTarget != null)
        {
            UpdateFoodGathering();
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

        if (m_CarriedWood + m_CarriedFood >= m_CarryCapacity)
        {
            StartReturningToCastle();
            return;
        }

        float distance = Vector2.Distance(transform.position, m_GatherTarget.transform.position);
        if (distance > m_GatherRange)
        {
            m_NextChopTime = 0f;
            StopChopping();
            MoveTo(m_GatherTarget.transform.position);
            return;
        }

        GetComponent<AIPawn>().StopMoving();
        if (m_NextChopTime <= 0f)
        {
            StartChopping();
            m_NextChopTime = Time.time + m_ChopDuration;
            return;
        }

        if (Time.time < m_NextChopTime)
        {
            return;
        }

        int amountToGather = Mathf.Min(
            m_WoodPerGather,
            m_CarryCapacity - m_CarriedWood - m_CarriedFood);
        int gathered = m_GatherTarget.Gather(amountToGather);
        if (gathered <= 0)
        {
            StartReturningToCastle();
            return;
        }

        m_CarriedWood += gathered;
        m_NextChopTime = 0f;
        StopChopping();
        Debug.Log($"{name} gathered {gathered} wood. Carried wood: {m_CarriedWood}/{m_CarryCapacity}.", this);

        if (m_CarriedWood + m_CarriedFood >= m_CarryCapacity ||
            m_GatherTarget == null ||
            !m_GatherTarget.HasResources)
        {
            StartReturningToCastle();
        }
    }

    private void UpdateFoodGathering()
    {
        if (m_FoodGatherTarget == null)
        {
            if (m_CarriedFood > 0)
            {
                StartReturningToCastle();
            }
            else
            {
                m_FoodGatherTarget = null;
            }

            return;
        }

        if (!m_FoodGatherTarget.IsAlive)
        {
            int remainingFoodYield = m_FoodGatherTarget.FoodYield;
            m_CarriedFood += Mathf.Min(
                remainingFoodYield,
                m_CarryCapacity - m_CarriedWood - m_CarriedFood);
            m_FoodGatherTarget = null;
            StartReturningToCastle();
            return;
        }

        if (m_CarriedWood + m_CarriedFood >= m_CarryCapacity)
        {
            StartReturningToCastle();
            return;
        }

        float distance = Vector2.Distance(transform.position, m_FoodGatherTarget.transform.position);
        if (distance > m_GatherRange)
        {
            m_NextChopTime = 0f;
            StopChopping();
            MoveTo(m_FoodGatherTarget.transform.position);
            return;
        }

        GetComponent<AIPawn>().StopMoving();
        if (m_NextChopTime <= 0f)
        {
            StartChopping();
            m_NextChopTime = Time.time + m_ChopDuration;
            return;
        }

        if (Time.time < m_NextChopTime)
        {
            return;
        }

        SheepResourceNode sheep = m_FoodGatherTarget;
        int foodYield = sheep.FoodYield;
        bool killed = sheep.ApplyHarvestDamage(m_FoodDamagePerChop);
        m_NextChopTime = 0f;
        StopChopping();

        if (!killed)
        {
            return;
        }

        int foodGathered = Mathf.Min(foodYield, m_CarryCapacity - m_CarriedWood - m_CarriedFood);
        m_CarriedFood += foodGathered;
        m_FoodGatherTarget = null;
        Debug.Log($"{name} gathered {foodGathered} food. Returning to Castle.", this);
        StartReturningToCastle();
    }

    private void StartReturningToCastle()
    {
        m_NextChopTime = 0f;
        StopChopping();
        if (m_CarriedWood + m_CarriedFood <= 0)
        {
            m_GatherTarget = null;
            m_FoodGatherTarget = null;
            return;
        }

        SetCarryingState(true);
        m_IsReturningToCastle = true;
        m_ReturnTarget = FindNearestCastle();
        if (m_ReturnTarget == null)
        {
            GetComponent<AIPawn>()?.StopMoving();
            if (!m_ReportedMissingCastle)
            {
                Debug.LogError($"Worker '{name}' is carrying resources but no CastleUnit was found.", this);
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
                    Debug.LogError($"Worker '{name}' cannot deliver resources because no CastleUnit is available.", this);
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
        if (m_CarriedWood > 0)
        {
            m_ReturnTarget.DepositWood(m_CarriedWood);
        }

        if (m_CarriedFood > 0)
        {
            m_ReturnTarget.DepositFood(m_CarriedFood);
        }

        m_CarriedWood = 0;
        m_CarriedFood = 0;
        SetCarryingState(false);
        m_ReturnTarget = null;
        m_IsReturningToCastle = false;
        m_ReportedMissingCastle = false;

        if ((m_GatherTarget == null || !m_GatherTarget.HasResources) &&
            (m_FoodGatherTarget == null || !m_FoodGatherTarget.IsAlive))
        {
            m_GatherTarget = null;
            m_FoodGatherTarget = null;
            return;
        }

        Vector3 nextTarget = m_GatherTarget != null
            ? m_GatherTarget.transform.position
            : m_FoodGatherTarget.transform.position;
        MoveTo(nextTarget);
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

    private void StartChopping()
    {
        SetChoppingState(true);
        if (m_GatherTarget != null)
        {
            m_GatherTarget.BeginChopping(
                this,
                m_ChoppingIcon,
                m_ChoppingIconOffsetOnTree,
                m_ChoppingIconScale);
            m_IconTree = m_GatherTarget;
        }

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
                $"Worker '{name}' cannot play the gathering animation. Add a Trigger named '{m_GatherTrigger}' to its Animator.",
                this);
            m_ReportedMissingGatherTrigger = true;
        }
    }

    private void SetFoodGatherAnimation(bool useFoodAnimation)
    {
        if (m_Animator == null)
        {
            return;
        }

        if (!useFoodAnimation)
        {
            if (m_OriginalAnimatorController != null)
            {
                m_Animator.runtimeAnimatorController = m_OriginalAnimatorController;
            }

            return;
        }

        if (m_FoodGatherAnimationClip == null)
        {
            if (!m_ReportedMissingFoodAnimation)
            {
                Debug.LogError(
                    $"Worker '{name}' needs the Gather_2 animation clip assigned to use it when gathering Sheep.",
                    this);
                m_ReportedMissingFoodAnimation = true;
            }

            return;
        }

        if (m_OriginalAnimatorController == null)
        {
            m_OriginalAnimatorController = m_Animator.runtimeAnimatorController;
        }

        if (m_OriginalAnimatorController == null)
        {
            Debug.LogError($"Worker '{name}' has no Animator Controller to override.", this);
            return;
        }

        if (m_ChopAnimationClip == null)
        {
            foreach (AnimationClip clip in m_OriginalAnimatorController.animationClips)
            {
                if (clip != null && clip.name == "Chop")
                {
                    m_ChopAnimationClip = clip;
                    break;
                }
            }
        }

        if (m_ChopAnimationClip == null)
        {
            Debug.LogError(
                $"Animator Controller on '{name}' does not contain a clip named 'Chop' to override.",
                this);
            return;
        }

        if (m_FoodGatherAnimatorController == null)
        {
            m_FoodGatherAnimatorController = new AnimatorOverrideController(m_OriginalAnimatorController);
            m_FoodGatherAnimatorController.name = $"{m_OriginalAnimatorController.name}_FoodGather";
            m_FoodGatherAnimatorController[m_ChopAnimationClip] = m_FoodGatherAnimationClip;
        }

        m_Animator.runtimeAnimatorController = m_FoodGatherAnimatorController;
    }

    private void StopChopping()
    {
        SetChoppingState(false);
        if (m_IconTree != null)
        {
            m_IconTree.EndChopping(this);
            m_IconTree = null;
        }
    }

    private void SetChoppingState(bool isChopping)
    {
        if (m_Animator == null || string.IsNullOrWhiteSpace(m_IsChoppingParameter))
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter in m_Animator.parameters)
        {
            if (parameter.name == m_IsChoppingParameter &&
                parameter.type == AnimatorControllerParameterType.Bool)
            {
                m_Animator.SetBool(m_IsChoppingParameter, isChopping);
                return;
            }
        }

        if (!m_ReportedMissingChoppingParameter)
        {
            Debug.LogError(
                $"Worker '{name}' needs a Bool Animator parameter named '{m_IsChoppingParameter}' to control the chopping state.",
                this);
            m_ReportedMissingChoppingParameter = true;
        }
    }

    private void SetCarryingState(bool isCarrying)
    {
        if (isCarrying)
        {
            ShowCarriedResources();
        }
        else
        {
            m_CarriedWoodVisual?.SetActive(false);
            m_CarriedFoodVisual?.SetActive(false);
        }

        if (m_Animator == null || string.IsNullOrWhiteSpace(m_IsCarryingParameter))
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter in m_Animator.parameters)
        {
            if (parameter.name == m_IsCarryingParameter &&
                parameter.type == AnimatorControllerParameterType.Bool)
            {
                m_Animator.SetBool(m_IsCarryingParameter, isCarrying);
                return;
            }
        }

        if (!m_ReportedMissingCarryingParameter)
        {
            Debug.LogError(
                $"Worker '{name}' needs a Bool Animator parameter named '{m_IsCarryingParameter}' to play its carrying animation.",
                this);
            m_ReportedMissingCarryingParameter = true;
        }
    }

    private void ShowCarriedResources()
    {
        if (m_CarriedWood > 0)
        {
            ShowCarriedWood();
        }

        if (m_CarriedFood > 0)
        {
            ShowCarriedFood();
        }
    }

    private void ShowCarriedWood()
    {
        if (m_CarriedWoodVisual == null)
        {
            if (m_CarriedWoodPrefab == null)
            {
                if (!m_ReportedMissingWoodPrefab)
                {
                    Debug.LogError($"Worker '{name}' has no carried wood prefab assigned.", this);
                    m_ReportedMissingWoodPrefab = true;
                }

                return;
            }

            m_CarriedWoodVisual = Instantiate(m_CarriedWoodPrefab, transform, false);
            m_CarriedWoodVisual.name = "CarriedWood";
            m_CarriedWoodVisual.transform.localPosition = m_CarriedWoodOffset;
            m_CarriedWoodVisual.transform.localRotation = Quaternion.identity;
            m_CarriedWoodVisual.transform.localScale = m_CarriedWoodScale;
            m_CarriedWoodRenderer = m_CarriedWoodVisual.GetComponentInChildren<SpriteRenderer>();

            SpriteRenderer workerRenderer = GetComponentInChildren<SpriteRenderer>();
            if (m_CarriedWoodRenderer != null && workerRenderer != null)
            {
                m_CarriedWoodRenderer.sortingLayerID = workerRenderer.sortingLayerID;
                m_CarriedWoodRenderer.sortingOrder = workerRenderer.sortingOrder + 1;
            }
        }

        m_CarriedWoodVisual.SetActive(true);
    }

    private void ShowCarriedFood()
    {
        if (m_CarriedFoodSprite == null)
        {
            if (!m_ReportedMissingFoodSprite)
            {
                Debug.LogError($"Worker '{name}' has no carried Food sprite assigned.", this);
                m_ReportedMissingFoodSprite = true;
            }

            return;
        }

        if (m_CarriedFoodVisual == null)
        {
            GameObject foodVisual = new("CarriedFood");
            foodVisual.transform.SetParent(transform, false);
            foodVisual.transform.localPosition = m_CarriedFoodOffset;
            foodVisual.transform.localRotation = Quaternion.identity;
            foodVisual.transform.localScale = m_CarriedFoodScale;

            SpriteRenderer foodRenderer = foodVisual.AddComponent<SpriteRenderer>();
            foodRenderer.sprite = m_CarriedFoodSprite;
            SpriteRenderer workerRenderer = GetComponentInChildren<SpriteRenderer>();
            if (workerRenderer != null)
            {
                foodRenderer.sortingLayerID = workerRenderer.sortingLayerID;
                foodRenderer.sortingOrder = workerRenderer.sortingOrder + 1;
            }

            m_CarriedFoodVisual = foodVisual;
        }

        m_CarriedFoodVisual.SetActive(true);
    }
}
