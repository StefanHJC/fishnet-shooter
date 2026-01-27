using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Experimental
{
    public class AmmoHealthCounter : MonoBehaviour
    {
        private const string AmmoLabel = "AMMO";
        private const string HealthLabel = "HP";
        
        [SerializeField] private TMP_Text _ammoField;
        [SerializeField] private TMP_Text _healthField;
        
        private PlayerInventory _ammo;
        
        public void Init(PlayerInventory ammo)
        {
            _ammo = ammo;
        }

        public void Init(PlayerHealth playerHealth)
        {
            playerHealth.OnChanged += (int val) => _healthField.text = $"{HealthLabel} {val}";
        }

        private void Update()
        {
            if (_ammo == null)
                return;
            
            _ammoField.text = $"{_ammo.BulletsLeft}/{_ammo.BulletsTotalLeft}";
        }
    }
}