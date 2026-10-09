public interface IStateBehaviour<StateContextType> where StateContextType : IStateContext
{
    BehaviourActions GetOnEnter(StateContextType context);
    BehaviourActions GetOnTick(StateContextType context);
    BehaviourActions GetOnExit(StateContextType context);
}
