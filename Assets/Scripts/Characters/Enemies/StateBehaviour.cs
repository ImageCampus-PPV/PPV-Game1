using GreenAbyss.FSM;
using UnityEngine;

public abstract class StateBehaviour<StateContextType> : ScriptableObject, IStateBehaviour<StateContextType> where StateContextType : IStateContext
{
    public abstract GreenAbyss.FSM.BehaviourActions GetOnEnter(StateContextType context);
    public abstract GreenAbyss.FSM.BehaviourActions GetOnTick(StateContextType context);
    public abstract GreenAbyss.FSM.BehaviourActions GetOnExit(StateContextType context);
}
