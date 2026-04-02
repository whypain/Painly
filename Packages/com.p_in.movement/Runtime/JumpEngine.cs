using UnityEngine;

namespace Painly.Movement
{
    public class JumpEngine
    {
        private readonly Transform m_transform;
        private readonly float m_coyoteTime;
        private readonly float m_groundCheckDist;
        private readonly LayerMask m_groundLayer;

        private float m_coyotePeriod;
        private bool m_isGrounded;
        private bool m_isInCoyotePeriod;
        
        public bool CanJump => m_isInCoyotePeriod || m_isGrounded;
        
        public JumpEngine(Transform transform, float coyoteTime, LayerMask groundLayer, float groundCheckDist)
        {
            m_transform = transform;
            m_coyoteTime = coyoteTime;
            m_groundLayer = groundLayer;
            m_groundCheckDist = groundCheckDist;
        }

        public void Tick(float dt)
        {
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