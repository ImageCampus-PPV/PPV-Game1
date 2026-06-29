using UnityEngine;

public class FindTargetQuery : ICommandQuery<Transform>
{
    public float Range { get; }
    public LayerMask TargetLayer { get; }
    public FindTargetQuery(float range, LayerMask targetLayer) 
    { 
        Range = range; 
        TargetLayer = targetLayer; 
    }
}
