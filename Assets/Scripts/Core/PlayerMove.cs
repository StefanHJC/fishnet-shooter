using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public class PlayerMove : NetworkBehaviour
    {
        [SerializeField] private PlayerInput _input;
        [SerializeField] private float _speed;

        
        private Vector2 _moveInput;

        public override void OnStartClient()
        {
            if (IsOwner)
                _input.enabled = true;
        }

        public void OnMove(InputValue val) => _moveInput = val.Get<Vector2>();

        private void Update()
        {
            if (!IsOwner)
                return;
            
            Vector3 moveDir = new Vector3(_moveInput.x, 0f, _moveInput.y);
            moveDir = moveDir.sqrMagnitude > 1f + Mathf.Epsilon ? moveDir.normalized : Vector3.zero;
            
            transform.position += _speed * Time.deltaTime * moveDir;
        }
    }
}