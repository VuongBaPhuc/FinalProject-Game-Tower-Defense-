using UnityEngine;

public class GameManager : SingletonManager<GameManager>
{
    public Unit ActiveUnit;
    private Vector2 m_InitialTouchPosition;
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
        Vector2 worldPoint = (Vector2)Camera.main.ScreenToWorldPoint(inputPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (HasClickedOnUnit(hit, out var unit))
        {
            ActiveUnit = unit;
        }
        else
        {

        }
    }


    bool HasClickedOnUnit(RaycastHit2D hit, out Unit unit)
    {
        if(hit.collider != null && hit.collider.TryGetComponent<Unit>(out var ClickedUnit))
        {
            unit = ClickedUnit;
            return true;
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

