using System;
using System.Collections.Generic;
using Project.Scripts.Infrastracture.ServiceLocator;
using Project.Scripts.Sounds;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.LevelObjects
{
    public class LevelMusicController : MonoBehaviour
    {
        [SerializeField] private List<string> _musicEvents;
        private ISoundSystem _soundSystem;
        private int _currentTrack;

        private void Start()
        {
            _soundSystem = ServiceLocator.Global.Get<ISoundSystem>();
            _currentTrack = Random.Range(0, _musicEvents.Count);
            string chosenEvent = _musicEvents[_currentTrack];
            _soundSystem.InvokeEvent(chosenEvent);
        }
    }
}