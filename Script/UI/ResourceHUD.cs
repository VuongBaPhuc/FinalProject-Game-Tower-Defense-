using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceHUD : MonoBehaviour
{
    [SerializeField] private Image m_WoodIcon;
    [SerializeField] private Text m_WoodCount;
    [SerializeField] private Sprite m_FoodIconSprite;
    [SerializeField] private Image m_FoodIcon;
    [SerializeField] private Text m_FoodCount;

    private readonly List<CastleUnit> m_Castles = new();
    private RectTransform m_HudRoot;
    private bool m_IsGeneratedHud;
    private bool m_IsGeneratedFoodHud;

    private void OnEnable()
    {
        bool hasManualHud = m_WoodIcon != null || m_WoodCount != null;
        if (hasManualHud)
        {
            ConfigureManualHud();
        }
        else
        {
            CreateHud();
        }

        ConfigureFoodHud();

        m_Castles.Clear();
        foreach (CastleUnit castle in FindObjectsByType<CastleUnit>())
        {
            castle.WoodChanged += HandleWoodChanged;
            castle.FoodChanged += HandleResourceChanged;
            m_Castles.Add(castle);
        }

        RefreshResourceCounts();
    }

    private void OnDisable()
    {
        foreach (CastleUnit castle in m_Castles)
        {
            if (castle != null)
            {
                castle.WoodChanged -= HandleWoodChanged;
                castle.FoodChanged -= HandleResourceChanged;
            }
        }

        m_Castles.Clear();
        if (m_IsGeneratedFoodHud)
        {
            if (m_FoodIcon != null)
            {
                Destroy(m_FoodIcon.gameObject);
            }

            if (m_FoodCount != null)
            {
                Destroy(m_FoodCount.gameObject);
            }

            m_FoodIcon = null;
            m_FoodCount = null;
            m_IsGeneratedFoodHud = false;
        }

        if (m_IsGeneratedHud && m_HudRoot != null)
        {
            Destroy(m_HudRoot.gameObject);
            m_HudRoot = null;
            m_WoodIcon = null;
            m_WoodCount = null;
            m_FoodIcon = null;
            m_FoodCount = null;
            m_IsGeneratedHud = false;
        }
    }

    private void ConfigureManualHud()
    {
        if (m_WoodIcon == null || m_WoodCount == null)
        {
            Debug.LogError(
                "Assign both the Wood Icon Image and Wood Count Text in ResourceHUD.",
                this);
            return;
        }

        if (m_WoodIcon.sprite == null)
        {
            Sprite[] woodSprites = Resources.LoadAll<Sprite>("Resources/W_Idle");
            if (woodSprites.Length == 0)
            {
                Debug.LogError("Could not load the wood icon at Resources/Resources/W_Idle.png.", this);
            }
            else
            {
                m_WoodIcon.sprite = woodSprites[0];
            }
        }

        m_WoodIcon.preserveAspect = true;
        if (m_WoodCount.font == null)
        {
            m_WoodCount.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        m_WoodCount.raycastTarget = false;
    }

    private void ConfigureFoodHud()
    {
        if (m_FoodIcon == null && m_FoodCount == null)
        {
            if (m_FoodIconSprite == null)
            {
                Debug.LogError("Assign a Food icon Sprite in ResourceHUD.", this);
                return;
            }

            if (m_WoodIcon == null || m_WoodCount == null)
            {
                Debug.LogError("ResourceHUD needs its Wood Icon and Wood Count before creating the Food row.", this);
                return;
            }

            m_FoodIcon = Instantiate(m_WoodIcon, m_WoodIcon.transform.parent);
            m_FoodIcon.name = "Food";
            m_FoodIcon.sprite = m_FoodIconSprite;
            m_FoodIcon.preserveAspect = true;
            m_FoodIcon.raycastTarget = false;
            m_FoodIcon.rectTransform.anchoredPosition += new Vector2(125f, 0f);

            m_FoodCount = Instantiate(m_WoodCount, m_WoodCount.transform.parent);
            m_FoodCount.name = "Food Count";
            m_FoodCount.text = "0";
            m_FoodCount.raycastTarget = false;
            m_FoodCount.rectTransform.anchoredPosition += new Vector2(125f, 0f);
            m_IsGeneratedFoodHud = true;
        }

        if (m_FoodIcon == null || m_FoodCount == null)
        {
            Debug.LogError(
                "Assign both the Food Icon Image and Food Count Text, or assign a Food icon Sprite for automatic setup.",
                this);
            return;
        }

        if (m_FoodIcon.sprite == null)
        {
            m_FoodIcon.sprite = m_FoodIconSprite;
        }

        if (m_FoodIcon.sprite == null)
        {
            Debug.LogError("The Food Icon Image has no Sprite assigned.", m_FoodIcon);
        }

        m_FoodIcon.preserveAspect = true;
        if (m_FoodCount.font == null)
        {
            m_FoodCount.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        m_FoodCount.raycastTarget = false;
    }

    private void CreateHud()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            Debug.LogError("ResourceHUD must be attached to a Canvas or one of its children.", this);
            return;
        }

        GameObject root = new("WoodResourceHUD", typeof(RectTransform));
        m_HudRoot = root.GetComponent<RectTransform>();
        m_HudRoot.SetParent(canvas.transform, false);
        m_HudRoot.anchorMin = Vector2.one;
        m_HudRoot.anchorMax = Vector2.one;
        m_HudRoot.pivot = Vector2.one;
        m_HudRoot.anchoredPosition = new Vector2(-24f, -24f);
        m_HudRoot.sizeDelta = new Vector2(300f, 48f);

        GameObject iconObject = new("WoodIcon", typeof(RectTransform), typeof(Image));
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.SetParent(m_HudRoot, false);
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(40f, 40f);

        m_WoodIcon = iconObject.GetComponent<Image>();
        Sprite[] woodSprites = Resources.LoadAll<Sprite>("Resources/W_Idle");
        if (woodSprites.Length == 0)
        {
            Debug.LogError("Could not load the wood icon at Resources/Resources/W_Idle.png.", this);
        }
        else
        {
            m_WoodIcon.sprite = woodSprites[0];
            m_WoodIcon.preserveAspect = true;
        }

        GameObject countObject = new("WoodCount", typeof(RectTransform), typeof(Text));
        RectTransform countRect = countObject.GetComponent<RectTransform>();
        countRect.SetParent(m_HudRoot, false);
        countRect.anchorMin = new Vector2(0f, 0.5f);
        countRect.anchorMax = new Vector2(0f, 0.5f);
        countRect.pivot = new Vector2(0f, 0.5f);
        countRect.anchoredPosition = new Vector2(48f, 0f);
        countRect.sizeDelta = new Vector2(102f, 40f);

        m_WoodCount = countObject.GetComponent<Text>();
        m_WoodCount.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        m_WoodCount.fontSize = 24;
        m_WoodCount.fontStyle = FontStyle.Bold;
        m_WoodCount.color = Color.white;
        m_WoodCount.alignment = TextAnchor.MiddleLeft;
        m_WoodCount.raycastTarget = false;
        m_IsGeneratedHud = true;
    }

    private void HandleWoodChanged(CastleUnit castle)
    {
        RefreshResourceCounts();
    }

    private void HandleResourceChanged(CastleUnit castle)
    {
        RefreshResourceCounts();
    }

    private void RefreshResourceCounts()
    {
        int totalWood = 0;
        int totalFood = 0;
        foreach (CastleUnit castle in m_Castles)
        {
            if (castle != null)
            {
                totalWood += castle.StoredWood;
                totalFood += castle.StoredFood;
            }
        }

        if (m_WoodCount != null)
        {
            m_WoodCount.text = totalWood.ToString();
        }

        if (m_FoodCount != null)
        {
            m_FoodCount.text = totalFood.ToString();
        }
    }
}
