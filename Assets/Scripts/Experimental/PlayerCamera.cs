using FishNet.Object;
using UnityEngine;

namespace Experimental
{
    public class PlayerCamera : NetworkBehaviour
    {
        [SerializeField] private Camera _prefab;
        [SerializeField] private Transform _holder;

        public override void OnStartClient()
        {
            if (!IsOwner)
                return;
            
            Instantiate(_prefab, _holder.position, _holder.rotation, _holder).gameObject.SetActive(true);
        }
    }
}