using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.General.StateMachine
{
    public class StateMachineFactory
    {
        private readonly StateMachine _stateMachine;

        public StateMachineFactory(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void SetupStateMachine(List<StateConfiguration> configurations, Type initialStateType)
        {
            List<Type> allStateTypes = configurations.Select(x => x.State.GetType()).ToList();
            List<IState> allStates = configurations.Select(x => x.State).ToList();

            // Register states and their transitions
            foreach (var config in configurations)
            {
                foreach (var transition in config.Transitions)
                {
                    if (allStateTypes.Contains(transition.FromState) && allStateTypes.Contains(transition.ToState))
                    {
                        _stateMachine.AddTransition(
                            allStates[allStateTypes.IndexOf(transition.FromState)],
                            allStates[allStateTypes.IndexOf(transition.ToState)],
                            transition.Condition
                        );
                    }
                }
            }

            IState initialState = allStates[allStateTypes.IndexOf(initialStateType)];
            // Set initial state
            if (initialState != null)
            {
                _stateMachine.SetState(initialState);
            }
            else
            {
                Debug.LogError("Initial state is null.");
            }
        }
    }
}