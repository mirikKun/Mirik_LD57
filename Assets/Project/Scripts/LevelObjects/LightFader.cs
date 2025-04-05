using System.Collections;
using UnityEngine;

namespace Scripts.LevelObjects
{
    public class LightFader:MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 1f;
        [SerializeField] private Light _light;
        private float _startIntensity;
        public void FadeIn()
        {
            _startIntensity = _light.intensity;
            StartCoroutine(FadeLight());
        }

        private IEnumerator FadeLight()
        {
           _startIntensity = _light.intensity;
            float targetIntensity = 0;
            float elapsedTime = 0f;

            while (elapsedTime < _fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                _light.intensity = Mathf.Lerp(_startIntensity, targetIntensity, elapsedTime / _fadeDuration);
                yield return null;
            }

            _light.intensity = targetIntensity;
        }
    }
}