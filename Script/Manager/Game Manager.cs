using UnityEngine;

public class GameManager : SingletonManager<GameManager>
{
    public Unit ActiveUnit;
    [SerializeField] private PointToClick m_PointToClickPrefab;
    private Vector2 m_InitialTouchPosition;
    private PointToClick m_CurrentPointToClick;
    void Update()
    {
        Vector2 inputPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : Input.mousePosition;

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            m_InitialTouchPosition = inputPosition;
        }

        if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            if (Vector2.Distance(m_InitialTouchPosition, inputPosition) < 10)
            {
                DetectClick(inputPosition);
            } 
        }    
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
    }
}

