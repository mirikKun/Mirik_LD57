using System;
using System.Linq;
using Project.Scripts.Sounds.Attributes;
using Project.Scripts.Sounds.AudioConfigsBase;
using UnityEngine;

namespace Project.Scripts.Sounds.ActionConfigs
{
    [Serializable]
    public class SetVolumeActionConfig : ISoundSystemActionConfig
    {
        [SerializeField]
        [ClipNameDropdown]
        private string _clipName;

        [SerializeField]
        private float _transitionTime;

        [SerializeField]
        [Range(0f, 1f)]
        private float _volume;

        [SerializeField]
        private SoundSystemActionWaitType _waitType;

        public SoundSystemActionWaitType WaitType => _waitType;
        public bool IsCompleted { get; private set; }

  
        public void Invoke(SoundSystemActionContext context)
        {
            IsCompleted = false;

            foreach (AudioSource audioSource in context.AudioSources.Where(audioSource => audioSource.clip.name == _clipName))
            {
                // DOTween.To(() => audioSource.volume, volume => audioSource.volume = volume, m_Volume, m_TransitionTime)
                //     .SetTarget(audioSource)
                //     .OnComplete(() => IsCompleted = true);
                audioSource.volume = _volume;
                IsCompleted = true;
                
            }
        }
    }
}