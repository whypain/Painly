using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Painly.Movement
{
    public enum MovementAxis
    {
        X,
        XY,
        XZ,
    }

    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float m_speed = 10;
        [SerializeField] private float m_sprintMult = 0.2f;
        
        [Space]
        [SerializeField] private MovementAxis m_movementAxis = MovementAxis.XZ;

        [Space] [Header("Jump Settings")] 
        [SerializeField] private float m_jumpForce;
        [SerializeField] private float m_coyoteTime;
        [SerializeField] private float m_groundCheckDist;
        [SerializeField] private LayerMask m_groundLayer;

        [Space] [Header("Input Settings")]
        [SerializeField] private InputActionReference m_moveActionRef;
        [SerializeField] private InputActionReference m_sprintActionRef;
        [SerializeField] private InputActionReference m_jumpActionRef;

        private Rigidbody m_rigidbody;
        private JumpEngine m_jumpEngine;
        private Vector3 m_inputDir;
        private float m_currSpeed;

        public void SetSpeed(float newSpeed)
        {
            m_speed = newSpeed;
            m_currSpeed = m_speed;
            if (m_sprintActionRef.action.IsPressed())
            {
                m_currSpeed = m_speed * m_sprintMult;
            }
        }
        
        /// <summary>
        /// set sprint multiplier
        /// </summary>
        /// <param name="value"></param>
        public void SetSprintMult(float value)
        {
            m_sprintMult = value;
        }
        
        private void Awake()
        {
            m_currSpeed = m_speed;
            m_jumpEngine = new JumpEngine(transform, m_coyoteTime, m_groundLayer, m_groundCheckDist);
        }

        private void OnEnable()
        {
            InputAction move = m_moveActionRef.action;
            InputAction sprint = m_sprintActionRef.action;
            InputAction jump = m_jumpActionRef.action;
            
            move.Enable();
            sprint.Enable();
            jump.Enable();
            
            move.performed += OnMove;
            move.canceled  += OnMove;
            
            sprint.performed += OnSprint;
            sprint.canceled  += OnSprint;

            jump.performed += OnJump;
        }

        private void OnDisable()
        {
            InputAction move = m_moveActionRef.action;
            InputAction sprint = m_sprintActionRef.action;
            InputAction jump = m_jumpActionRef.action;

            move.Disable();
            sprint.Disable();
            jump.Disable();
            
            move.performed -= OnMove;
            move.canceled  -= OnMove;
            
            sprint.performed -= OnSprint;
            sprint.canceled  -= OnSprint;
            
            jump.performed -= OnJump;
        }

        private void FixedUpdate()
        {
            m_jumpEngine.Tick(Time.fixedDeltaTime);
            if (m_inputDir != Vector3.zero) HandleMove();
        }

        private void OnSprint(InputAction.CallbackContext ctx)
        {
            if (ctx.canceled) m_currSpeed = m_speed;
            else if (ctx.performed) m_currSpeed = m_speed * m_sprintMult;
        }

        private void OnJump(InputAction.CallbackContext _)
        {
            Action jumpHandler = m_movementAxis switch
            {
                MovementAxis.X => JumpInYAxis,
                MovementAxis.XZ => JumpInYAxis,
                _ => () => { }
            };
            
            if (m_jumpEngine.CanJump)
            {
                jumpHandler();
            }
        }

        private void HandleMove()
        {
            m_rigidbody.linearVelocity += m_inputDir * (m_currSpeed * Time.fixedDeltaTime);
            m_rigidbody.linearVelocity = Vector3.ClampMagnitude(m_rigidbody.linearVelocity, m_currSpeed);
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            Vector2 input = ctx.ReadValue<Vector2>();

            switch (m_movementAxis)
            {
                case MovementAxis.XY:
                    m_inputDir = new Vector3(input.x, input.y, 0).normalized;
                    break;
                case MovementAxis.XZ:
                    m_inputDir = new Vector3(input.x, 0, input.y).normalized;
                    break;
                case MovementAxis.X:
                    m_inputDir = new Vector3(input.x, 0, 0).normalized;
                    break;
            }
        }

        private void JumpInYAxis()
        {
            m_rigidbody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }

        private void OnValidate()
        {
            m_currSpeed = m_speed;
            if (!TryGetComponent(out m_rigidbody))
            {
                m_rigidbody = gameObject.AddComponent<Rigidbody>();
            }
        }
    }
}