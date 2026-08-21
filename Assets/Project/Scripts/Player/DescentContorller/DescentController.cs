using System;
using Assets.Scripts.Player.Controller;
using Scripts.Player.Health;
using UnityEngine;

namespace Scripts.Player.DescentContorller
{
    public class DescentController : MonoBehaviour
    {
        [SerializeField] private PlayerMover _playerMover;
        [Space]
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private float _minGroundedChange = 0.5f;



        private float _currentDepth;
        private float _maxDepth;
        private float _maxGroundDepth;
        private Vector3 _startPosition;
        private bool _inStaminaReplenishZone;
        private int _darknessFollowZoneCount;
        private Vector3 _lastGroundPosition;
        private bool _grounded;

        private Vector3 _lastPosition;
        public event Action<float, float> DepthChanged;
        public event Action Grounded; 
        public Vector3 LastGroundPosition => _lastGroundPosition;
        public bool InDarknessFollowZone => _darknessFollowZoneCount > 0;


        private void Start()
        {
            _currentDepth = 0;
            _maxDepth = 0;
            _lastPosition = transform.position;
            _startPosition = _lastPosition;
        }

        private void Update()
        {
            CheckGrounded();
            CheckCurrentDepth();
        }

        private void CheckGrounded()
        {
            if(!_grounded && _playerMover.IsGrounded()&&
               Mathf.Abs(_lastGroundPosition.y - transform.position.y) > _minGroundedChange)
            {
                Grounded?.Invoke();
            }
            if(_grounded||_inStaminaReplenishZone)
            {
                _lastGroundPosition = transform.position;
            }
            _grounded = _playerMover.IsGrounded();
        }

  
  
        public void SetInStaminaReplenishZone(bool inside)
        {
            _inStaminaReplenishZone = inside;
        }

        public void AddDarknessFollowZone() => _darknessFollowZoneCount++;

        public void RemoveDarknessFollowZone() => _darknessFollowZoneCount--;
   

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