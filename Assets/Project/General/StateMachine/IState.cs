namespace Assets.Scripts.General.StateMachine {
    public interface IState {
        void Update() { }
        void FixedUpdate() { }
        void OnEnter() { }
        void OnExit() { }
        void Dispose() { }
    }
}