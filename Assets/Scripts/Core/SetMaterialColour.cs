using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Core
{
    public class SetMaterialColour : NetworkBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;
        
        public readonly SyncVar<Color> Colour = new SyncVar<Color>();

        private void Awake()
        {
            Colour.OnChange += SetColour;
        }

        private void OnDestroy()
        {
            Colour.OnChange -= SetColour;
        }
        
        private void SetColour(Color prev, Color next, bool asServer)
        {
            Debug.Log("SET " + Colour.Value);
            GetComponent<MeshRenderer>().material.color = Colour.Value;
        }
    }
}