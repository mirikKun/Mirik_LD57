using System;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Player.DescentContorller;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class DarknessFollowing:MonoBehaviour
    {
        
        [SerializeField] private float _acceleration= 13f;
        [SerializeField] private float _deceleration= 1.3f;
        [SerializeField] private float _impulseSpeedChange=-20;
        [SerializeField] private Vector3 _offset;
        [Space] 
        [SerializeField] private DescentController _descentController;

        [SerializeField] private LocationsGenerator _locationsGenerator;
        [SerializeField] private AnimationCurve _speedCurve;
        [Space] 
        [SerializeField] private Vector2 _darknessPlainsSize=new Vector2(10,10);
        [SerializeField] private List<Transform> _availableDarknessPlains;
        [SerializeField] private Transform _darknessPlainsPrefab;
        [SerializeField]private List<Transform> _darknessPlainsPool=new List<Transform>();
        private float _currentSpeed;

        private void Start()
        {
            _descentController.Grounded += OnCharacterGrounded;
            _locationsGenerator.LocationEntered += OnLocationEntered;
        }

        private void OnDestroy()
        {
            _descentController.Grounded -= OnCharacterGrounded;
            _locationsGenerator.LocationEntered -= OnLocationEntered;

        }

        private void OnLocationEntered(Vector3 center, Vector3 size)
        {
            _darknessPlainsPool.AddRange(_availableDarknessPlains);
            _availableDarknessPlains.Clear();
            Vector2Int darknessPlainCount = new Vector2Int(Mathf.CeilToInt(size.x / _darknessPlainsSize.x), Mathf.CeilToInt(size.z / _darknessPlainsSize.y));
            darknessPlainCount+= new Vector2Int(2, 2);
            for (int i = 0; i < darknessPlainCount.x; i++)
            {
                for (int j = 0; j < darknessPlainCount.y; j++)
                {
                    if (_darknessPlainsPool.Count == 0)
                    {
                        _darknessPlainsPool.Add(Instantiate(_darknessPlainsPrefab, transform));
                    }
                    Transform darknessPlain = _darknessPlainsPool[0];

                    _darknessPlainsPool.RemoveAt(0);
                    darknessPlain.position = new Vector3(-transform.position.x+center.x + i * _darknessPlainsSize.x, transform.position.y,-transform.position.z+ center.z + j * _darknessPlainsSize.y);
                    darknessPlain.gameObject.SetActive(true);
                    _availableDarknessPlains.Add(darknessPlain);
                }
            }

            foreach (var darknessPlain in _darknessPlainsPool)
            {
                darknessPlain.gameObject.SetActive(false);
            }
        }

        public void Update()
        {
            Vector3 targetPosition = new Vector3(0,_descentController.GetCurrenMaxDepth(),0) + _offset;
            
            int accelerationSign= (int)Mathf.Sign(targetPosition.y - transform.position.y);
            int speedSign= (int)Mathf.Sign(_currentSpeed);
            _currentSpeed += _acceleration * accelerationSign * Time.deltaTime;
            if(accelerationSign!= speedSign)
            {
                _currentSpeed /=_deceleration;
            }
            
            transform.position=new Vector3(transform.position.x,transform.position.y+_currentSpeed*Time.deltaTime,transform.position.z);
    
        }

        private void OnCharacterGrounded()
        {
            Vector3 targetPosition = new Vector3(0,_descentController.GetCurrenMaxDepth(),0) + _offset;

            float speedOffset= _impulseSpeedChange*_speedCurve.Evaluate(Mathf.Abs(targetPosition.y - transform.position.y)/_descentController.GetDistance);
      
            _currentSpeed+= speedOffset;
        }
        
    }
}