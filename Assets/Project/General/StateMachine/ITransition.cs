namespace Assets.Scripts.General.StateMachine {
    public interface ITransition {
        IState To { get; }
        IPredicate Condition { get; }
    }
}