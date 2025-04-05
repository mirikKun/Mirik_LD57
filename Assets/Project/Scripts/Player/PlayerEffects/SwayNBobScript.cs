using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Player.Controller;
using Scripts;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerEffects

{
    public class SwayNBobScript : MonoBehaviour
    {
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private InputReader _input;

        
        
        [Header("Sway")] [SerializeField] private float _step = 0.01f;
        [SerializeField] private float _maxStepDistance = 0.06f;

        [Header("Sway Rotation")] [SerializeField]
        private float _rotationStep = 4f;

        [SerializeField] private float _maxRotationStep = 5f;

        [SerializeField] private float _smooth = 10f;

        [Header("Bobbing")] [SerializeField] private float _speedCurve;


        [SerializeField] private Vector3 _travelLimit = Vector3.one * 0.025f;
        [SerializeField] private Vector3 _bobLimit = Vector3.one * 0.01f;

        [SerializeField] private float _bobExaggeration;

        [Header("Bob Rotation")] [SerializeField]
        private Vector3 _multiplier;


        private Vector3 _swayPos;

        //
        private Vector3 _swayEulerRot;
        private float _smoothRot = 12f;

        //

        private Vector3 _bobPosition;

        //
        private Vector3 _bobEulerRotation;

        private Vector2 _walkInput;
        private Vector2 _lookInput;

        private float CurveSin
        {
            get => Mathf.Sin(_speedCurve);
        }

        private float CurveCos
        {
            get => Mathf.Cos(_speedCurve);
        }


        void LateUpdate()
        {
            GetInput();

            Sway();
            SwayRotation();
            BobOffset();
            BobRotation();

            CompositePositionRotation();
        }


        void GetInput()
        {
            _walkInput = _input.Direction;
            _lookInput = _input.LookDirection;
        }


        void Sway()
        {
            Vector3 invertLook = _lookInput * -_step;
            invertLook.x = Mathf.Clamp(invertLook.x, -_maxStepDistance, _maxStepDistance);
            invertLook.y = Mathf.Clamp(invertLook.y, -_maxStepDistance, _maxStepDistance);

            _swayPos = invertLook;
        }

        void SwayRotation()
        {
            Vector2 invertLook = _lookInput * -_rotationStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -_maxRotationStep, _maxRotationStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -_maxRotationStep, _maxRotationStep);
            _swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
        }

        void CompositePositionRotation()
        {
            _targetTransform.localPosition = Vector3.Lerp(_targetTransform.localPosition, _swayPos + _bobPosition,
                Time.deltaTime * _smooth);
            _targetTransform.localRotation = Quaternion.Slerp(_targetTransform.localRotation,
                Quaternion.Euler(_swayEulerRot) * Quaternion.Euler(_bobEulerRotation), Time.deltaTime * _smoothRot);
        }

        void BobOffset()
        {
            _speedCurve += Time.deltaTime * (_mover.IsGrounded()
                ? (_walkInput.x + _walkInput.y) * _bobExaggeration
                : 1f) + 0.01f;

            _bobPosition.x = (CurveCos * _bobLimit.x * (_mover.IsGrounded() ? 1 : 0)) - (_walkInput.x * _travelLimit.x);
            _bobPosition.y = (CurveSin * _bobLimit.y) - (_walkInput.y * _travelLimit.y);
            _bobPosition.z = -(_walkInput.y * _travelLimit.z);
        }

        void BobRotation()
        {
            _bobEulerRotation.x = (_walkInput != Vector2.zero
                ? _multiplier.x * (Mathf.Sin(2 * _speedCurve))
                : _multiplier.x * (Mathf.Sin(2 * _speedCurve) / 2));
            _bobEulerRotation.y = (_walkInput != Vector2.zero ? _multiplier.y * CurveCos : 0);
            _bobEulerRotation.z = (_walkInput != Vector2.zero ? _multiplier.z * CurveCos * _walkInput.x : 0);
        }
    }
}