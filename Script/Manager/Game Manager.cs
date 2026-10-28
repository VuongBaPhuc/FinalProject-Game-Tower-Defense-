using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BuildingProcess))]
public class GameManager : SingletonManager<GameManager>
{
    public Unit ActiveUnit;
    [SerializeField] private PointToClick m_PointToClickPrefab;
    [SerializeField] private ActionBar m_ActionBar;
    [SerializeField] private BuildingProcess m_BuildingProcess;
    private Vector2 m_InitialTouchPosition;
    private PointToClick m_CurrentPointToClick;
    private bool m_IsTrackingClick;
    private Unit m_BuildingUnit;
    private BuildActionSO m_PendingBuildAction;

    private void Start()
    {
        if (m_BuildingProcess == null)
        {
            m_BuildingProcess = GetComponent<BuildingProcess>();
            if (m_BuildingProcess == null)
            {
                m_BuildingProcess = gameObject.AddComponent<BuildingProcess>();
            }
        }

        if (m_ActionBar != null)
        {
            m_ActionBar.Show(null);
        }
    }

    void Update()
    {
        bool hasTouch = Input.touchCount > 0;
        Touch touch = hasTouch ? Input.GetTouch(0) : default;
        Vector2 inputPosition = hasTouch ? touch.position : Input.mousePosition;
        bool pointerDown = hasTouch
            ? touch.phase == TouchPhase.Began
            : Input.GetMouseButtonDown(0);
        bool pointerUp = hasTouch
            ? touch.phase == TouchPhase.Ended
            : Input.GetMouseButtonUp(0);

        if (pointerDown)
        {
            m_IsTrackingClick = !IsPointerOverUI(hasTouch ? touch.fingerId : -1);
            if (m_IsTrackingClick)
            {
                m_InitialTouchPosition = inputPosition;
            }
        }

        if (pointerUp && m_IsTrackingClick)
        {
            m_IsTrackingClick = false;
            if (Vector2.Distance(m_InitialTouchPosition, inputPosition) < 10)
            {
                DetectClick(inputPosition);
            }
        }
    }

    private bool IsPointerOverUI(int pointerId)
    {
        return EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    void DetectClick(Vector2 inputPosition)
    {
        if (Camera.main == null)
        {
            return;
        }

        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputPosition);
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPoint);

        if (m_PendingBuildAction != null)
        {
            if (HasClickedOnUnit(colliders, out var clickedUnit))
            {
                CancelBuildPlacement();
                ActiveUnit = clickedUnit;
                m_ActionBar?.Show(ActiveUnit);
                return;
            }

            Unit builder = m_BuildingUnit;
            BuildActionSO action = m_PendingBuildAction;
            CancelBuildPlacement();

            if (m_BuildingProcess == null)
            {
                Debug.LogError("GameManager requires a BuildingProcess component.", this);
                return;
            }

            m_BuildingProcess.BuildAt(
                builder,
                action,
                new Vector3(worldPoint.x, worldPoint.y, 0f));
            return;
        }

        if (HasClickedOnUnit(colliders, out var unit))
        {
            CancelActiveGathering();
            ActiveUnit = unit;
            if (m_ActionBar != null)
            {
                m_ActionBar.Show(ActiveUnit);
            }
        }
        else if (ActiveUnit is WorkerUnit worker &&
                 TryFindResourceNode(colliders, out var resourceNode))
        {
            worker.StartGathering(resourceNode);
        }
        else if (ActiveUnit != null)
        {
            if (ActiveUnit is HumanoidUnit humanoid)
            {
                humanoid.CancelAutoAttack();
            }

            CancelActiveGathering();
            ActiveUnit.MoveTo(worldPoint);
            ShowPointToClick(worldPoint);
        }
    }

    private bool TryFindResourceNode(Collider2D[] colliders, out TreeResourceNode resourceNode)
    {
        foreach (Collider2D collider in colliders)
        {
            resourceNode = collider.GetComponent<TreeResourceNode>();
            if (resourceNode == null)
            {
                resourceNode = collider.GetComponentInParent<TreeResourceNode>();
            }

            if (resourceNode != null)
            {
                return true;
            }
        }

        resourceNode = null;
        return false;
    }

    private void CancelActiveGathering()
    {
        if (ActiveUnit is WorkerUnit worker)
        {
            worker.CancelGathering();
        }
    }

    public void BeginBuildPlacement(Unit builder, BuildActionSO action)
    {
        if (builder == null || action == null)
        {
            Debug.LogError("Cannot enter build placement without a builder and build action.", this);
            return;
        }

        if (!builder.CanMove)
        {
            Debug.LogError($"Builder '{builder.name}' has no AIPawn component.", builder);
            return;
        }

        m_BuildingUnit = builder;
        m_PendingBuildAction = action;
        Debug.Log($"Click a location to build '{action.DisplayName}'.", builder);
    }

    private void CancelBuildPlacement()
    {
        m_BuildingUnit = null;
        m_PendingBuildAction = null;
    }

    private void ShowPointToClick(Vector2 worldPoint)
    {
        if (m_PointToClickPrefab == null)
        {
            return;
        }

        if (m_CurrentPointToClick != null)
        {
            Destroy(m_CurrentPointToClick.gameObject);
        }

        m_CurrentPointToClick = Instantiate(
            m_PointToClickPrefab,
            new Vector3(worldPoint.x, worldPoint.y, 0f),
            Quaternion.identity);
    }


    bool HasClickedOnUnit(Collider2D[] colliders, out Unit unit)
    {
        foreach (Collider2D collider in colliders)
        {
            Unit clickedUnit = collider.GetComponent<Unit>();
            if (clickedUnit == null)
            {
                clickedUnit = collider.GetComponentInParent<Unit>();
            }

            if (clickedUnit != null)
            {
                unit = clickedUnit;
                return true;
            }
        }

        unit = null;
        return false;
    }
}
