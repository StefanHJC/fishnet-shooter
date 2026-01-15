using Cysharp.Threading.Tasks;
using FishNet.Object;
using UnityEngine;

namespace Core
{
    public class DespawnAfterTime : NetworkBehaviour
    {
        [SerializeField] private float _time;

        public override void OnStartServer()
        {
            WaitAndDespawnAsync(_time).Forget();
        }

        private async UniTaskVoid WaitAndDespawnAsync(float time)
        {
            await UniTask.WaitForSeconds(time);
            
            Despawn();
        }
    }
}