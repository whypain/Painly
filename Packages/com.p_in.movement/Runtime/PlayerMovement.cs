using UnityEngine;
using UnityEngine.InputSystem;

namespace Painly.Movement
{
    public enum MovementAxis
    {
        XY,
        XZ,
    }
    
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float m_speed = 10;
        [SerializeField] private float m_sprintMult = 0.2f;
        [SerializeField] private bool m_usePhysics = true;
        
        [Space]
        [SerializeField] private MovementAxis m_movementAxis = MovementAxis.XZ;
        [Space]

        [SerializeField] private InputActionReference m_moveActionRef;
        [SerializeField] private InputActionReference m_sprintActionRef;

        private Rigidbody m_rigidbody;
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
        }

        private void OnEnable()
        {
            InputAction move = m_moveActionRef.action;
            InputAction sprint = m_sprintActionRef.action;
            
            move.Enable();
            sprint.Enable();
            
            move.performed += OnMove;
            move.canceled  += OnMove;
            
            sprint.performed += OnSprint;
            sprint.canceled  += OnSprint;
        }

        private void OnDisable()
        {
            InputAction move = m_moveActionRef.action;
            InputAction sprint = m_sprintActionRef.action;

            move.Disable();
            sprint.Disable();
            
            move.performed -= OnMove;
            move.canceled  -= OnMove;
            
            sprint.performed -= OnSprint;
            sprint.canceled  -= OnSprint;
        }

        private void FixedUpdate()
        {
            if (m_inputDir != Vector3.zero) HandleMove();
        }

        private void OnSprint(InputAction.CallbackContext ctx)
        {
            if (ctx.canceled) m_currSpeed = m_speed;
            else if (ctx.performed) m_currSpeed = m_speed * m_sprintMult;
        }

        private void HandleMove()
        {
            if (m_usePhysics)
            {
                m_rigidbody.linearVelocity += m_inputDir * (m_currSpeed * Time.fixedDeltaTime);
                m_rigidbody.linearVelocity = Vector3.ClampMagnitude(m_rigidbody.linearVelocity, m_currSpeed);
            }
            else
            {
                transform.position += Vector3.ClampMagnitude(m_inputDir * (m_currSpeed * Time.fixedDeltaTime), m_currSpeed);
            }
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
            }
        }

        private void OnValidate()
        {
            m_currSpeed = m_speed;
            if (m_usePhysics)
            {
                if (!TryGetComponent(out m_rigidbody))
                {
                    m_rigidbody = gameObject.AddComponent<Rigidbody>();
                }
            }
        }
    }
}