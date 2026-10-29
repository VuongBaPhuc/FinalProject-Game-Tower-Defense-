using UnityEngine;

public class WorldHealthBar : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float m_Width = 0.8f;
    [SerializeField, Min(0.01f)] private float m_Height = 0.08f;
    [SerializeField] private Vector3 m_Offset = new(0f, 0.75f, 0f);

    private static Sprite s_BarSprite;
    private SpriteRenderer m_Fill;
    private SpriteRenderer m_Background;

    private void Awake()
    {
        CreateBar();
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0f)
        {
            Debug.LogError($"Health bar max health must be greater than zero. Received {maxHealth}.", this);
            return;
        }

        if (m_Fill == null)
        {
            CreateBar();
        }

        float healthRatio = Mathf.Clamp01(currentHealth / maxHealth);
        m_Fill.transform.localPosition = new Vector3(
            -0.5f * (1f - healthRatio),
            0f,
            -0.01f);
        m_Fill.transform.localScale = new Vector3(healthRatio, 0.75f, 1f);
        m_Fill.color = Color.Lerp(Color.red, Color.green, healthRatio);
    }

    private void CreateBar()
    {
        if (m_Fill != null)
        {
            return;
        }

        GameObject background = new("HealthBarBackground");
        background.transform.SetParent(transform, false);
        background.transform.localPosition = m_Offset;
        background.transform.localScale = new Vector3(m_Width, m_Height, 1f);
        m_Background = background.AddComponent<SpriteRenderer>();
        m_Background.sprite = GetBarSprite();
        m_Background.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        SpriteRenderer ownerRenderer = GetComponent<SpriteRenderer>();
        if (ownerRenderer != null)
        {
            m_Background.sortingLayerID = ownerRenderer.sortingLayerID;
            m_Background.sortingOrder = ownerRenderer.sortingOrder + 10;
        }
        else
        {
            m_Background.sortingOrder = 10;
        }

        GameObject fill = new("HealthBarFill");
        fill.transform.SetParent(background.transform, false);
        m_Fill = fill.AddComponent<SpriteRenderer>();
        m_Fill.sprite = GetBarSprite();
        m_Fill.sortingLayerID = m_Background.sortingLayerID;
        m_Fill.sortingOrder = m_Background.sortingOrder + 1;
    }

    private static Sprite GetBarSprite()
    {
        if (s_BarSprite != null)
        {
            return s_BarSprite;
        }

        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        s_BarSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f);
        s_BarSprite.hideFlags = HideFlags.HideAndDontSave;
        return s_BarSprite;
    }
}
