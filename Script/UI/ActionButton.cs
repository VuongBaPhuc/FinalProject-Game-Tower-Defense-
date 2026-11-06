using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ActionButton : MonoBehaviour
{
    // Cached UI components and click callback for the currently assigned action.
    private Button m_Button;
    private Image m_Icon;
    private UnityAction m_ClickListener;
    private BuildActionSO m_BuildAction;
    private GameObject m_WoodCostBadge;
    private Image m_WoodCostIcon;
    private Text m_WoodCostText;
    private Color m_IconColor = Color.white;

    // Assigns the action icon and replaces the previous click callback.
    public void Initialize(UnitActionSO action, Unit unit)
    {
        m_Button = GetComponent<Button>();
        m_Icon = FindIcon();

        if (action == null || unit == null)
        {
            Debug.LogError("ActionButton cannot initialize without an action and a selected unit.", this);
            Hide();
            return;
        }

        if (m_Button == null || m_Icon == null)
        {
            Debug.LogError("ActionButton requires a Button and an icon Image.", this);
            return;
        }

        if (action.Icon == null)
        {
            Debug.LogError($"Action '{action.DisplayName}' requires an icon Sprite.", action);
            return;
        }

        if (m_ClickListener != null)
        {
            m_Button.onClick.RemoveListener(m_ClickListener);
        }

        m_Icon.sprite = action.Icon;
        m_IconColor = Color.white;
        m_Icon.color = m_IconColor;
        m_BuildAction = action as BuildActionSO;
        m_Button.interactable = true;
        ConfigureWoodCostBadge();
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

    // Updates the Build button's cost badge and availability from the current Wood balance.
    public void RefreshAvailability(GameManager gameManager)
    {
        if (m_Button == null || m_BuildAction == null)
        {
            return;
        }

        bool canAfford = gameManager != null && gameManager.CanAffordBuild(m_BuildAction);
        m_Button.interactable = canAfford;
        m_Icon.color = canAfford ? m_IconColor : Color.gray;
        if (m_WoodCostIcon != null)
        {
            m_WoodCostIcon.color = canAfford ? Color.white : Color.gray;
        }

        if (m_WoodCostText != null)
        {
            m_WoodCostText.color = canAfford ? Color.white : Color.gray;
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

        m_BuildAction = null;
        gameObject.SetActive(false);
    }

    // Adds a small Wood icon and cost badge to Build actions without changing other action buttons.
    private void ConfigureWoodCostBadge()
    {
        if (m_BuildAction == null)
        {
            if (m_WoodCostBadge != null)
            {
                m_WoodCostBadge.SetActive(false);
            }

            return;
        }

        if (m_WoodCostIcon == null || m_WoodCostText == null)
        {
            m_WoodCostBadge = new GameObject("WoodCostBadge", typeof(RectTransform));
            m_WoodCostBadge.transform.SetParent(transform, false);
            RectTransform badgeRect = m_WoodCostBadge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 0f);
            badgeRect.anchorMax = new Vector2(1f, 0f);
            badgeRect.pivot = new Vector2(1f, 0f);
            badgeRect.anchoredPosition = new Vector2(-3f, 3f);
            badgeRect.sizeDelta = new Vector2(58f, 28f);

            Image background = m_WoodCostBadge.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.75f);
            background.raycastTarget = false;

            GameObject iconObject = new("WoodIcon");
            iconObject.transform.SetParent(m_WoodCostBadge.transform, false);
            RectTransform iconRect = iconObject.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(3f, 0f);
            iconRect.sizeDelta = new Vector2(22f, 22f);
            m_WoodCostIcon = iconObject.AddComponent<Image>();
            m_WoodCostIcon.preserveAspect = true;
            m_WoodCostIcon.raycastTarget = false;

            Sprite[] woodSprites = Resources.LoadAll<Sprite>("Resources/W_Idle");
            if (woodSprites.Length == 0)
            {
                Debug.LogError("Could not load the Wood cost icon at Resources/Resources/W_Idle.png.", this);
            }
            else
            {
                m_WoodCostIcon.sprite = woodSprites[0];
            }

            GameObject costObject = new("WoodCost");
            costObject.transform.SetParent(m_WoodCostBadge.transform, false);
            RectTransform costRect = costObject.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0f, 0f);
            costRect.anchorMax = new Vector2(1f, 1f);
            costRect.offsetMin = new Vector2(27f, 0f);
            costRect.offsetMax = new Vector2(-2f, 0f);
            m_WoodCostText = costObject.AddComponent<Text>();
            m_WoodCostText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_WoodCostText.alignment = TextAnchor.MiddleCenter;
            m_WoodCostText.raycastTarget = false;
        }

        m_WoodCostBadge.SetActive(true);
        m_WoodCostText.text = m_BuildAction.WoodCost.ToString();
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
