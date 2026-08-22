using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States;
using ImprovedTimers;
using Project.Scripts.Generation;
using Project.Scripts.Infrastracture.ServiceLocator;
using Project.Scripts.Sounds;
using Scripts;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerEffects
{
    public class LampDarknessPulse : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private PlayerController _player;
        [SerializeField] private DarknessFollowing _darkness;
        [SerializeField] private ParticleSystem _burstParticles;
        [SerializeField] private ParticleSystem _readyParticles;
        [SerializeField] private Light _lampLight;
        [SerializeField] private string _lampSoundEvent = "Lamp";
        [SerializeField] private float _duration = 3f;
        [SerializeField] private float _cooldown = 5f;
        [SerializeField] private float _radiusMultiplier = 2.2f;
        [SerializeField] private float _fogDensityMultiplier = 0.2f;
        [SerializeField] private float _lightIntensityMultiplier = 3.5f;
        [SerializeField] private float _lightRangeMultiplier = 2f;
        [SerializeField] private AnimationCurve _pulseCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.12f, 1f),
            new Keyframe(1f, 0f));

        private CountdownTimer _cooldownTimer;
        private ISoundSystem _soundSystem;
        private float _restFogDensity;
        private float _restLightIntensity;
        private float _restLightRange;
        private float _elapsed;
        private bool _isPulsing;

        private void Awake()
        {
            if (_readyParticles == null)
            {
                var ready = Instantiate(_burstParticles, _burstParticles.transform.parent);
                ready.gameObject.name = "Ready";
                ready.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _readyParticles = ready;
            }

            _cooldownTimer = new CountdownTimer(_cooldown);
            _cooldownTimer.OnTimerStop += PlayReadyParticles;
        }

        private void Start()
        {
            _soundSystem = ServiceLocator.Global.Get<ISoundSystem>();
        }

        private void OnDestroy()
        {
            _cooldownTimer.OnTimerStop -= PlayReadyParticles;
            _cooldownTimer.Dispose();
        }

        private void OnEnable()
        {
            _restFogDensity = RenderSettings.fogDensity;
            if (_lampLight != null)
            {
                _restLightIntensity = _lampLight.intensity;
                _restLightRange = _lampLight.range;
            }

            _input.Attack += OnAttack;
        }

        private void OnDisable()
        {
            _input.Attack -= OnAttack;
            _isPulsing = false;
            if (_darkness)
                _darkness.SetPulseScale(1f);
            ApplyLocal(0f);
        }

        private void Update()
        {
            if (!_isPulsing)
                return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            Apply(_pulseCurve.Evaluate(t));

            if (t < 1f)
                return;

            _isPulsing = false;
            Apply(0f);
        }

        private bool CanBurst()
        {
            if (!_cooldownTimer.IsFinished)
                return false;

            return _player.IsGrounded() || _player.GetStateType() == typeof(WallRunningState);
        }

        private void OnAttack(bool pressed)
        {
            if (!pressed || !CanBurst())
                return;

            _cooldownTimer.Start();
            _elapsed = 0f;
            _isPulsing = true;
            PlayParticles(_burstParticles);
            _soundSystem.InvokeEvent(_lampSoundEvent);
            Apply(_pulseCurve.Evaluate(0f));
        }

        private void PlayReadyParticles()
        {
            PlayParticles(_readyParticles);
        }

        private static void PlayParticles(ParticleSystem particles)
        {
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.Play(true);
        }

        private void Apply(float envelope)
        {
            _darkness.SetPulseScale(Mathf.Lerp(1f, _radiusMultiplier, envelope));
            ApplyLocal(envelope);
        }

        private void ApplyLocal(float envelope)
        {
            RenderSettings.fogDensity = Mathf.Lerp(
                _restFogDensity,
                _restFogDensity * _fogDensityMultiplier,
                envelope);

            if (_lampLight == null)
                return;

            _lampLight.intensity = Mathf.Lerp(
                _restLightIntensity,
                _restLightIntensity * _lightIntensityMultiplier,
                envelope);
            _lampLight.range = Mathf.Lerp(
                _restLightRange,
                _restLightRange * _lightRangeMultiplier,
                envelope);
        }
    }
}
