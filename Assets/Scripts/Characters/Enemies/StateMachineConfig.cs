using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/State Machine Config")]
public class StateMachineConfig : ScriptableObject
{
    [System.Serializable]
    public class StateEntry
    {
        public string stateName;
        public StateBehaviour<IEnemyContext> behaviour;
    }

    [System.Serializable]
    public class TransitionEntry
    {
        public Condition<IEnemyContext> condition;
        [StateName] public string toState; 
    }

    [System.Serializable]
    public class StateTransitionGroup
    {
        [StateName] public string stateName;
        public List<TransitionEntry> transitions;
    }

    [System.Serializable]
    public class AnyTransitionEntry
    {
        public Condition<IEnemyContext> condition;
        [StateName] public string toState;
    }

    public List<StateEntry> states = new List<StateEntry>();
    public List<StateTransitionGroup> transitionsByState = new List<StateTransitionGroup>();
    public List<AnyTransitionEntry> anyTransitions = new List<AnyTransitionEntry>();

    public string DefaultState => states.Count > 0 ? states[0].stateName : "Patrol";
}

