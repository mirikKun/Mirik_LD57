using UnityEngine;

namespace Assets.Scripts.Player.PlayerEffects
{
    public class PlayerEffects:MonoBehaviour
    {
        [SerializeField] private HookEffects _hookEffects;
        [SerializeField] private CameraMovingEffects _cameraMovingEffects;
        public HookEffects HookEffects => _hookEffects;
        public CameraMovingEffects CameraMovingEffects => _cameraMovingEffects;
    }
}