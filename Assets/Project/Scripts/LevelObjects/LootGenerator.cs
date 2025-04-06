using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.LevelObjects
{
    public class LootGenerator:MonoBehaviour
    {
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private List<GameObject> _lootPrefabs;
        

        private void Start()
        {
            int randomIndex = Random.Range(0, _lootPrefabs.Count);
            if(_lootPrefabs[randomIndex]!=null)
            Instantiate(_lootPrefabs[randomIndex], _spawnPoint.position, Quaternion.Euler(0, Random.Range(0, 360), 0),
                transform.parent);
            Destroy(gameObject);
        }
    }
}