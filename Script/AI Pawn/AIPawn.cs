using UnityEngine;

public class AIPawn : MonoBehaviour
{
    [SerializeField]

    private float m_Speed = 5f;

    private Vector3? m_Destination;

    public Vector3? Destination => m_Destination;

    void Update()
    {
        if (m_Destination.HasValue)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                m_Destination.Value,
                m_Speed * Time.deltaTime);

            if (transform.position == m_Destination.Value)
            {
                m_Destination = null;
            }    
        }    
        
    }
    // Allow external callers to set the current navigation destination
    public void SetDestination(Vector3 destination)
    {
        m_Destination = destination;
    }
}
