using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Project.Scripts.Sounds.AudioConfigsBase
{
    [Serializable]
    public class AudioClipGroup
    {
        [SerializeField]
        private List<AudioClip> _clips;

        [SerializeField]
        private AudioMixerGroup _group;

        public List<AudioClip> Clips => _clips;

        public AudioMixerGroup Group => _group;
    }
}