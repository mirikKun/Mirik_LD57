using System.Threading.Tasks;
using Project.Scripts.Sounds.AudioConfigsBase;

namespace Project.Scripts.Sounds
{
    public interface ISoundSystem
    {
        void LoadSoundAsset(SoundSystemAsset soundSystemAsset);
        
        void UnloadSoundAsset(SoundSystemAsset soundSystemAsset);
        
        void InvokeEvent(string eventName);
    }
}