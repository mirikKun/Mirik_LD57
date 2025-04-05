using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Sounds.Attributes;
using Project.Scripts.Sounds.AudioConfigsBase;
using Project.Scripts.Sounds.Utils;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Project.Scripts.Sounds.ActionConfigs
{
     [Serializable]
    public class PlayActionConfig : ISoundSystemActionConfig
    {
        [SerializeField]
        [ClipNameDropdown]
        private string _clipName;

        [SerializeField]
        private bool _loop;

        [SerializeField]
        [Range(0f, 1f)]
        private float _volume = 1f;

        [SerializeField]
        private SoundSystemActionWaitType _waitType;

        public SoundSystemActionWaitType WaitType => _waitType;

        public bool IsCompleted { get; private set; }

   
        public async void Invoke(SoundSystemActionContext context)
        {
            IsCompleted = false;
            GameObject soundGameObject = new($"{_clipName}");
            soundGameObject.transform.parent = context.Parent;
            AudioSource source = soundGameObject.AddComponent<AudioSource>();
            AudioClip sourceClip = GetAudioClip(context.CurrentAsset) ?? GetAudioClip(FindSoundSystemAsset(context.LoadedAssets));

            if (sourceClip == null)
            {
                Object.Destroy(soundGameObject);
                IsCompleted = true;
                return;
            }

            context.AudioSources.Add(source);
            AudioMixerGroup mixerGroup = GetAudioMixer(context.CurrentAsset) ?? GetAudioMixer(FindSoundSystemAsset(context.LoadedAssets));
            soundGameObject.name += $" ({mixerGroup.name})";
            source.outputAudioMixerGroup = mixerGroup;
            source.playOnAwake = false;
            source.loop = _loop;
            source.volume = _volume;
            source.clip = sourceClip;

            source.Play();

            if (_loop)
                return;

            //await Awaiters.Seconds(sourceClip.length);
            IsCompleted = true;

            if (soundGameObject == null)
                return;

            context.AudioSources.Remove(source);
            Object.Destroy(soundGameObject);
        }

        private SoundSystemAsset FindSoundSystemAsset(List<SoundSystemAsset> loadedAssets)
        {
            return (from systemAsset in loadedAssets
                let mixerGroup = GetAudioMixer(systemAsset)
                where mixerGroup != null
                select systemAsset).FirstOrDefault();
        }

        private AudioClip GetAudioClip(SoundSystemAsset asset)
        {
            return asset.ClipGroups.Select(assetClipGroup => assetClipGroup.Clips.FirstOrDefault(c => c.name == _clipName))
                .FirstOrDefault(clip => clip != null);
        }

        private AudioMixerGroup GetAudioMixer(SoundSystemAsset asset)
        {
            return asset.ClipGroups.FirstOrDefault(audioClipGroup => audioClipGroup.Clips.Any(c => c.name == _clipName))?.Group;
        }
    }
}