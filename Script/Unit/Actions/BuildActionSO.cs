using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BuildTower", menuName = "Units/Actions/Build Tower")]
// Stores the prefab, animations, and timing for a tower-building action.
public class BuildActionSO : UnitActionSO, IResourceCostAction
{
    // Structure prefab instantiated after the builder reaches the placement location.
    [FormerlySerializedAs("m_TowerPrefab")]
    [SerializeField] private GameObject m_BuildingPrefab;
    // Builder and optional structure animation triggers, plus total construction time.
    [SerializeField] private string m_BuildTrigger = "Build";
    [FormerlySerializedAs("m_TowerBuildTrigger")]
    [SerializeField] private string m_BuildingBuildTrigger;
    [SerializeField, Min(0.1f)] private float m_BuildRange = 5f;
    [SerializeField, Min(0.1f)] private float m_BuildDuration = 2f;
    [SerializeField, Min(0)] private int m_WoodCost = 25;
    [SerializeField] private Sprite m_WoodCostIcon;

    public GameObject TowerPrefab => m_BuildingPrefab;
    public GameObject BuildingPrefab => m_BuildingPrefab;
    public string BuildTrigger => m_BuildTrigger;
    public string TowerBuildTrigger => m_BuildingBuildTrigger;
    public string BuildingBuildTrigger => m_BuildingBuildTrigger;
    public float BuildRange => m_BuildRange;
    public float BuildDuration => m_BuildDuration;
    public int WoodCost => m_WoodCost;
    public ResourceCostType CostType => ResourceCostType.Wood;
    public int CostAmount => m_WoodCost;
    public Sprite CostIcon => m_WoodCostIcon;

    // Checks the build configuration and asks the GameManager to begin placement.
    public override void Execute(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogError($"Cannot execute build action '{DisplayName}' without a unit.", this);
            return;
        }

        if (m_BuildingPrefab == null)
        {
            Debug.LogError($"Build action '{DisplayName}' has no building prefab assigned.", this);
            return;
        }

        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Cannot start building because no GameManager exists in the scene.", this);
            return;
        }

        gameManager.BeginBuildPlacement(unit, this);
    }
}
