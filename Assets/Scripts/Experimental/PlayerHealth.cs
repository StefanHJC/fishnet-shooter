using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Experimental
{
    public class PlayerHealth : NetworkBehaviour
    {   
        [SerializeField] private int _maxHealth;

        private readonly SyncVar<int> _currentHealth = new();
        private int _predictedHealth;
        
        public event Action<int> OnChanged;
        public event Action<int> OnPredictionChanged;

        public override void OnStartClient()
        {
            PlayerHealthBar bar = GetComponentInChildren<PlayerHealthBar>();
            
            if (!IsOwner)
            {
                enabled = false;
                
                return;
            }
            bar.gameObject.SetActive(false);
            FindObjectOfType<AmmoHealthCounter>().Init(this);
            bar.Init(this);
            
            _currentHealth.OnChange += OnServerSetHealth;
            RequestInit_ServerCmd();
        }

        [Client(RequireOwnership = false)]
        public void ReduceHealthPrediction_Client(int damage)
        {
            _predictedHealth -= damage;
            OnPredictionChanged?.Invoke(_predictedHealth);
            GetComponentInChildren<PlayerHealthBar>().SetPredicted(_predictedHealth);
        }

        private void Awake()
        {
            _predictedHealth = _maxHealth;
            OnPredictionChanged?.Invoke(_maxHealth);
        }
        
        [ServerRpc]
        private void RequestInit_ServerCmd(NetworkConnection conn=null)
        {
            conn.FirstObject.GetComponent<PlayerHealth>().SetHealth_Server(_maxHealth);
        }

        private void OnServerSetHealth(int prev, int next, bool asServer) => OnChanged?.Invoke(next);

        [ObserversRpc]
        private void Die_ObserverCmd()
        {
            gameObject.SetActive(false);
            Debug.Log($"{LogUtils.Client} Player died");
        }

        [Server]
        private void Die_Server()
        {
            gameObject.SetActive(false);
            Debug.Log($"{LogUtils.Server} Player died");
        }

        [Server]
        public void ReduceHealth_Server(int damage)
        {
            _currentHealth.Value -= damage;  
            _predictedHealth = _currentHealth.Value;
            
            if (_currentHealth.Value <= 0)
            {
                Die_Server();
                Die_ObserverCmd();
            }
        }

        [Server]
        private void SetHealth_Server(int val)
        {
            _currentHealth.Value = val;
            _predictedHealth = val;
        }
    }
}