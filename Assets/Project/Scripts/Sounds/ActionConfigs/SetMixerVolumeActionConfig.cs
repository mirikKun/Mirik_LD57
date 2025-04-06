using System;
using System.Linq;
using DG.Tweening;
using Project.Scripts.Sounds.AudioConfigsBase;
using UnityEngine;

namespace Project.Scripts.Sounds.ActionConfigs
{
    [Serializable]
    public class SetMixerVolumeActionConfig : ISoundSystemActionConfig
    {
        [SerializeField]
        private string _mixerName;

        [SerializeField]
        private float _transitionTime;

        [SerializeField]
        [Range(-80, 20)]
        private float _volumeDB;

        [SerializeField]
        private SoundSystemActionWaitType _waitType;

        public SoundSystemActionWaitType WaitType => _waitType;
        public bool IsCompleted { get; private set; }

     
        
        public void Invoke(SoundSystemActionContext context)
        {
            IsCompleted = false;

            foreach (SoundSystemAsset soundSystemAsset in context.LoadedAssets)
            {
                foreach (AudioClipGroup clipGroup in soundSystemAsset.ClipGroups.Where(clipGroup => clipGroup.Group.name == _mixerName)
                             .Distinct())
                {
                    string paramName = $"{_mixerName}Volume";
                    clipGroup.Group.audioMixer.GetFloat(paramName, out float currentVolume);

                    DOTween.To(() => currentVolume, volume => clipGroup.Group.audioMixer.SetFloat(paramName, volume), _volumeDB,
                            _transitionTime)
                        .SetTarget(clipGroup)
                        .OnComplete(() => IsCompleted = true);
                    clipGroup.Group.audioMixer.SetFloat(paramName, _volumeDB);
                    IsCompleted = true;
                }
            }
        }
    }
}