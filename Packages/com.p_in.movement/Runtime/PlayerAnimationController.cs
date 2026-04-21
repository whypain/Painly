using UnityEngine;

namespace Painly.Movement
{
    [RequireComponent(typeof(PlayerMovementState), typeof(PlayerStat))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator m_animator;
        [SerializeField] private string m_moveAnimParam = "MovingSpeed";
        [SerializeField] private string m_jumpAnimParam = "isJumping";
        [SerializeField] private string m_fallAnimParam = "isFalling";

        private PlayerMovementState m_state;
        private PlayerStat m_stat;

        private void Awake()
        {
            m_state = GetComponent<PlayerMovementState>();
            m_stat = GetComponent<PlayerStat>();
        }

        private void Update()
        {
            float maxSpeed = m_state.IsSprinting ? m_stat.SprintSpeed : m_stat.WalkSpeed;
            m_animator.SetFloat(m_moveAnimParam, m_state.CurrentSpeed / maxSpeed);
            m_animator.SetBool(m_fallAnimParam, m_state.IsFalling);
            m_animator.SetBool(m_jumpAnimParam, m_state.IsJumping);
        }
    }
}