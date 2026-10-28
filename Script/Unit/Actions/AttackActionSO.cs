using UnityEngine;

[CreateAssetMenu(fileName = "AttackAction", menuName = "Units/Actions/Attack")]
public class AttackActionSO : UnitActionSO
{
    [SerializeField] private string m_AttackTrigger = "Attack_1";
    [SerializeField, Min(0.1f)] private float m_DetectionRadius = 5f;
    [SerializeField, Min(0.1f)] private float m_AttackRadius = 1.5f;
    [SerializeField, Min(0.1f)] private float m_Damage = 10f;
    [SerializeField, Min(0.1f)] private float m_AttackInterval = 1f;

    public override void Execute(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogError($"Cannot execute attack action '{DisplayName}' without a unit.", this);
            return;
        }

        if (unit is not HumanoidUnit humanoid)
        {
            Debug.LogError($"Unit '{unit.name}' must be a HumanoidUnit to use this attack action.", unit);
            return;
        }

        humanoid.EnableAutoAttack(
            m_DetectionRadius,
            m_AttackRadius,
            m_Damage,
            m_AttackInterval,
            m_AttackTrigger);
    }
}
