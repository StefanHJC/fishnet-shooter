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
        [SerializeField] private ParticleSystem _vfx;
        [SerializeField] private float _splashRadius;

        private PlayerThrowGrenade _thrower;
        private MeshRenderer _renderer;
        private bool _drawSplashGizmo;
        private readonly List<Vector3> _trackedPositions = new();

        public Rigidbody Rigidbody => _rigidbody;
        public float Splash => _splashRadius;
        public int Damage => MaxDamage;
        
        public event Action<Vector3, PlayerGrenade> OnExploded;

        private void Awake()
        {
            _vfx = GetComponentInChildren<ParticleSystem>();
            _renderer = GetComponentInChildren<MeshRenderer>();
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            ExplodeAsync().Forget();
        }

        private void FixedUpdate()
        {
            _trackedPositions.Add(transform.position);
            
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
            if (!_drawSplashGizmo)
                return;
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _splashRadius);
        }

        private async UniTaskVoid ExplodeAsync()
        {
            _drawSplashGizmo = true;
            _rigidbody.isKinematic = true;
            
            await UniTask.WaitForSeconds(.5f);
            PlayVFXAndDespawnAsync().Forget();

            OnExploded?.Invoke(transform.position, this);
            _trackedPositions.Clear();
        }
        
        private async UniTask PlayVFXAndDespawnAsync()
        {
            _renderer.enabled = false;
            
            if(!IsServerOnlyInitialized)
                _vfx.Play();
                       
            await UniTask.WaitForSeconds(_vfx.main.duration);

            Despawn(this);
            _drawSplashGizmo = false;
        }
    }
}