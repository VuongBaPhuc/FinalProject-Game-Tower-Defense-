using UnityEngine;

// Base type for stationary or placeable structures that share Unit behavior.
public class StructureUnit : Unit
{
    // Structures remain stationary even if a movement component is present on a prefab.
    public override bool CanMove => false;
}
