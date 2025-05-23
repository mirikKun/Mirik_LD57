using System;
using Assets.Scripts.Player.PlayerStateMachine.States;
using Project.Scripts.Infrastracture.ServiceLocator;
using Project.Scripts.Sounds;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.Controller
{
    public class PlayerSoundsManager : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [Space] [SerializeField] private string _deathSoundEvent;
        [SerializeField] private string _stepSoundEvent;
        [SerializeField] private string _highSpeedSoundEvent;
        [Space] [SerializeField] private float _stepSoundRate;

        [SerializeField] private float _timeToJump = 0.4f;
        private ISoundSystem _soundSystem;
        private float _stepProgress;
        private bool _wasGrounded;
        private float _passedTimeToJump;
        private bool _disabled;

        private void Start()
        {
            _soundSystem = ServiceLocator.Global.Get<ISoundSystem>();

            _playerController.PlayerRespawner.Respawned += OnRespawned;
        }

        private void OnDestroy()
        {
            _playerController.PlayerRespawner.Respawned -= OnRespawned;
        }

        public void Disable()
        {
            _disabled = true;
        }
        private void Update()
        {if(_disabled)
            return;
            MakeStepSound();
        }

        private void MakeStepSound()
        {
            Vector3 velocity = _playerController.GetVelocity();
            Vector3 horizontalVelocity = velocity - VectorMath.ExtractDotVector(velocity, _playerController.Tr.up);

            if (!_playerController.Mover.IsGrounded())
            {
                _passedTimeToJump += Time.deltaTime;
            }

            if (!_wasGrounded && _playerController.Mover.IsGrounded())
            {
                if (_passedTimeToJump > _timeToJump)
                {
                    _soundSystem.InvokeEvent(_stepSoundEvent);
                    _soundSystem.InvokeEvent(_stepSoundEvent);
                }

                _stepProgress = 0;
                _passedTimeToJump = 0;
            }

            _wasGrounded = _playerController.Mover.IsGrounded();
            if (_playerController.GetStateType() == typeof(GroundedState) && horizontalVelocity.magnitude > 0.01f)
            {
                _stepProgress += horizontalVelocity.magnitude * Time.deltaTime;

                if (_stepProgress >= _stepSoundRate)
                {
                    _soundSystem.InvokeEvent(_stepSoundEvent);
                    _stepProgress -= _stepSoundRate;
                }
            }
        }

        private void OnRespawned()
        {
            _soundSystem.InvokeEvent(_deathSoundEvent);
        }
    }
}