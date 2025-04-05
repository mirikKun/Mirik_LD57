using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Sounds.AudioConfigsBase
{
    [CreateAssetMenu(fileName = nameof(SoundSystemAsset), menuName = "SoundSystem/" + nameof(SoundSystemAsset), order = 0)]
    public class SoundSystemAsset : ScriptableObject
    {
        [SerializeField]
        private List<AudioClipGroup> _clipGroups;

        [SerializeField]
        private List<SoundSystemEvent> _soundSystemEvents = new()
        {
            new SoundSystemEvent()
        };

        public List<AudioClipGroup> ClipGroups => _clipGroups;

        public List<SoundSystemEvent> SoundSystemEvents => _soundSystemEvents;
    }
}