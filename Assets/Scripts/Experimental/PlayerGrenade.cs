using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FishNet.Object;
using UnityEngine;

namespace Experimental
{
    public class PlayerGrenade : NetworkObject
    {
        private const int MaxDamage = 200;
        
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _splashRadius;

        private bool _drawSplash;
        private readonly List<Vector3> _trackedPositions = new();
        private PlayerThrowGrenade _thrower;
        
        public Rigidbody Rigidbody => _rigidbody;
        public float Splash => _splashRadius;
        public int Damage => MaxDamage;
        
        public event Action<Vector3, PlayerGrenade> OnExploded;
        
        private void OnCollisionEnter(Collision collision)
        {
            ExplodeAsync().Forget();
        }

        private void FixedUpdate()
        {
            _trackedPositions.Add(transform.position);
            transform.forward = _rigidbody.linearVelocity;
            
            if (_trackedPositions.Count <= 0)
                return;

            for (int i = 0; i < _trackedPositions.Count; i++)
            {
                if (_trackedPositions.Count < i + 2)
                    break;
                
                Debug.DrawLine(_trackedPositions[i], _trackedPositions[i + 1], Color.magenta);
            }
        }

        private void OnDrawGizmos()
        {
            if (!_drawSplash)
                return;
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _splashRadius);
        }

        private async UniTaskVoid ExplodeAsync()
        {
            _drawSplash = true;
            _rigidbody.isKinematic = true;
            
            await UniTask.WaitForSeconds(4f);

            OnExploded?.Invoke(transform.position, this);
            _trackedPositions.Clear();
            Despawn();
        }
    }
}