using System;
using Assets.Scripts.Player.Controller;
using Scripts.Player.Health;
using UnityEngine;

namespace Scripts.Player.DescentContorller
{
    public class DescentController : MonoBehaviour
    {
        [SerializeField] private PlayerMover _playerMover;
        [SerializeField] private float _maxDepthStamina = 30;
        [SerializeField] private float _depthStaminaDecreaseRate = 1;
        [SerializeField] private float _depthStaminaOnGroundRegenSpeed = 10;
        [Space]
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private int _staminaEmptyDamage = 10;


        private float _currentDepthStamina;

        private float _currentDepth;
        private float _maxDepth;
        private float _maxGroundDepth;
        private Vector3 _startPosition;
        private bool _inStaminaReplenishZone;

        private Vector3 _lastPosition;
        public event Action<float, float> DepthChanged;
        public event Action<float> DepthStaminaChanged;


        private void Start()
        {
            _currentDepth = 0;
            _maxDepth = 0;
            _lastPosition = transform.position;
            _startPosition = _lastPosition;
            _currentDepthStamina = _maxDepthStamina;
        }

        private void Update()
        {
            CheckOneTimeDepth();
            CheckCurrentDepth();
        }
        public void ReplenishFullStamina()
        {
            _currentDepthStamina = _maxDepthStamina;
            DepthStaminaChanged?.Invoke(_currentDepthStamina/_maxDepthStamina);
        }

        public void SetInStaminaReplenishZone(bool inside)
        {
            _inStaminaReplenishZone = inside;
        }
        private void CheckOneTimeDepth()
        {
            if(_playerMover.IsGrounded()||_inStaminaReplenishZone)
            {
                if (_currentDepthStamina < _maxDepthStamina)
                {
                    _currentDepthStamina += _depthStaminaOnGroundRegenSpeed * Time.deltaTime;
                    _currentDepthStamina = Mathf.Clamp(_currentDepthStamina, 0, _maxDepthStamina);
                    DepthStaminaChanged?.Invoke(_currentDepthStamina/_maxDepthStamina);
                }
                if (_maxGroundDepth < _startPosition.y - transform.position.y)
                {
                    _maxGroundDepth = _startPosition.y - transform.position.y;
                }
            }
            else
            {
                if (_currentDepthStamina > 0)
                {
                    float depthDiff = _startPosition.y - transform.position.y-_currentDepth;
                    // if (_currentDepth < _maxGroundDepth)
                    // {
                    //     depthDiff = 0;
                    // }
                    
                    float staminaChange = _depthStaminaDecreaseRate * depthDiff;
                    _currentDepthStamina -= staminaChange;
                    _currentDepthStamina = Mathf.Clamp(_currentDepthStamina, 0, _maxDepthStamina);
                    DepthStaminaChanged?.Invoke(_currentDepthStamina/_maxDepthStamina);
                }
                else
                {
                    _playerHealth.TakeDamage(_staminaEmptyDamage );
                    ReplenishFullStamina();
                }
            }
        }

        private void CheckCurrentDepth()
        {
            if (_lastPosition.y != transform.position.y)
            {
                _lastPosition = transform.position;
                _currentDepth = _startPosition.y - _lastPosition.y;
                if (_currentDepth > _maxDepth)
                {
                    _maxDepth = _currentDepth;
                }

                DepthChanged?.Invoke(_currentDepth, _maxDepth);
            }
        }
    }
}