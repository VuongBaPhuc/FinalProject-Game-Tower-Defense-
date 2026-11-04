using UnityEngine;

[CreateAssetMenu(fileName = "BuildTower", menuName = "Units/Actions/Build Tower")]
// Stores the prefab, animations, and timing for a tower-building action.
public class BuildActionSO : UnitActionSO
{
    // Tower prefab instantiated after the builder reaches the placement location.
    [SerializeField] private GameObject m_TowerPrefab;
    // Builder and optional tower animation triggers, plus total construction time.
    [SerializeField] private string m_BuildTrigger = "Build";
    [SerializeField] private string m_TowerBuildTrigger;
    [SerializeField, Min(0.1f)] private float m_BuildDuration = 2f;

    public GameObject TowerPrefab => m_TowerPrefab;
    public string BuildTrigger => m_BuildTrigger;
    public string TowerBuildTrigger => m_TowerBuildTrigger;
    public float BuildDuration => m_BuildDuration;

    // Checks the build configuration and asks the GameManager to begin placement.
    public override void Execute(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogError($"Cannot execute build action '{DisplayName}' without a unit.", this);
            return;
        }

        if (m_TowerPrefab == null)
        {
            Debug.LogError($"Build action '{DisplayName}' has no Tower prefab assigned.", this);
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
