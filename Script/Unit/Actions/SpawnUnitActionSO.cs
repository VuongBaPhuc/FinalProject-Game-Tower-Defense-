using UnityEngine;

[CreateAssetMenu(fileName = "SpawnUnitAction", menuName = "Units/Actions/Spawn Unit")]
// Defines a House action that spends Food and creates a configured unit.
public class SpawnUnitActionSO : UnitActionSO, IResourceCostAction
{
    [SerializeField] private GameObject m_UnitPrefab;
    [SerializeField, Min(0)] private int m_FoodCost;
    [SerializeField] private Sprite m_FoodCostIcon;

    public GameObject UnitPrefab => m_UnitPrefab;
    public ResourceCostType CostType => ResourceCostType.Food;
    public int CostAmount => m_FoodCost;
    public Sprite CostIcon => m_FoodCostIcon;

    // Validates the selected House and asks the GameManager to train the unit.
    public override void Execute(Unit unit)
    {
        if (!(unit is HouseUnit house))
        {
            Debug.LogError($"Action '{DisplayName}' can only be used from a House.", unit);
            return;
        }

        if (m_UnitPrefab == null || m_UnitPrefab.GetComponent<Unit>() == null)
        {
            Debug.LogError($"Spawn action '{DisplayName}' requires a prefab with a Unit component.", this);
            return;
        }

        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError($"Cannot execute '{DisplayName}' because no GameManager exists in the scene.", this);
            return;
        }

        gameManager.TrySpawnUnit(house, this);
    }
}
