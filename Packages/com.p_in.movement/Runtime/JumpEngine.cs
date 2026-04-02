using UnityEngine;

namespace Painly.Movement
{
    public class JumpEngine
    {
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
            m_jumpCooldown = groundCheckDist;
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
            
            m_isGrounded = isGroundedNow;
        }

        public void OnJump()
        {
            m_isInJumpCooldownPeriod = true;
            m_jumpCooldownPeriod = m_jumpCooldown;
        }

        private bool RaycastGround()
        {
            return Physics.Raycast(
                origin: m_transform.position, 
                direction: Vector3.down, 
                m_groundCheckDist,
                m_groundLayer);
        }
    }
}