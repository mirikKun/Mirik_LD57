using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Scripts
{
    public interface IInputReader {
        Vector2 Direction { get; }
        void EnablePlayerActions();
    }

    [CreateAssetMenu(fileName = "InputReader", menuName = "InputReader")]
    public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions, IInputReader {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { };
        public event UnityAction EnableMouseControlCamera = delegate { };
        public event UnityAction DisableMouseControlCamera = delegate { };
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction<bool> Dash = delegate { };
        public event UnityAction<bool> Crouch = delegate { };
        public event UnityAction<bool> Action1 = delegate { };
        public event UnityAction<bool> Action2 = delegate { };
        public event UnityAction<bool> Action3 = delegate { };
        public event UnityAction<bool> Action4 = delegate { };
        public event UnityAction<bool> Attack = delegate { };
        public event UnityAction<bool>AttackAlt = delegate { };
        public event UnityAction<RaycastHit> Click = delegate { };
        public event UnityAction Esc = delegate { };

        public PlayerInputActions inputActions;

        public bool IsJumpKeyPressed() => inputActions.Player.Jump.IsPressed();


        
        public Vector2 Direction
        {
            get
            {if(_actionsDisabled) return Vector2.zero;
                return inputActions.Player.Move.ReadValue<Vector2>();
            }
        }

        public Vector2 LookDirection
        {
            get
            {
                if(_actionsDisabled) return Vector2.zero;
                return inputActions.Player.Look.ReadValue<Vector2>();
            }
        }

        public const float HoldDuration = 0.2f;
        private bool _actionsDisabled = false;

        public void EnablePlayerActions() {
            if (inputActions == null) {
                inputActions = new PlayerInputActions();
                inputActions.Player.SetCallbacks(this);
            }
            inputActions.Enable();
            _actionsDisabled = false;
        }
        public void DisablePlayerActions() {
            _actionsDisabled = true;
        }

        public void OnMove(InputAction.CallbackContext context) {
            if (_actionsDisabled) return;
            Move.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context) {
            if (_actionsDisabled) return;

            Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        bool IsDeviceMouse(InputAction.CallbackContext context) {
            
            // Debug.Log($"Device name: {context.control.device.name}");
            return context.control.device.name == "Mouse";
        }

        public void OnFire(InputAction.CallbackContext context) {
            if (_actionsDisabled) return;

                switch (context.phase) {
                    case InputActionPhase.Started:
                        Attack.Invoke(true);
                        break;
                    case InputActionPhase.Canceled:
                        Attack.Invoke(false);
                        break;
                } 
            
        }

        public void OnAltFire(InputAction.CallbackContext context)
        {            if (_actionsDisabled) return;

            switch (context.phase) {
                case InputActionPhase.Started:
                    AttackAlt.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    AttackAlt.Invoke(false);
                    break;
            }
        }

        public void OnClick(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Started) {
                if (IsDeviceMouse(context)) {
                    var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray.origin, ray.direction, out var hit, 100)) {
                        Click.Invoke(hit);
                    }
                }
            }
        }

        public void OnMouseControlCamera(InputAction.CallbackContext context) {
            if (_actionsDisabled) return;

            switch (context.phase) {
                case InputActionPhase.Started:
                    EnableMouseControlCamera.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    DisableMouseControlCamera.Invoke();
                    break;
            }
        }

        public void OnRun(InputAction.CallbackContext context) {
            if (_actionsDisabled) return;

            switch (context.phase) {
                case InputActionPhase.Started:
                    Dash.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Dash.Invoke(false);
                    break;
            }
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (_actionsDisabled) return;

            switch (context.phase) {
                case InputActionPhase.Started:
                    Jump.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Jump.Invoke(false);
                    break;
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (_actionsDisabled) return;

            switch (context.phase) {
                case InputActionPhase.Started:
                    Crouch.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Crouch.Invoke(false);
                    break;
            }
        }

        public void OnAction1(InputAction.CallbackContext context)
        {
            if (_actionsDisabled) return;

            switch (context.phase) {
                case InputActionPhase.Started:
                    Action1.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Action1.Invoke(false);
                    break;
            } 
        }

        public void OnAction2(InputAction.CallbackContext context)
        {
            if (_actionsDisabled) return;

            switch (context.phase) {
                case InputActionPhase.Started:
                    Action2.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Action2.Invoke(false);
                    break;
            }     }

        public void OnAction3(InputAction.CallbackContext context)
        {
            if (_actionsDisabled) return;

            switch (context.phase) {
                case InputActionPhase.Started:
                    Action3.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Action3.Invoke(false);
                    break;
            }     }

        public void OnAction4(InputAction.CallbackContext context)
        {
            if (_actionsDisabled) return;

            switch (context.phase) {
                case InputActionPhase.Started:
                    Action4.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Action4.Invoke(false);
                    break;
            }     }

        public void OnEsc(InputAction.CallbackContext context)
        {
           Esc?.Invoke();
        }
    }
}