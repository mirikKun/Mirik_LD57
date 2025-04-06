using System;
using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.PlayerStateMachine;
using Assets.Scripts.Player.PlayerStateMachine.States;
using Scripts;
using Scripts.Player.DescentContorller;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.Controller
{
    [RequireComponent(typeof(PlayerMover))]
    public class PlayerController : MonoBehaviour
    {
        #region Fields

        [SerializeField] private InputReader _input;
        [SerializeField] private PlayerStatesHolder _playerStatesHolder;
        [SerializeField] private PlayerEffects.PlayerEffects _playerEffects;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private PlayerRespawner _playerRespawner;
[SerializeField] private DescentController _descentController;
[SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private Transform _cameraViewTransform;
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private float _movementSpeed = 7f;
        [SerializeField] private float _airControlRate = 2f;

        [SerializeField] private float _airFriction = 0.5f;
        [SerializeField] private float _groundFriction = 100f;
        [SerializeField] private float _gravity = 30f;

        [SerializeField] private float _slideGravity = 5f;

        /////
        [SerializeField] private float _slopeLimit = 30f;
        [SerializeField] private bool _useLocalMomentum;


        private Transform _transform;
        private PlayerMover _mover;
        private CeilingDetector _ceilingDetector;
        private WallDetector _wallDetector;


        private StateMachine _stateMachine;

        private Vector3 _momentum;
        private Vector3 _savedVelocity;
        private Vector3 _savedMovementVelocity;

        public event Action<Vector3> OnJump = delegate { };
        public event Action<Vector3> OnLand = delegate { };

        #endregion

        public Transform Tr => _transform;
        public Transform CameraTrX => _cameraController.CameraTrX;
        public Transform CameraTrY => _cameraController.CameraTrY;

        public InputReader Input => _input;
        public PlayerEffects.PlayerEffects PlayerEffects => _playerEffects;
        public CameraController CameraController => _cameraController;
        public PlayerRespawner PlayerRespawner => _playerRespawner;
        public DescentController DescentController => _descentController;
        public PlayerInventory PlayerInventory => _playerInventory;
        public PlayerMover Mover => _mover;
        public WallDetector WallDetector => _wallDetector;
        public float Gravity => _gravity;
        public Vector3 GetGroundNormal() => _mover.GetGroundNormal();
        public float GroundFriction => _groundFriction;

        public float AirFriction => _airFriction;
        public float SlideGravity => _slideGravity;
        public float MovementSpeed => _movementSpeed;
        public float AirControlRate => _airControlRate;
        public bool HitCeiling() => _ceilingDetector != null && _ceilingDetector.HitCeiling();

        public bool HitWallToTheFront() => _wallDetector != null && _wallDetector.HitForwardWall();
        public bool HitSidewaysWall() => _wallDetector != null && _wallDetector.HitSidewaysWall();
        public bool HitForwardBottomWall() => _wallDetector != null && _wallDetector.HitForwardBottomWall();
        public Vector3 GetWallNormal() => _wallDetector.GetWallNormal();

        private void Awake()
        {
            _transform = transform;
            _mover = GetComponent<PlayerMover>();
            _ceilingDetector = GetComponent<CeilingDetector>();
            _wallDetector = GetComponent<WallDetector>();
            SetupStateMachine();
            _playerStatesHolder.OnStateConfigsChanged += SetupStateMachine;
            _input.EnablePlayerActions();

        }

        private void OnDestroy()
        {
            _stateMachine?.Dispose();

        }


        private void SetupStateMachine()
        {
            _stateMachine?.Dispose();
            _stateMachine = new StateMachine();
            List<StateConfiguration> configurations = _playerStatesHolder.GetStateConfigurations(this);
            StateMachineFactory factory = new StateMachineFactory(_stateMachine);
            factory.SetupStateMachine(configurations, typeof(GroundedState));
        }


        private void At(IState from, IState to, Func<bool> condition) =>
            _stateMachine.AddTransition(from, to, condition);

        private void Any<T>(IState to, Func<bool> condition) => _stateMachine.AddAnyTransition(to, condition);

        public bool IsRising() => VectorMath.GetDotProduct(GetMomentum(), _transform.up) > 0f;
        public bool IsFalling() => VectorMath.GetDotProduct(GetMomentum(), _transform.up) < 0f;

        public bool IsGroundTooSteep() =>
            !_mover.IsGrounded() || Vector3.Angle(_mover.GetGroundNormal(), _transform.up) > _slopeLimit;

        public void SetState<T>() where T : IState
        {
            if (_stateMachine.CurrentState is T) return;
            _stateMachine.SetState<T>();
        }

        public Type GetStateType()
        {
            return _stateMachine.CurrentState.GetType();
        }

        private void Update() => _stateMachine.Update();

        private void FixedUpdate()
        {
            _savedVelocity = Vector3.zero;
            _mover.CheckForGround();
            _stateMachine.FixedUpdate();

            _savedVelocity += _useLocalMomentum ? _transform.localToWorldMatrix * _momentum : _momentum;
            _mover.SetExtendSensorRange(IsGroundedState());
            _mover.SetVelocity(_savedVelocity);
            _savedMovementVelocity = CalculateMovementVelocity();


            if (_ceilingDetector != null) _ceilingDetector.Reset();
            if (_wallDetector != null) _wallDetector.Reset();
        }

        public void SetVelocity(Vector3 velocity)
        {
            _savedVelocity = velocity;
        }

        public void SetMomentum(Vector3 momentum)
        {
            _momentum = momentum;
            if (_useLocalMomentum) _momentum = _transform.worldToLocalMatrix * _momentum;
        }


        private bool IsGroundedState() => _stateMachine.CurrentState is GroundedState or SlopeSlidingState;

        public bool IsGrounded() => _mover.IsGrounded();
        public Vector3 GetVelocity() => _savedVelocity;
        public Vector3 GetMomentum() => _useLocalMomentum ? _transform.localToWorldMatrix * _momentum : _momentum;
        public Vector3 GetHorizontalMomentum() => GetMomentum() - GetVerticalMomentum();

        public Vector3 GetVerticalMomentum() => VectorMath.ExtractDotVector(GetMomentum(), Tr.up);
        public Vector3 GetMovementVelocity() => _savedMovementVelocity;
        public Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * _movementSpeed;

        public bool HaveStateInHistory<T>(int statesBack = 6)
        {
            int statesCount = _stateMachine.PreviousStates.Count;
            for (int i = 0; i < statesBack; i++)
            {
                if (statesCount - 1 - i < 0) return false;

                if (_stateMachine.PreviousStates[statesCount - 1 - i] is T) return true;
            }

            return false;
        }

        public bool HaveStateBeforeStateInHistory<T, TBefore>(int statesBack = 10)
        {
            int statesCount = _stateMachine.PreviousStates.Count;

            for (int i = 0; i < statesBack; i++)
            {
                if (statesCount - 1 - i < 0) return false;

                if (_stateMachine.PreviousStates[statesCount - 1 - i] is T) return true;
                if (_stateMachine.PreviousStates[statesCount - 1 - i] is TBefore) return false;
            }

            return false;
        }


        public Vector3 CalculateMovementDirection()
        {
            Vector3 direction = CameraTrX == null
                ? _transform.right * _input.Direction.x + _transform.forward * _input.Direction.y
                : Vector3.ProjectOnPlane(CameraTrX.right, _transform.up).normalized * _input.Direction.x +
                  Vector3.ProjectOnPlane(CameraTrX.forward, _transform.up).normalized * _input.Direction.y;

            return direction.magnitude > 1f ? direction.normalized : direction;
        }


        public void OnGroundContactLost()
        {
            if (_useLocalMomentum) _momentum = _transform.localToWorldMatrix * _momentum;

            Vector3 velocity = GetMovementVelocity();
            if (velocity.sqrMagnitude >= 0f && _momentum.sqrMagnitude > 0f)
            {
                Vector3 projectedMomentum = Vector3.Project(_momentum, velocity.normalized);
                float dot = VectorMath.GetDotProduct(projectedMomentum.normalized, velocity.normalized);

                if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f) velocity = Vector3.zero;
                else if (dot > 0f) velocity -= projectedMomentum;
            }

            _momentum += velocity;

            if (_useLocalMomentum) _momentum = _transform.worldToLocalMatrix * _momentum;
        }

        public void OnGroundContactRegained()
        {
            Vector3 collisionVelocity = _useLocalMomentum ? _transform.localToWorldMatrix * _momentum : _momentum;
            OnLand.Invoke(collisionVelocity);
        }

        public void OnFallStart()
        {
            var currentUpMomentum = VectorMath.ExtractDotVector(_momentum, _transform.up);
            _momentum = VectorMath.RemoveDotVector(_momentum, _transform.up);
        }


        public void StartCrouching(float height)
        {
            _mover.SetColliderHeight(height);
        }


        public void StopCrouching()
        {
            _mover.ResetColliderHeight();
        }
    }
}