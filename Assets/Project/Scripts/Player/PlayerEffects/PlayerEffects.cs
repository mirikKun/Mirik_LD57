using UnityEngine;

namespace Assets.Scripts.Player.PlayerEffects
{
    public class PlayerEffects:MonoBehaviour
    {
        [SerializeField] private HookEffects _hookEffects;
        [SerializeField] private CameraMovingEffects _cameraMovingEffects;
        [SerializeField] private ParticleSystem _landingParticles;
        [SerializeField] private float _landingParticlesMinHeight = 0.3f;

        public HookEffects HookEffects => _hookEffects;
        public CameraMovingEffects CameraMovingEffects => _cameraMovingEffects;

        public void OnLandedFromFall(float height)
        {
            _cameraMovingEffects.StartFallEffect(height);
            PlayLandingParticles(height);
        }

        private void PlayLandingParticles(float height)
        {
            if (_landingParticles == null || height < _landingParticlesMinHeight)
                return;

            _landingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _landingParticles.Play(true);
        }
    }
}