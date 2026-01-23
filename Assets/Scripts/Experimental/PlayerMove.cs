using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Experimental
{
    public class PlayerInputController : NetworkBehaviour
    {
        [SerializeField] private PlayerInput _input;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private Transform _camera;
        [SerializeField] private Transform _weapon;
        [SerializeField] private PlayerShoot _shoot;
        [SerializeField] private Vector2 _cameraClamp;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private float _sensivity;
        [SerializeField] private float _speed;
        
        private readonly SyncVar<Color> _color = new();
        private InputAction _attackAction;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private float _rotationX;

        public override void OnStartClient()
        {
            _color.OnChange += SetColor;
            SetColorSCmd();
            
            if (!IsOwner)
            {
                enabled = false;
                _input.enabled = false;
                
                return;
            }
        }
        
        public void OnMove(InputValue val) => _moveInput = val.Get<Vector2>();

        public void OnLook(InputValue val) => _lookInput = val.Get<Vector2>() * _sensivity * Time.deltaTime;

        public void OnJump() => HandleJump();

        private void Awake()
        {
            //Cursor.lockState = CursorLockMode.Locked;
            _attackAction = _input.currentActionMap.FindAction("Attack");
        }

        [ServerRpc]
        private void SetColorSCmd()
        {
            _color.Value = Random.ColorHSV();
        }

        private void SetColor(Color prev, Color next, bool asServer)
        {
            _renderer.material.color = _color.Value;
        }

        private void Update()
        {
            HandleMovement();
            HandleJump();
            HandleSprint();
            HandleAttack();
        }

        private void HandleAttack()
        {
            if (_attackAction.WasPressedThisFrame() || _attackAction.IsPressed())
                _shoot.TryShot_Client();
        }

        private void LateUpdate()
        {
            HandleCamera();
        }

        private void HandleCamera()
        {
            _rotationX -= _lookInput.y;
            _rotationX = Mathf.Clamp(_rotationX, _cameraClamp.x, _cameraClamp.y);

            _camera.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            _weapon.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.Rotate(Vector3.up * _lookInput.x);
        }
        
        private void HandleMovement()
        {
            Vector3 moveDir = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            moveDir = moveDir.sqrMagnitude > Mathf.Epsilon ? moveDir.normalized : Vector3.zero;
            
            transform.position += moveDir * (_speed * Time.deltaTime);
        }

        private void HandleJump()
        {
            
        }

        private void HandleSprint()
        {
            
        }
    }
}