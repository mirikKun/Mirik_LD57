using System;
using System.Collections;
using UnityEngine;

namespace Scripts.UI.Animations
{
    public class FadeAnimation : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _fadeCurve;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 1f;
        
        


        public void FadeIn()
        {
         
            StartCoroutine(Fading(1, 0));
        }
       

        public void FadeOut()
        {
        
            StartCoroutine(Fading(0, 1));
        }
        public void InstantFadeIn()
        {
            _canvasGroup.alpha = 0;
        }

        private IEnumerator Fading(float from, float to)
        {
            float elapsedTime = 0f;
            _canvasGroup.alpha = from;

            while (elapsedTime < _fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / _fadeDuration);
                _canvasGroup.alpha = Mathf.Lerp(from, to, _fadeCurve.Evaluate(t));
                yield return null;
            }

            _canvasGroup.alpha = to;
        }
    }
}