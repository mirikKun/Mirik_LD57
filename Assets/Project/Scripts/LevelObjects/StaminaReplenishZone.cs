using System;
using Scripts.Player.DescentContorller;
using UnityEngine;

namespace Scripts.LevelObjects
{
    public class StaminaReplenishZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<DescentController>(out var descentController))
            {
                descentController.SetInStaminaReplenishZone(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<DescentController>(out var descentController))
            {
                descentController.SetInStaminaReplenishZone(false);
            }
        }
    }
}