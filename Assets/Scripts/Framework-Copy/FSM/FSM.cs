using System;
using System.Collections.Generic;

namespace GreenAbyss.FSM
{
    public class FSM
    {
        private Type _currentState;
        private Dictionary<Type, State> _states;
        private Dictionary<Type, Func<object[]>> _behaviourOnTickParameters;
        private Dictionary<Type, Func<object[]>> _behaviourOnEnterParameters;
        private Dictionary<Type, Func<object[]>> _behaviourOnExitParameters;

        private BehaviourActions GetCurrentTickBehaviour => _states[_currentState].GetOnTickBehaviour
            (_behaviourOnTickParameters[_currentState]?.Invoke());
        private BehaviourActions GetCurrentOnEnterBehaviour => _states[_currentState].GetOnEnterBehaviour
            (_behaviourOnEnterParameters[_currentState]?.Invoke());
        private BehaviourActions GetCurrentOnExitBehaviour => _states[_currentState].GetOnExitBehaviour
            (_behaviourOnExitParameters[_currentState]?.Invoke());

        public FSM(Type defaultState)
        {
            _states = new Dictionary<Type, GreenAbyss.FSM.State>();

            _behaviourOnTickParameters = new Dictionary<Type, Func<object[]>>();
            _behaviourOnEnterParameters = new Dictionary<Type, Func<object[]>>();
            _behaviourOnExitParameters = new Dictionary<Type, Func<object[]>>();
            ForceState(defaultState);
        }

        public void AddState<TState>(Func<object[]> onTickParameters = null, Func<object[]> onEnterParameters = null, Func<object[]> onExitParameters = null) where TState : GreenAbyss.FSM.State, new()
        {
            Type stateType = typeof(TState);

            if (_states.ContainsKey(stateType))
                return;

            TState state = new TState();
            state.changeState += Transition;
            _states.Add(stateType, state);
            _behaviourOnTickParameters.Add(stateType, onTickParameters);
            _behaviourOnEnterParameters.Add(stateType, onEnterParameters);
            _behaviourOnExitParameters.Add(stateType, onExitParameters);

        }

        private void ForceState(Type state)
        {
            _currentState = state;
        }

        public Type GetState()
        {
            return _currentState;
        }

        public void Transition(Type stateType)
        {
            if (_states.ContainsKey(_currentState))
                ExcecuteBehaviour(GetCurrentOnExitBehaviour);

            _currentState = stateType;
            ExcecuteBehaviour(GetCurrentOnEnterBehaviour);
        }

        public void Tick()
        {
            if (!_states.ContainsKey(_currentState))
                return;

            ExcecuteBehaviour(GetCurrentTickBehaviour);
        }

        private void ExcecuteBehaviour(GreenAbyss.FSM.BehaviourActions behaviourActions)
        {
            if (behaviourActions.Equals(default))
                return;

            behaviourActions.UpdateBehaviours?.Invoke();

            behaviourActions.TransitionBehaviour?.Invoke();
        }

        public void Dispose()
        {

        }
    }
}
