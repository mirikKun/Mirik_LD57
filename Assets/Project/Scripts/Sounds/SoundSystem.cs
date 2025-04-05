using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Project.Scripts.Sounds.AudioConfigsBase;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.Scripts.Sounds
{
    public class SoundSystem : ISoundSystem
    {
        private readonly List<SoundSystemAsset> _loadedAssets = new();
        private readonly List<AudioSource> _playedAudioSources = new();
        private readonly GameObject _rootGameObject;

        public SoundSystem()
        {
            _rootGameObject = new GameObject(nameof(SoundSystem));
            Object.DontDestroyOnLoad(_rootGameObject);
        }

        public void LoadSoundAsset(SoundSystemAsset soundSystemAsset)
        {
            if (_loadedAssets.Contains(soundSystemAsset))
                return;
            _loadedAssets.Add(soundSystemAsset);
        }

        public void UnloadSoundAsset(SoundSystemAsset soundSystemAsset)
        {
            if (_loadedAssets.Contains(soundSystemAsset))
                return;
            StopAllAssetSounds(soundSystemAsset);
            _loadedAssets.Remove(soundSystemAsset);
        }

        public async void InvokeEvent(string eventName)
        {
            foreach (SoundSystemAsset soundSystemAsset in _loadedAssets)
            {
                foreach (SoundSystemEvent soundSystemEvent in soundSystemAsset.SoundSystemEvents.Where(soundEvent =>
                             soundEvent.EventName == eventName))
                {
                    foreach (ISoundSystemActionConfig actionConfig in soundSystemEvent.Actions)
                    {
                        SoundSystemActionContext soundSystemActionContext = new()
                        {
                            Parent = _rootGameObject.transform,
                            CurrentAsset = soundSystemAsset,
                            LoadedAssets = _loadedAssets,
                            AudioSources = _playedAudioSources
                        };

                        actionConfig.Invoke(soundSystemActionContext);

                        if (actionConfig.WaitType == SoundSystemActionWaitType.RunNextActionAfterComplete)
                            await UniTask.WaitUntil(() => actionConfig.IsCompleted);
                    }
                }
            }
        }

        private void StopAllAssetSounds(SoundSystemAsset soundSystemAsset)
        {
            foreach (AudioSource playedAudioSource in _playedAudioSources.ToList().Where(playedAudioSource =>
                         soundSystemAsset.ClipGroups.Any(clipGroup =>
                             clipGroup.Clips.Contains(playedAudioSource.clip))))
            {
                playedAudioSource.Stop();
                playedAudioSource.clip = null;
                _playedAudioSources.Remove(playedAudioSource);
                Object.Destroy(playedAudioSource.gameObject);
            }
        }
    }
}