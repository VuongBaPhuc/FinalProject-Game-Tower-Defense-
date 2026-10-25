using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : SingletonManager<GameManager>
{
    public Unit ActiveUnit;
    [SerializeField] private PointToClick m_PointToClickPrefab;
    [SerializeField] private ActionBar m_ActionBar;
    private Vector2 m_InitialTouchPosition;
    private PointToClick m_CurrentPointToClick;
    private bool m_IsTrackingClick;

    private void Start()
    {
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

        if (HasClickedOnUnit(colliders, out var unit))
        {
            ActiveUnit = unit;
            if (m_ActionBar != null)
            {
                m_ActionBar.Show(ActiveUnit);
            }
        }
        else if (ActiveUnit != null)
        {
            ActiveUnit.MoveTo(worldPoint);
            ShowPointToClick(worldPoint);
        }
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
    // Temporary compatibility method called by Unit.Awake
    // Implement real initialization logic here if needed
    public void Test()
    {
        Debug.Log("GameManager Test method called.");
    }
}
