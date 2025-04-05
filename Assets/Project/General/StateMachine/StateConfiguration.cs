using System.Collections.Generic;

namespace Assets.Scripts.General.StateMachine
{
    public struct StateConfiguration
    {
        public IState State;
        public List<TransitionConfiguration> Transitions;
    }


}