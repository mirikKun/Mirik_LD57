using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Sounds.AudioConfigsBase
{
    public struct SoundSystemActionContext
    {
        public Transform Parent;
        public SoundSystemAsset CurrentAsset;
        public List<SoundSystemAsset> LoadedAssets;
        public List<AudioSource> AudioSources;
    }
}