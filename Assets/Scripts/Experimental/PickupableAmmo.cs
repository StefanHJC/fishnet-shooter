using System;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Experimental
{
    public class PickupableAmmo : NetworkBehaviour
    {
        [SerializeField] private GameObject _bulletMesh;
        [SerializeField] private GameObject _grenadeMesh;
        [SerializeField] private GameObject _currentMesh;
        [SerializeField] private Vector2 _spawnTimeInterval = new Vector2(10, 20);
        [SerializeField] private Vector2Int _spawnCountInterval = new Vector2Int(1, 5);
        [SerializeField] private float _spawnRadius = 10f;

        private Vector3 _cachedPos;
        private AmmoType _type;
        private int _count;
        
        
        public override void OnStartServer()
        {
            _cachedPos = transform.position;
            _type = (AmmoType)Random.Range(0, 1);
            _count = Random.Range(_spawnCountInterval.x, _spawnCountInterval.y);
            
            if (_type == AmmoType.Bullets)
                _count *= PlayerInventory.BulletsPerMagazine;
        }

        public override void OnStartClient()
        {
            SetView_Client(forType: _type);
        }
        
        private void Update()
        {
            if (IsServerOnlyInitialized)
                return;
            
            _currentMesh.transform.Rotate(Vector3.up, 45 * Time.deltaTime, Space.World);
        }
        
        [Client]
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerHealth player))
            {
                RequestPickupAmmo_ServerCmd(conn: player.Owner);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _spawnRadius);
        }

        [Server]
        private async UniTaskVoid SpawnAsync_Server()
        {
            await UniTask.WaitForSeconds(Random.Range(_spawnTimeInterval.x, _spawnTimeInterval.y));
            
            Debug.Log($"{LogUtils.Server} Spawn Ammo");
            _type = (AmmoType)Random.Range(0, 1);
            _count = Random.Range(_spawnCountInterval.x, _spawnCountInterval.y);
            
            if (_type == AmmoType.Bullets)
                _count *= PlayerInventory.BulletsPerMagazine;
            
            
            Vector3 randomPos = Random.insideUnitCircle * _spawnRadius;
            transform.position = new Vector3(_cachedPos.x + randomPos.x, _cachedPos.y, _cachedPos.z + randomPos.y);
            
            Spawn_ObserverCmd(transform.position, _type);
        }

        [ServerRpc(RequireOwnership =  false)]
        private void RequestPickupAmmo_ServerCmd(NetworkConnection conn)
        {
            //TODO: validate player pos
            Debug.Log($"{LogUtils.Server} RequestPickupAmmo ServerCmd");
            conn!.FirstObject.GetComponent<PlayerInventory>().Add_Server(conn, _type, _count);
            gameObject.SetActive(false);
            OnPickupConfirmed_ObserverCmd();
            
            SpawnAsync_Server().Forget();
        }

        [ObserversRpc]
        private void OnPickupConfirmed_ObserverCmd()
        {
            gameObject.SetActive(false);
        }
        
        [ObserversRpc]
        private void Spawn_ObserverCmd(Vector3 position, AmmoType type)
        {
            _type = type;
            transform.position = position;
            gameObject.SetActive(true);
            
            SetView_Client(forType: type);
        }

        [Client]
        private void SetView_Client(AmmoType forType)
        {
            Debug.Log($"{LogUtils.Client} SetView for type {forType.ToString()}");
            _bulletMesh.SetActive(false);
            _grenadeMesh.SetActive(false);
            
            switch (forType)
            {
                case AmmoType.Bullets:
                    _bulletMesh.gameObject.SetActive(true);
                    break;
                
                case AmmoType.Grenade:
                    _grenadeMesh.gameObject.SetActive(true);
                    break;
            }
        }
    }
}