using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public class PlayerCharacter : NetworkBehaviour
    {
        [SerializeField] private PlayerInput _input;
        [SerializeField] private float _speed;

        public readonly SyncVar<int> CurrentWeaponId = new SyncVar<int>();
        
        private PlayerInputListener _inputListener;
        private PlayerMoveController _move;

        public float Speed => _speed;

        public override void OnStartClient()
        {
            Init();
        }

        public override void OnStopClient()
        {
            
        }

        private void Init()
        {
            if (!IsOwner)
                return;
            
            _inputListener = new PlayerInputListener(_input);
            _move = new PlayerMoveController(_inputListener, this);
        }
    }
    
    public class PlayerInputListener : IDisposable
    {
        private readonly InputAction _moveAction;
        private readonly InputAction _attackAction;

        public Vector2 Move => _moveAction.ReadValue<Vector2>();

        public event Action OnAttackPerformed;
        public event Action OnJumpPerformed;
        public event Action OnWeaponReloadPerformed;
        public event Action<int> OnWeaponChangePerformed;
        
        public PlayerInputListener(PlayerInput input)
        {
            _moveAction = input.actions.FindAction("Move");
            _attackAction = input.actions.FindAction("Attack");
            
            _attackAction.performed += OnAttack;
        }

        public void Dispose()
        {
            _attackAction.performed -= OnAttack;
        }
        
        private void OnAttack(InputAction.CallbackContext _) => OnAttackPerformed?.Invoke();
    }

    public class PlayerHealth
    {
        
        public void ReceiveDamage(int val)
        {
            
        }
    }

    public class PlayerMoveController : ITickable
    {
        private readonly PlayerInputListener _input;
        private readonly PlayerCharacter _player;

        public PlayerMoveController(PlayerInputListener input, PlayerCharacter player)
        {
            _input = input;
            _player = player;
        }
        
        public void Tick()
        {
            Vector3 moveDir = new Vector3(_input.Move.x, 0f, _input.Move.y);
            moveDir = moveDir.sqrMagnitude > Mathf.Epsilon ? moveDir.normalized : Vector3.zero;
            
            _player.transform.position += _player.Speed * Time.deltaTime * moveDir;
        }
    }

}