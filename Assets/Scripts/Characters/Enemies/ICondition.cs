public interface ICondition<StateContextType> where StateContextType : IStateContext
{
    bool Evaluate(StateContextType context);
}
