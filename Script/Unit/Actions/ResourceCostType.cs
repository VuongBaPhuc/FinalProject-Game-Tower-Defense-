public enum ResourceCostType
{
    Wood,
    Food
}

public interface IResourceCostAction
{
    ResourceCostType CostType { get; }
    int CostAmount { get; }
    UnityEngine.Sprite CostIcon { get; }
}
