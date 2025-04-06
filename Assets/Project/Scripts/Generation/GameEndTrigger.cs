using System;
using Assets.Scripts.Player.Controller;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class GameEndTrigger:MonoBehaviour
    {
        public static event Action OnGameEnded;
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerController>(out var playerController))
            {
                playerController.PlayerSoundsManager.Disable();
                OnGameEnded?.Invoke();
            }
        }
    }
}