using System;
using FishNet.Object;

namespace Core
{
    public class PlayerShootController : IDisposable
    {
        private readonly PlayerInputListener _input;
        private readonly ServerShootController _serverShoot;

        public PlayerShootController(PlayerInputListener input, ServerShootController serverShoot)
        {
            _input = input;
            _serverShoot = serverShoot;

            _input.OnAttackPerformed += TryPerformAttack;
        }

        public void Dispose()
        {
            _input.OnAttackPerformed -= TryPerformAttack;
        }

        private void TryPerformAttack()
        {
            throw new NotImplementedException();
        }

        private void PerformAttackRpc()
        {
            
        }

        private void PerformAttack()
        {
            
        }
    }
    
    public class ServerShootController : NetworkBehaviour
    {
        public void Construct()
        {
            
        }
        
        [ServerRpc]
        private void PerformShotRpc()
        {
            
        }
        
        //private void Send
    }
}