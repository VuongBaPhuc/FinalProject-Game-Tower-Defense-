using UnityEngine;

public class SheepResourceNode : MonoBehaviour
{
    // Health the sheep starts with and food awarded when it is harvested.
    [SerializeField, Min(1f)] private float m_MaxHealth = 100f;
    [SerializeField, Min(1)] private int m_FoodYield = 5;
    [Header("Wandering")]
    [SerializeField] private Vector2 m_WanderCenter = new(0f, -14f);
    [SerializeField, Min(0f)] private float m_WanderRadius = 4f;
    [SerializeField, Min(0.1f)] private float m_WanderSpeed = 0.8f;
    [SerializeField, Min(0.5f)] private float m_WanderPause = 2f;

    // Health bar displayed above the sheep.
    private WorldHealthBar m_HealthBar;
    private float m_CurrentHealth;
    private bool m_IsAlive = true;
    private bool m_IsBeingGathered;
    private bool m_IsWaitingAtWanderPoint;
    private Vector3 m_WanderTarget;
    private float m_NextWanderTime;

    // Public read-only status used by worker gathering logic.
    public bool IsAlive => m_IsAlive;
    public float CurrentHealth => m_CurrentHealth;
    public float MaxHealth => m_MaxHealth;
    public int FoodYield => m_FoodYield;

    // Initializes health and creates the health bar if the prefab does not have one.
    private void Awake()
    {
        m_CurrentHealth = m_MaxHealth;
        m_HealthBar = GetComponent<WorldHealthBar>();
        if (m_HealthBar == null)
        {
            m_HealthBar = gameObject.AddComponent<WorldHealthBar>();
        }

        m_HealthBar.SetHealth(m_CurrentHealth, m_MaxHealth);
        ChooseWanderTarget();
    }

    // Moves the sheep between random points near the enemy spawn area when not being gathered.
    private void Update()
    {
        if (!m_IsAlive || m_IsBeingGathered)
        {
            return;
        }

        if (m_IsWaitingAtWanderPoint)
        {
            if (Time.time < m_NextWanderTime)
            {
                return;
            }

            ChooseWanderTarget();
            m_IsWaitingAtWanderPoint = false;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            m_WanderTarget,
            m_WanderSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, m_WanderTarget) <= 0.05f)
        {
            m_IsWaitingAtWanderPoint = true;
            m_NextWanderTime = Time.time + m_WanderPause;
        }
    }

    // Pauses or resumes wandering while a worker is harvesting this sheep.
    public void SetBeingGathered(bool isBeingGathered)
    {
        m_IsBeingGathered = isBeingGathered;
        if (!isBeingGathered)
        {
            ChooseWanderTarget();
            m_IsWaitingAtWanderPoint = false;
        }
    }

    // Picks a destination inside the sheep's configured roaming radius.
    private void ChooseWanderTarget()
    {
        Vector2 offset = Random.insideUnitCircle * m_WanderRadius;
        m_WanderTarget = new Vector3(
            m_WanderCenter.x + offset.x,
            m_WanderCenter.y + offset.y,
            transform.position.z);
    }

    // Applies one harvest hit and destroys the sheep when its health reaches zero.
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
