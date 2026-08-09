using System.Collections.Generic;
using Project.Scripts.Infrastracture.GameLoop;
using Scripts.Player.DescentContorller;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class DarknessFollowing : MonoBehaviour, IGameStartable, IGameUpdatable
    {
        [SerializeField] private float _acceleration = 13f;
        [SerializeField] private float _deceleration = 1.3f;
        [SerializeField] private float _impulseSpeedChange = -20;
        [SerializeField] private Vector3 _offset;
        [SerializeField] private Vector3 _newLocationOffset;
        [SerializeField] private float _tunnelEntryDarknessDrop = 5f;
        [Space]
        [SerializeField] private DescentController _descentController;

        [SerializeField] private LocationsGenerator _locationsGenerator;
        [SerializeField] private AnimationCurve _speedCurve;
        [Space]
        [SerializeField] private Vector2 _darknessPlainsSize = new Vector2(10, 10);
        [SerializeField] private List<Transform> _availableDarknessPlains;
        [SerializeField] private Transform _darknessPlainsPrefab;
        [SerializeField] private List<Transform> _darknessPlainsPool = new List<Transform>();
        private float _currentSpeed;
        private bool _hasMaxY;
        private float _maxDarknessY;

        public void GameStart()
        {
            _descentController.Grounded += OnCharacterGrounded;
            _locationsGenerator.LocationEntered += OnLocationEntered;
        }

        private void OnDestroy()
        {
            _descentController.Grounded -= OnCharacterGrounded;
            _locationsGenerator.LocationEntered -= OnLocationEntered;
        }

        private void OnLocationEntered(Bounds locationBounds, Transform entryPlatform)
        {
            if (entryPlatform != null)
            {
                float targetY = entryPlatform.position.y - _tunnelEntryDarknessDrop;
                _maxDarknessY = _hasMaxY ? Mathf.Min(_maxDarknessY, targetY) : targetY;
                _hasMaxY = true;
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                _currentSpeed = 0f;
            }

            _darknessPlainsPool.AddRange(_availableDarknessPlains);
            _availableDarknessPlains.Clear();

            float plainWidth = _darknessPlainsSize.x;
            float plainDepth = _darknessPlainsSize.y;
            int countX = Mathf.Max(1, Mathf.CeilToInt(locationBounds.size.x / plainWidth)) + 2;
            int countZ = Mathf.Max(1, Mathf.CeilToInt(locationBounds.size.z / plainDepth)) + 2;
            float startX = locationBounds.min.x - plainWidth;
            float startZ = locationBounds.min.z - plainDepth;
            float y = transform.position.y;

            for (int i = 0; i < countX; i++)
            {
                for (int j = 0; j < countZ; j++)
                {
                    if (_darknessPlainsPool.Count == 0)
                        _darknessPlainsPool.Add(Instantiate(_darknessPlainsPrefab, transform));

                    Transform darknessPlain = _darknessPlainsPool[0];
                    _darknessPlainsPool.RemoveAt(0);
                    darknessPlain.position = new Vector3(startX + i * plainWidth, y, startZ + j * plainDepth);
                    darknessPlain.gameObject.SetActive(true);
                    _availableDarknessPlains.Add(darknessPlain);
                }
            }

            foreach (var darknessPlain in _darknessPlainsPool)
                darknessPlain.gameObject.SetActive(false);
        }

        public void GameUpdate()
        {
            Vector3 targetPosition = GetTargetPosition();

            int accelerationSign = (int)Mathf.Sign(targetPosition.y - transform.position.y);
            int speedSign = (int)Mathf.Sign(_currentSpeed);
            _currentSpeed += _acceleration * accelerationSign * Time.deltaTime;
            if (accelerationSign != speedSign)
                _currentSpeed /= _deceleration;

            float newY = transform.position.y + _currentSpeed * Time.deltaTime;
            if (_hasMaxY && newY > _maxDarknessY)
            {
                newY = _maxDarknessY;
                if (_currentSpeed > 0f)
                    _currentSpeed = 0f;
            }

            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        private void OnCharacterGrounded()
        {
            var targetPosition = GetTargetPosition();
            float speedOffset = _impulseSpeedChange * _speedCurve.Evaluate(Mathf.Abs(targetPosition.y - transform.position.y) / _offset.magnitude);

            _currentSpeed += speedOffset;
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 target;
            if (_locationsGenerator.TryGetNearestLocationEnterPoint(_descentController.LastGroundPosition, _offset.magnitude, out Vector3 locationEnter))
                target = locationEnter + _newLocationOffset;
            else
                target = new Vector3(0, _descentController.LastGroundPosition.y, 0) + _offset;

            if (_hasMaxY)
                target.y = Mathf.Min(target.y, _maxDarknessY);

            return target;
        }
    }
}
