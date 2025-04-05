using UnityEngine;
using UnityEngine.Audio;

namespace Project.Scripts.Sounds.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SoundMixersConfig", menuName = "ScriptableObjects/Sound/SoundMixersConfig")]

    public class SoundMixersSO:ScriptableObject
    {
        [field:SerializeField] public AudioMixerGroup MusicGroup { get; private set; }
        [field:SerializeField] public AudioMixerGroup SoundGroup { get; private set; }
        [field: SerializeField] public float MinimalVolume { get; private set; } = -80;
        [field: SerializeField] public float DefaultVolume { get; private set; } = 0;


    }
}