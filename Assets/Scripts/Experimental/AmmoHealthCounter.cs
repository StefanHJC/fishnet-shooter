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

        public void Init(PlayerHealth playerHealth)
        {
            playerHealth.OnChanged += (int val) => _healthField.text = $"{HealthLabel} {val}";
        }
    }
}