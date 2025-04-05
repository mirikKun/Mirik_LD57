using System;
using System.Collections.Generic;
using Project.Scripts.Sounds.Attributes;
using UnityEngine;

namespace Project.Scripts.Sounds.AudioConfigsBase
{
    [Serializable]
    public class SoundSystemEvent
    {
        [SerializeField]
        private string _eventName;

        [SerializeReference]
        [ActionList]
        private List<ISoundSystemActionConfig> _actions = new();

        public string EventName => _eventName;

        public List<ISoundSystemActionConfig> Actions => _actions;
    }
}