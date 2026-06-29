using UnityEngine;

public abstract class StateBehaviour<StateContextType> : ScriptableObject, IStateBehaviour<StateContextType> where StateContextType : IStateContext
{
    public abstract BehaviourActions GetOnEnter(StateContextType context);
    public abstract BehaviourActions GetOnTick(StateContextType context);
    public abstract BehaviourActions GetOnExit(StateContextType context);
}
