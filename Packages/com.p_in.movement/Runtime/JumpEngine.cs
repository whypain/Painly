using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Painly.Movement
{
    [System.Serializable]
    public class JumpEngine
    {
        public event Action OnLanded;
        public bool IsGrounded => m_isGrounded;
        public bool IsJumping { get; private set; }

        private readonly Transform m_transform;
        private readonly float m_coyoteTime;
        private readonly float m_jumpCooldown;
        private readonly float m_groundCheckDist;
        private readonly LayerMask m_groundLayer;

        private float m_coyotePeriod;
        private float m_jumpCooldownPeriod;
        private bool m_isGrounded;
        private bool m_isInCoyotePeriod;
        private bool m_isInJumpCooldownPeriod;
        
        public bool CanJump => (m_isInCoyotePeriod || m_isGrounded) && !m_isInJumpCooldownPeriod;
        
        public JumpEngine(Transform transform, float coyoteTime, LayerMask groundLayer, float groundCheckDist, float jumpCooldown)
        {
            m_transform = transform;
            m_coyoteTime = coyoteTime;
            m_jumpCooldown = jumpCooldown;
            m_groundLayer = groundLayer;
            m_groundCheckDist = groundCheckDist;
        }

        public void Tick(float dt)
        {
            if (m_isInJumpCooldownPeriod && m_jumpCooldownPeriod > 0)
            {
                m_jumpCooldownPeriod -= dt;
            }
            else if (m_isInJumpCooldownPeriod && m_jumpCooldownPeriod <= 0)
            {
                m_isInJumpCooldownPeriod = false;
            }
            
            if (m_isInCoyotePeriod && m_coyotePeriod > 0)
            {
                m_coyotePeriod -= dt;
            }
            else if (m_isInCoyotePeriod && m_coyotePeriod <= 0)
            {
                m_isInCoyotePeriod = false;
            }

            bool isGroundedNow = RaycastGround();
            
            if (m_isGrounded && !isGroundedNow)
            {
                m_coyotePeriod = m_coyoteTime;
                m_isInCoyotePeriod = true;
                m_isGrounded = false;
                return;
            }
            if (!m_isGrounded && isGroundedNow)
            {
                OnLanded?.Invoke();
                IsJumping = false;
            }
            
            m_isGrounded = isGroundedNow;
        }

        public async void RequestJump(Rigidbody rb, float jumpForce, float jumpDelay)
        {
            if (CanJump)
            {
                IsJumping = true;
                await Task.Delay(TimeSpan.FromSeconds(jumpDelay));
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                m_isInJumpCooldownPeriod = true;
                m_jumpCooldownPeriod = m_jumpCooldown;
            }
        }

        private bool RaycastGround()
        {
            Ray ray = new Ray(m_transform.position, Vector3.down * m_groundCheckDist);
            Debug.DrawRay(ray.origin, ray.direction * m_groundCheckDist, Color.red);
            return Physics.Raycast(ray, m_groundCheckDist, m_groundLayer);
        }
    }
}