using UnityEngine;

public class FindTargetQuery : ICommandQuery<Transform>
{
    public float Range { get; }
    public LayerMask TargetLayer { get; }
    public bool DebugArea { get; }
    public FindTargetQuery(float range, LayerMask targetLayer, bool debugArea = false) 
    { 
        Range = range; 
        TargetLayer = targetLayer;
        DebugArea = debugArea;
    }
}
