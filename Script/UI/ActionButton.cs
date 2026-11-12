using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ActionButton : MonoBehaviour
{
    // Cached UI components and click callback for the currently assigned action.
    private Button m_Button;
    private Image m_Icon;
    private UnityAction m_ClickListener;
    private IResourceCostAction m_CostAction;
    private Unit m_ActionUnit;
    private GameObject m_ResourceCostBadge;
    private Image m_ResourceCostIcon;
    private Text m_ResourceCostText;
    private Color m_IconColor = Color.white;

    // Assigns the action icon and replaces the previous click callback.
    public void Initialize(UnitActionSO action, Unit unit)
    {
        m_Button = GetComponent<Button>();
        m_Icon = FindIcon();

        if (m_ClickListener != null && m_Button != null)
        {
            m_Button.onClick.RemoveListener(m_ClickListener);
            m_ClickListener = null;
        }

        if (action == null || unit == null)
        {
            Debug.LogError("ActionButton cannot initialize without an action and a selected unit.", this);
            Hide();
            return;
        }

        if (m_Button == null || m_Icon == null)
        {
            Debug.LogError("ActionButton requires a Button and an icon Image.", this);
            Hide();
            return;
        }

        if (action.Icon == null)
        {
            Debug.LogError($"Action '{action.DisplayName}' requires an icon Sprite.", action);
            Hide();
            return;
        }

        m_Icon.sprite = action.Icon;
        m_IconColor = Color.white;
        m_Icon.color = m_IconColor;
        m_CostAction = action as IResourceCostAction;
        m_ActionUnit = unit;
        m_Button.interactable = true;
        ConfigureResourceCostBadge();
        m_ClickListener = () =>
        {
            if (action == null || unit == null || !isActiveAndEnabled)
            {
                Hide();
                return;
            }

            action.Execute(unit);
        };
        m_Button.onClick.AddListener(m_ClickListener);
        gameObject.SetActive(true);
        RefreshAvailability(GameManager.FindAnyObjectByType<GameManager>());
    }

    // Updates the action's cost badge and availability from friendly Castle resources.
    public void RefreshAvailability(GameManager gameManager)
    {
        if (m_Button == null || m_Icon == null)
        {
            return;
        }

        bool canAfford = m_CostAction == null ||
            (gameManager != null &&
             gameManager.CanAfford(m_CostAction.CostType, m_CostAction.CostAmount, m_ActionUnit));
        m_Button.interactable = canAfford;
        m_Icon.color = canAfford ? m_IconColor : Color.gray;
        if (m_ResourceCostIcon != null)
        {
            m_ResourceCostIcon.color = canAfford ? Color.white : Color.gray;
        }

        if (m_ResourceCostText != null)
        {
            m_ResourceCostText.color = canAfford ? Color.white : Color.gray;
        }
    }

    // Hides this button when the selected unit does not support its action.
    public void Hide()
    {
        if (m_Button == null)
        {
            m_Button = GetComponent<Button>();
        }

        if (m_ClickListener != null)
        {
            m_Button.onClick.RemoveListener(m_ClickListener);
            m_ClickListener = null;
        }

        if (m_Button != null)
        {
            m_Button.interactable = true;
        }

        m_CostAction = null;
        m_ActionUnit = null;
        gameObject.SetActive(false);
    }

    // Adds a resource icon and cost badge to actions that spend a resource.
    private void ConfigureResourceCostBadge()
    {
        if (m_CostAction == null)
        {
            if (m_ResourceCostBadge != null)
            {
                m_ResourceCostBadge.SetActive(false);
            }

            return;
        }

        if (m_ResourceCostIcon == null || m_ResourceCostText == null)
        {
            m_ResourceCostBadge = new GameObject("ResourceCostBadge", typeof(RectTransform));
            m_ResourceCostBadge.transform.SetParent(transform, false);
            RectTransform badgeRect = m_ResourceCostBadge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 0f);
            badgeRect.anchorMax = new Vector2(1f, 0f);
            badgeRect.pivot = new Vector2(1f, 0f);
            badgeRect.anchoredPosition = new Vector2(-3f, 3f);
            badgeRect.sizeDelta = new Vector2(58f, 28f);

            Image background = m_ResourceCostBadge.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.75f);
            background.raycastTarget = false;

            GameObject iconObject = new("ResourceIcon");
            iconObject.transform.SetParent(m_ResourceCostBadge.transform, false);
            RectTransform iconRect = iconObject.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(3f, 0f);
            iconRect.sizeDelta = new Vector2(22f, 22f);
            m_ResourceCostIcon = iconObject.AddComponent<Image>();
            m_ResourceCostIcon.preserveAspect = true;
            m_ResourceCostIcon.raycastTarget = false;

            GameObject costObject = new("ResourceCost");
            costObject.transform.SetParent(m_ResourceCostBadge.transform, false);
            RectTransform costRect = costObject.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0f, 0f);
            costRect.anchorMax = new Vector2(1f, 1f);
            costRect.offsetMin = new Vector2(27f, 0f);
            costRect.offsetMax = new Vector2(-2f, 0f);
            m_ResourceCostText = costObject.AddComponent<Text>();
            m_ResourceCostText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_ResourceCostText.alignment = TextAnchor.MiddleCenter;
            m_ResourceCostText.raycastTarget = false;
        }

        if (m_CostAction.CostIcon == null)
        {
            Debug.LogError("A resource-cost action requires a cost icon Sprite.", this);
        }

        m_ResourceCostBadge.SetActive(true);
        m_ResourceCostIcon.sprite = m_CostAction.CostIcon;
        m_ResourceCostText.text = m_CostAction.CostAmount.ToString();
    }

    // Finds the button's child icon, falling back to an Image on the button itself.
    private Image FindIcon()
    {
        foreach (Image image in GetComponentsInChildren<Image>(true))
        {
            if (image.gameObject != gameObject)
            {
                return image;
            }
        }

        return GetComponent<Image>();
    }
}
