using System;
using Assets.Scripts.Player.Controller;
using UnityEngine;

namespace Scripts.LevelObjects
{
    public class TutorialShower : MonoBehaviour
    {
        [SerializeField] private float _showDistance = 2f;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _canvasRoot;
        [SerializeField] private AnimationCurve _alphaCurve;
        private PlayerController _playerController;
        private bool _isShowing;

        private void Update()
        {
            if (_isShowing)
            {
                float distance = Vector3.Distance(_playerController.transform.position, transform.position);
                if (distance < _showDistance)
                {
                    float alpha = 1 - (distance / _showDistance);
                    _canvasGroup.alpha = _alphaCurve.Evaluate(alpha);
                    _canvasRoot.LookAt(_playerController.transform.position);
                }
                else
                {
                    _canvasGroup.alpha = 0;
                }
            }
            else
            {
                if (_canvasGroup.alpha > 0)
                {
                    _canvasGroup.alpha = 0;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out PlayerController playerController))
            {
                StartTutorialTracking(playerController);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out PlayerController playerController))
            {
                StopTutorialTracking();
            }
        }

        private void StartTutorialTracking(PlayerController playerController)
        {
            _playerController = playerController;
            _isShowing = true;
        }

        private void StopTutorialTracking()
        {
            _playerController = null;
            _isShowing = false;
        }
    }
}