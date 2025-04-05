using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Infrastracture
{
    public class SceneLoaderService:ISceneLoaderService
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public SceneLoaderService(ICoroutineRunner coroutineRunner) => 
            _coroutineRunner = coroutineRunner;

        public void Load(string name, Action onLoaded = null) =>
            _coroutineRunner.StartCoroutine(LoadScene(name, onLoaded));
        public static void InstantLoad(string name)
        {
            SceneManager.LoadScene(name);
        }
    
        public IEnumerator LoadScene(string nextScene, Action onLoaded = null)
        {
            if (SceneManager.GetActiveScene().name == nextScene)
            {
                onLoaded?.Invoke();
                yield break;
            }
      
            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene);

            while (!waitNextScene.isDone)
                yield return null;
      
            onLoaded?.Invoke();
        }
    }
}