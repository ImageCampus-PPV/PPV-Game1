using GreenAbyss.FSM;

public interface IStateBehaviour<StateContextType> where StateContextType : IStateContext
{
    GreenAbyss.FSM.BehaviourActions GetOnEnter(StateContextType context);
    GreenAbyss.FSM.BehaviourActions GetOnTick(StateContextType context);
    GreenAbyss.FSM.BehaviourActions GetOnExit(StateContextType context);
}
