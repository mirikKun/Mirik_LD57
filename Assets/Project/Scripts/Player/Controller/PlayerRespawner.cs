using System;
using Assets.Scripts.Player.PlayerStateMachine.States;
using UnityEngine;

namespace Assets.Scripts.Player.Controller
{
    public class PlayerRespawner:MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        private Vector3 _respawnPosition;
        public event Action Respawned;
        public void SetRespawnPosition(Vector3 position)
        {
            _respawnPosition = position;
        }
        public void Respawn()
        {
            transform.position = _respawnPosition;
            _playerController.SetMomentum(Vector3.zero);
            _playerController.SetVelocity(Vector3.zero);
            _playerController.SetState<GroundedState>();
            _playerController.PlayerInventory.ResetTempAbilities();
            Respawned?.Invoke();
        }
    }
}