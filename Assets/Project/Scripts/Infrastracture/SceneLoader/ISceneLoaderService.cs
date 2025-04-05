using System;
using System.Collections;

namespace Project.Scripts.Infrastracture
{
    public interface ISceneLoaderService
    {
        IEnumerator LoadScene(string nextScene, Action onLoaded = null);
        void Load(string name, Action onLoaded = null);
    }
}