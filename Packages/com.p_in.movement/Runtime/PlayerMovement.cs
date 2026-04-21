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

    [RequireComponent(typeof(Rigidbody), typeof(PlayerMovementState), typeof(PlayerStat))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float m_acceleration = 5f;
        [SerializeField] private float m_rotationSpeed = 10;
        
        [Space]
        [SerializeField] private MovementAxis m_movementAxis = MovementAxis.XZ;

        [Space] [Header("Jump Settings")] 
        [SerializeField] private float m_jumpForce = 5;
        [SerializeField] private float m_coyoteTime = 0.2f;
        [SerializeField] private float m_jumpDelay = 0.2f;
        [SerializeField] private float m_jumpCooldown = 1;
        [SerializeField] private float m_groundCheckDist = 1;
        [SerializeField] private LayerMask m_groundLayer;

        [Space] [Header("Input Settings")]
        [SerializeField] private InputActionReference m_moveActionRef;
        [SerializeField] private InputActionReference m_sprintActionRef;
        [SerializeField] private InputActionReference m_jumpActionRef;

        private Rigidbody m_rigidbody;
        private PlayerMovementState m_movementState;
        private PlayerStat m_playerStat;
        private JumpEngine m_jumpEngine;
        private Vector3 m_inputDir;

        public void SetWalkSpeed(float newSpeed)
        {
            m_playerStat.WalkSpeed = newSpeed;
        }
        
        public void SetSprintSpeed(float value)
        {
            m_playerStat.SprintSpeed = value;
        }

        private void OnEnable()
        {
            m_jumpEngine = new JumpEngine(transform, m_coyoteTime, m_groundLayer, m_groundCheckDist, m_jumpCooldown);

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

        private void OnSprint(InputAction.CallbackContext ctx) 
            => m_movementState.IsSprinting = ctx.performed;
        private void OnJump(InputAction.CallbackContext _) 
            => m_jumpEngine.RequestJump(m_rigidbody, m_jumpForce, m_jumpDelay);
        

        private void Update()
        {
            m_jumpEngine.Tick(Time.deltaTime);
            UpdateMovementState();
        }

        private void FixedUpdate()
        {
            HandleRotation();
            HandleMove();
        }

        private void HandleMove()
        {
            Vector3 localInputDir = transform.TransformDirection(m_inputDir);
            Debug.DrawRay(transform.position + Vector3.up * 2f, localInputDir, Color.green);
            Debug.DrawRay(transform.position + Vector3.up * 2f, transform.forward, Color.blue);

            if (m_inputDir == Vector3.zero) return;
            float maxSpeed = m_movementState.IsSprinting ? m_playerStat.SprintSpeed : m_playerStat.WalkSpeed;

            var lv = m_rigidbody.linearVelocity;
            lv += localInputDir * m_acceleration;
            lv = Vector3.ClampMagnitude(lv, maxSpeed);

            m_rigidbody.linearVelocity = lv;
        }

        private void HandleRotation()
        {
            Vector3 localInputDir = transform.TransformDirection(m_inputDir);
            Vector3 steer = (localInputDir + transform.forward).normalized;

            transform.forward = Vector3.Lerp(transform.forward, steer, m_rotationSpeed * Time.fixedDeltaTime);

            // Lock X and Z rotation to prevent tipping over
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }

        private void UpdateMovementState()
        {
            m_movementState.CurrentSpeed = m_rigidbody.linearVelocity.magnitude;
            m_movementState.IsGrounded = m_jumpEngine.IsGrounded;
            m_movementState.IsJumping = m_jumpEngine.IsJumping;
            m_movementState.IsFalling = !m_jumpEngine.IsGrounded && m_rigidbody.linearVelocity.y < 0;
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

        private void OnValidate()
        {
            m_rigidbody ??= GetComponent<Rigidbody>();
            m_movementState ??= GetComponent<PlayerMovementState>();
            m_playerStat ??= GetComponent<PlayerStat>();
        }
    }
}