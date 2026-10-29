using UnityEngine;

public class TreeResourceNode : MonoBehaviour
{
    [SerializeField, Min(1)] private int m_WoodRemaining = 10;
    [SerializeField] private Animator m_Animator;
    [SerializeField] private string m_ChoppingAnimationState = "Idle";

    public bool HasResources => m_WoodRemaining > 0;
    public int WoodRemaining => m_WoodRemaining;

    private WorkerUnit m_ChoppingWorker;
    private GameObject m_ChoppingIndicator;
    private SpriteRenderer m_TreeRenderer;
    private Sprite m_DefaultSprite;
    private bool m_ReportedMissingAnimationState;

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
