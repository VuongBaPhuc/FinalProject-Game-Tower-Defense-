using UnityEngine;

public class AIPawn : MonoBehaviour
{
    // Movement speed in world units per second.
    [SerializeField]
    private float m_Speed = 5f;

    // Null means the pawn currently has no movement destination.
    private Vector3? m_Destination;

    // Exposes the current destination so other systems can detect when movement ends.
    public Vector3? Destination => m_Destination;

    // Moves toward the destination and clears it after the pawn arrives.
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
    // Starts moving toward the requested world position.
    public void SetDestination(Vector3 destination)
    {
        m_Destination = destination;
    }

    // Cancels the current movement command.
    public void StopMoving()
    {
        m_Destination = null;
    }
}
