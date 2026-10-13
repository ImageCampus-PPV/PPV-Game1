using System;

namespace GreenAbyss.FSM
{
    public abstract class State
    {
        public Action<Type> changeState;
        public abstract GreenAbyss.FSM.BehaviourActions GetOnEnterBehaviour(params object[] parameters);
        public abstract GreenAbyss.FSM.BehaviourActions GetOnTickBehaviour(params object[] parameters);
        public abstract GreenAbyss.FSM.BehaviourActions GetOnExitBehaviour(params object[] parameters);
    }

}