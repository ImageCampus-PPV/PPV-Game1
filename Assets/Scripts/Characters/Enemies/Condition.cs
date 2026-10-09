using UnityEngine;

public abstract class Condition<StateContextType> : ScriptableObject, ICondition<StateContextType> where StateContextType : IStateContext
{
    public abstract bool Evaluate(StateContextType context);
}

