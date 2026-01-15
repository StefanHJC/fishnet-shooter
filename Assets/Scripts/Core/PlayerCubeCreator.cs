using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public class PlayerCubeCreator : NetworkBehaviour
    {
        [SerializeField] private NetworkObject _cubePrefab;
        [SerializeField] private PlayerInput _input;
        
        public override void OnStartClient()
        {
            if (IsOwner)
                _input.enabled = true;
        }

        public void OnAttack(InputValue val)
        {
            if (val.isPressed)
                SpawnCube();
        }

        [ServerRpc]
        private void SpawnCube()
        {
            NetworkObject obj = Instantiate(_cubePrefab, transform.position, Quaternion.identity);
            
            Spawn(obj);
            obj.GetComponent<SetMaterialColour>().Colour.Value = Random.ColorHSV();
        }
    }
}