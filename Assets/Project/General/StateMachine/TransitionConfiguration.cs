using System;

namespace Assets.Scripts.General.StateMachine
{
    public class TransitionConfiguration
    {
        public Type FromState;
        public Type ToState;
        public Func<bool> Condition;
        public TransitionConfiguration(Type fromState, Type toState, Func<bool> condition)
        {
            FromState = fromState;
            ToState = toState;
            Condition = condition;
        }
        public static TransitionConfiguration GetConfiguration<TFrom,TTo>(Func<bool> condition) where TFrom:IState where TTo:IState
        {
            return new TransitionConfiguration(typeof(TFrom),typeof(TTo),condition);
        }
        public static TransitionConfiguration GetConfiguration<TFrom>(IState toState,Func<bool> condition,Func<bool> extraCondition) where TFrom:IState 
        {
            return new TransitionConfiguration(typeof(TFrom), toState.GetType(), 
                () => condition() && extraCondition());
        }
        public static TransitionConfiguration GetConfiguration<TTo>(IState fromState,Func<bool> condition) where TTo:IState 
        {
            return new TransitionConfiguration( fromState.GetType(),typeof(TTo), condition);
        }
        public static TransitionConfiguration GetConfiguration<TFrom,TTo>(Func<bool> condition,float possibility) where TFrom:IState where TTo:IState
        {
            return new TransitionConfiguration(typeof(TFrom), typeof(TTo), 
                () => condition() && UnityEngine.Random.value >= possibility);

        }
        
    }       
}