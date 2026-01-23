using FishNet.Object;
using TMPro;
using UnityEngine;

namespace Experimental
{
    public class PlayerHealthBar : NetworkBehaviour
    {
        [SerializeField] private TMP_Text _healthField;

        public void Init(PlayerHealth playerHealth)
        {
            playerHealth.OnChanged += (val) => 
            { 
                _healthField.text = val.ToString();
                SetNewVal_ServerCmd(val);
            };
            
            playerHealth.OnPredictionChanged += SetPredicted;
        }

        [Client(RequireOwnership = false)]
        public void SetPredicted(int val) => _healthField.text = val.ToString();

        [ServerRpc]
        private void SetNewVal_ServerCmd(int val)
        {
            _healthField.text = val.ToString();
            
            SetNewVal_ObserverCmd(val);
        }

        [ObserversRpc(ExcludeOwner = true)]
        private void SetNewVal_ObserverCmd(int val)
        {
            _healthField.text = val.ToString();
        }
    }
}