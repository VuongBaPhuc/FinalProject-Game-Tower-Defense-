using UnityEngine;

public class SheepResourceNode : MonoBehaviour
{
    [SerializeField, Min(1f)] private float m_MaxHealth = 100f;
    [SerializeField, Min(1)] private int m_FoodYield = 5;

    private WorldHealthBar m_HealthBar;
    private float m_CurrentHealth;
    private bool m_IsAlive = true;

    public bool IsAlive => m_IsAlive;
    public float CurrentHealth => m_CurrentHealth;
    public float MaxHealth => m_MaxHealth;
    public int FoodYield => m_FoodYield;

    private void Awake()
    {
        m_CurrentHealth = m_MaxHealth;
        m_HealthBar = GetComponent<WorldHealthBar>();
        if (m_HealthBar == null)
        {
            m_HealthBar = gameObject.AddComponent<WorldHealthBar>();
        }

        m_HealthBar.SetHealth(m_CurrentHealth, m_MaxHealth);
    }

    public bool ApplyHarvestDamage(float damage)
    {
        if (damage <= 0f)
        {
            Debug.LogError($"Harvest damage must be greater than zero. Received {damage}.", this);
            return false;
        }

        if (!m_IsAlive)
        {
            return true;
        }

        m_CurrentHealth = Mathf.Max(0f, m_CurrentHealth - damage);
        m_HealthBar.SetHealth(m_CurrentHealth, m_MaxHealth);
        DamageNumberPopup.Show(transform, damage);

        if (m_CurrentHealth > 0f)
        {
            return false;
        }

        m_IsAlive = false;
        Destroy(gameObject);
        return true;
    }
}
