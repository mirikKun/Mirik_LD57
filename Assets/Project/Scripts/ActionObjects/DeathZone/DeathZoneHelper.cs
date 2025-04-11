using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.ActionObjects
{
    [ExecuteInEditMode]
    public class DeathZoneHelper: MonoBehaviour
    {

        [SerializeField] private Transform _darknessPlanePrefab;
        [SerializeField] private Vector2 _darknessPlaneSize= new Vector2(10, 10);
        [SerializeField] private Vector2 _size;
        [SerializeField] private BoxCollider _triggerCollider;
        [SerializeField] private Transform _darknessParent;
        [SerializeField] private bool _update;
        private Vector2Int _darknessPlaneCount;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(_darknessPlanePrefab == null||!_update)
            {
                return;
            }
            _triggerCollider.size = new Vector3(_size.x, _triggerCollider.size.y, _size.y);
            int darknessPlaneX = Mathf.CeilToInt(_size.x / _darknessPlaneSize.x);
            int darknessPlaneY = Mathf.CeilToInt(_size.y / _darknessPlaneSize.y);
           
            _darknessPlaneCount = new Vector2Int(darknessPlaneX, darknessPlaneY);
            //CreateDarknessPlanes();
            
        }

   
#endif
       
        [ContextMenu("Recreate")]
        private void CreateDarknessPlanes()
        {

            for (var i = _darknessParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_darknessParent.GetChild(i).gameObject);
            }

            for (int i = 0; i < _darknessPlaneCount.x; i++)
            {
                for (int j = 0; j < _darknessPlaneCount.y; j++)
                {
                    var darknessPlane = Instantiate(_darknessPlanePrefab, _darknessParent);
                    darknessPlane.localPosition = new Vector3(i * _darknessPlaneSize.x, 0, j * _darknessPlaneSize.y)-
                                                  new Vector3((_size.x-_darknessPlaneSize.x) / 2, 0,
                                                      (_size.y - _darknessPlaneSize.y) / 2);
                }
            }
        }
    }
}