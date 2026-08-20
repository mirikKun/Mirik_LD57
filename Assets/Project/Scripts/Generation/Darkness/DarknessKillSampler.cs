using Assets.Scripts.Player.Controller;
using Scripts.Player.Health;
using UnityEngine;

namespace Project.Scripts.Generation.Darkness
{
    public class DarknessKillSampler
    {
        private readonly DarknessClearAnchors _anchors;
        private readonly DarknessChaseState _chase;
        private readonly PlayerHealth _playerHealth;
        private readonly Transform _player;
        private readonly PlayerController _playerController;
        private readonly float _killThreshold;
        private readonly float _velocityLookAhead;
        private readonly int _damage;

        public DarknessKillSampler(
            DarknessClearAnchors anchors,
            DarknessChaseState chase,
            PlayerHealth playerHealth,
            Transform player,
            PlayerController playerController,
            float killThreshold,
            float velocityLookAhead,
            int damage)
        {
            _anchors = anchors;
            _chase = chase;
            _playerHealth = playerHealth;
            _player = player;
            _playerController = playerController;
            _killThreshold = killThreshold;
            _velocityLookAhead = velocityLookAhead;
            _damage = damage;
        }

        public void Tick()
        {
            Vector3 samplePos = _player.position;
            Vector3 velocity = _playerController.GetVelocity();
            if (velocity.sqrMagnitude > 0.01f)
                samplePos += velocity.normalized * _velocityLookAhead;

            if (_anchors.SampleDensity(samplePos, _chase) >= _killThreshold)
                _playerHealth.TakeDamage(_damage);
        }
    }
}
