using Project.Scripts.Sounds.AudioConfigsBase;

namespace Project.Scripts.Sounds
{
    public interface ISoundSystemActionConfig
    {
        SoundSystemActionWaitType WaitType { get; }
        bool IsCompleted { get; }
        void Invoke(SoundSystemActionContext context);
    }
}