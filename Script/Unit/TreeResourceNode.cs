using UnityEngine;

public class TreeResourceNode : MonoBehaviour
{
    // Remaining wood that workers can harvest from this tree.
    [SerializeField, Min(1)] private int m_WoodRemaining = 10;
    // Optional Animator used for the tree's chopping appearance.
    [SerializeField] private Animator m_Animator;
    // Animator state played while a worker is chopping this tree.
    [SerializeField] private string m_ChoppingAnimationState = "Idle";

    // Indicates whether this tree can still provide wood.
    public bool HasResources => m_WoodRemaining > 0;
    public int WoodRemaining => m_WoodRemaining;

    // Worker currently interacting with this tree.
    private WorkerUnit m_ChoppingWorker;
    // Reusable icon shown above the tree while it is being chopped.
    private GameObject m_ChoppingIndicator;
    private SpriteRenderer m_TreeRenderer;
    private Sprite m_DefaultSprite;
    private bool m_ReportedMissingAnimationState;

    // Caches the original sprite and keeps the tree Animator disabled while idle.
    private void Awake()
    {
        m_TreeRenderer = GetComponent<SpriteRenderer>();
        if (m_TreeRenderer != null)
        {
            m_DefaultSprite = m_TreeRenderer.sprite;
        }

        if (m_Animator == null)
        {
            m_Animator = GetComponent<Animator>();
        }

        if (m_Animator != null)
        {
            m_Animator.enabled = false;
        }
    }

    // Shows chopping feedback and animation for the worker using this resource node.
    public void BeginChopping(
        WorkerUnit worker,
        Sprite icon,
        Vector3 iconOffset,
        float iconScale)
    {
        if (worker == null)
        {
            Debug.LogError("A WorkerUnit is required to show tree chopping feedback.", this);
            return;
        }

        if (icon == null)
        {
            Debug.LogError("Assign a chopping icon on the WorkerUnit.", worker);
            return;
        }

        if (m_ChoppingWorker != null && m_ChoppingWorker != worker)
        {
            EndChopping(m_ChoppingWorker);
        }

        m_ChoppingWorker = worker;
        ShowChoppingIndicator(icon, iconOffset, iconScale);
        PlayChoppingAnimation();
    }

    // Hides chopping feedback and restores the tree's idle appearance.
    public void EndChopping(WorkerUnit worker)
    {
        if (worker == null || m_ChoppingWorker != worker)
        {
            return;
        }

        m_ChoppingWorker = null;
        if (m_ChoppingIndicator != null)
        {
            m_ChoppingIndicator.SetActive(false);
        }

        StopChoppingAnimation();
    }

    // Removes up to the requested amount of wood and destroys the tree when depleted.
    public int Gather(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogError($"Gather amount must be greater than zero. Received {amount}.", this);
            return 0;
        }

        if (!HasResources)
        {
            return 0;
        }

        int gathered = Mathf.Min(amount, m_WoodRemaining);
        m_WoodRemaining -= gathered;

        if (m_WoodRemaining == 0)
        {
            Destroy(gameObject);
        }

        return gathered;
    }

    // Creates the indicator once, then updates its appearance and position for reuse.
    private void ShowChoppingIndicator(Sprite icon, Vector3 offset, float scale)
    {
        if (m_ChoppingIndicator == null)
        {
            m_ChoppingIndicator = new GameObject("ChoppingIndicator");
            m_ChoppingIndicator.transform.SetParent(transform, false);
            SpriteRenderer iconRenderer = m_ChoppingIndicator.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = icon;

            SpriteRenderer treeRenderer = GetComponent<SpriteRenderer>();
            if (treeRenderer != null)
            {
                iconRenderer.sortingLayerID = treeRenderer.sortingLayerID;
                iconRenderer.sortingOrder = treeRenderer.sortingOrder + 5;
            }
        }

        m_ChoppingIndicator.transform.localPosition = offset;
        m_ChoppingIndicator.transform.localScale = Vector3.one * scale;
        m_ChoppingIndicator.SetActive(true);
    }

    // Starts the configured Animator state if the controller contains it.
    private void PlayChoppingAnimation()
    {
        if (m_Animator == null || string.IsNullOrWhiteSpace(m_ChoppingAnimationState))
        {
            return;
        }

        int stateHash = Animator.StringToHash(m_ChoppingAnimationState);
        if (!m_Animator.HasState(0, stateHash))
        {
            if (!m_ReportedMissingAnimationState)
            {
                Debug.LogError(
                    $"Tree '{name}' Animator has no state named '{m_ChoppingAnimationState}'.",
                    this);
                m_ReportedMissingAnimationState = true;
            }

            return;
        }

        m_Animator.enabled = true;
        m_Animator.Play(stateHash, 0, 0f);
    }

    // Stops the tree animation and restores the sprite captured during initialization.
    private void StopChoppingAnimation()
    {
        if (m_Animator != null)
        {
            m_Animator.enabled = false;
        }

        if (m_TreeRenderer != null && m_DefaultSprite != null)
        {
            m_TreeRenderer.sprite = m_DefaultSprite;
        }
    }
}
