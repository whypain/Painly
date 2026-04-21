using UnityEngine;

namespace Painly.Movement
{
    public class PlayerMovementState : MonoBehaviour
    {
        public float CurrentSpeed;
        public bool IsJumping;
        public bool IsFalling;
        public bool IsGrounded;
        public bool IsSprinting;
    }
}