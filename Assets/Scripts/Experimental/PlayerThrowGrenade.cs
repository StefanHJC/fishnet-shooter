using System.Numerics;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Experimental
{
    public interface IWeaponHandler
    {
        void OnUse();
    }
    
    public class PlayerThrowGrenade : NetworkBehaviour, IWeaponHandler
    {
        [SerializeField] private PlayerGrenade _projectilePrefab; 
        [SerializeField] private Transform _projectileSpawn;
        [SerializeField] private float _throwForce;
        
        public override void OnStartClient()
        {
            //TODO: sub on throw
        }

        public void OnUse()
        {
            Throw_Client();
        }

        [Client]
        private void Throw_Client()
        {
            Debug.Log($"{LogUtils.Client} Throw Client");
            Throw_ServerCmd(_projectileSpawn.rotation, _projectileSpawn.forward);
        }

        [ServerRpc]
        private void Throw_ServerCmd(Quaternion spawnRotation, Vector3 spawnForward)
        {
            PlayerGrenade grenade = Instantiate(_projectilePrefab, _projectileSpawn.position, spawnRotation);
            grenade.transform.forward = spawnForward;
            grenade.Rigidbody.AddForce(spawnForward * _throwForce, ForceMode.Impulse);
            grenade.gameObject.SetActive(true);
            Spawn(grenade);

            grenade.OnExploded += ProcessExplosion_Server;
            Throw_ObserverCmd(spawnForward * _throwForce, grenade);
        }

        [Server]
        private void ProcessExplosion_Server(Vector3 pos, PlayerGrenade grenade)
        {
            grenade.OnExploded -= ProcessExplosion_Server;

            var affected = Physics.OverlapSphere(pos, grenade.Splash);

            foreach (var collider in affected)
            {
                if (collider.TryGetComponent(out PlayerHealth health))
                {
                    float calculatedDamage = CalculateDamageLinear_Server(health, grenade);
                    health.ReduceHealth_Server(Mathf.RoundToInt(calculatedDamage));
                }
            }
        }

        [Server]
        private float CalculateDamageLinear_Server(PlayerHealth health, PlayerGrenade grenade)
        {
            Vector2 grenadeProjXZ = new Vector2(grenade.transform.position.x, grenade.transform.position.z);
            Vector2 playerProjXZ = new Vector2(health.transform.position.x, health.transform.position.z);
            
            Debug.Log($"{LogUtils.Server} Damage Linear Server {Mathf.Clamp01(1 / Vector2.Distance(grenadeProjXZ, playerProjXZ) + Mathf.Epsilon)}");
            
            return grenade.Damage * Mathf.Clamp01(1 / Vector2.Distance(grenadeProjXZ, playerProjXZ) + Mathf.Epsilon);
        }
        
        [ObserversRpc]
        private void Throw_ObserverCmd(Vector3 impulse, NetworkObject nob)
        {
            PlayerGrenade grenade = nob.GetComponent<PlayerGrenade>();
            grenade.Rigidbody.AddForce(impulse, ForceMode.Impulse);
        }
    }
}