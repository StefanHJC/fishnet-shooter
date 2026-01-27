using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Experimental
{
    public enum AmmoType
    {
        Bullets,
        Grenade,
    }
    
    public class PlayerInventory : NetworkBehaviour
    {
        public const int BulletsPerMagazine = 32;
        
        [SerializeField] private int _grenadesCount = 1;
        [SerializeField] private int _bulletsTotalLeft = BulletsPerMagazine;
        [SerializeField] private int _reloadDuration = 2;

        public int BulletsLeft = BulletsPerMagazine;
        public int BulletsTotalLeft => _bulletsTotalLeft;

        public override void OnStartClient()
        {
            if (!IsOwner)
                return;
            
            FindAnyObjectByType<AmmoHealthCounter>().Init(this);
        }

        [Server]
        public void Add_Server(NetworkConnection targetPlayer, AmmoType type, int count)
        {
            AddByType(type, count);
            Add_ClientCmd(targetPlayer, type, count);
        }

        [Client]
        public void TryReload_Client()
        {
            Debug.Log($"{LogUtils.Client} TryReload_Client");
            
            if (_bulletsTotalLeft <= 0 && BulletsLeft < _bulletsTotalLeft)
               return;

            RequestReload_ServerCmd();
        }

        [ServerRpc]
        private void RequestReload_ServerCmd(NetworkConnection conn = null)
        {
            if (_bulletsTotalLeft <= 0)
                return;

            int toReload = Mathf.Clamp(_bulletsTotalLeft, 1, BulletsPerMagazine);
            
            ProcessReload_Server(conn, toReload).Forget();
        }

        [Server]
        private async UniTaskVoid ProcessReload_Server(NetworkConnection conn, int count)
        {
            //TODO: ProcessReload_Client()
            await UniTask.WaitForSeconds(_reloadDuration);
            
            _bulletsTotalLeft -= count;
            BulletsLeft = count;
            
            Reload_ClientCmd(conn, count);
        }

        [TargetRpc]
        private void Reload_ClientCmd(NetworkConnection conn, int count)
        {
            //Todo play anim
            Debug.Log($"{LogUtils.Client} Reload complete");
            BulletsLeft = count;
            _bulletsTotalLeft -= count;
        }

        [TargetRpc]
        private void Add_ClientCmd(NetworkConnection conn, AmmoType type, int count)
        {
            Debug.Log($"{LogUtils.Client} Received {count} {type.ToString()}");
            AddByType(type, count);
        }

        private void AddByType(AmmoType type, int count)
        {
            switch (type)
            {
                case AmmoType.Bullets:
                    _bulletsTotalLeft +=  count;
                    break;
                case AmmoType.Grenade:
                    _grenadesCount += count;
                    break;
            }
        }
    }
}