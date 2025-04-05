using UnityEngine;

namespace Assets.Scripts.Player.Controller {
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimationController : MonoBehaviour {
        PlayerController controller;
        Animator animator;

        readonly int speedHash = Animator.StringToHash("Speed");
        readonly int isJumpingHash = Animator.StringToHash("IsJumping");

        void Start() {
            controller = GetComponent<PlayerController>();
            animator = GetComponentInChildren<Animator>();
            
            controller.OnJump += HandleJump;
            controller.OnLand += HandleLand;
        }

        void Update() {
            animator.SetFloat(speedHash, controller.GetMovementVelocity().magnitude);
        }

        void HandleJump(Vector3 momentum) => animator.SetBool(isJumpingHash, true);
        void HandleLand(Vector3 momentum) => animator.SetBool(isJumpingHash, false);
    }
}