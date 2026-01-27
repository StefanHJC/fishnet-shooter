using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using Channel = FishNet.Transporting.Channel;

namespace Experimental
{
    public static class LogUtils
    {
        public const string Server = "<color=orange>[SERVER]</color>";
        public const string Client = "<color=green>[CLIENT]</color>";
    }

    public class XorShift32
    {
        private uint state;

        public XorShift32(uint seed)
        {
            state = seed;
        }

        public float NextFloat()
        {
            return (NextUInt() & 0xFFFFFF) / (float)0x1000000;
        }

        private uint NextUInt()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            
            return state;
        }
    }

    public struct TracerBroadcast : IBroadcast
    {
        public Vector3 Origin;
        public Vector3 Direction;
        public int ShooterId;
    }
    
    public struct MuzzleBroadcast : IBroadcast {}
    
    public class PlayerShoot : NetworkBehaviour
    {
        [SerializeField] private TrailRenderer _tracerPrefab;
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private ParticleSystem _bulletMist;
        [SerializeField] private PlayerInventory _ammo;
        [SerializeField] private Animator _weaponAnim;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Vector3 _spreadRange;
        [SerializeField] private int _damage;
        [SerializeField] private int _fireRate;
        [SerializeField] private float _bulletSpeed;

        private XorShift32 _prng;
        private uint _seed;
        private long _shotIndex;
        private float _lastShootTime;
        private float _shootDelay;

        public ParticleSystem BulletMist => _bulletMist;
        
        public override void OnStartServer()
        {
            InitPRNG_Server();
        }

        public override void OnStartClient()
        {
            if (!IsOwner)
                return;

            _shootDelay = 60f / _fireRate;
            RequestData_ServerCmd();
        }

        private void Start()
        {
            ClientManager.RegisterBroadcast<TracerBroadcast>(OnTracerBroadcast_Client);
            ServerManager.RegisterBroadcast<TracerBroadcast>(OnTracerBroadcast_Server);
        }

        private void OnDisable()
        {
            ClientManager.UnregisterBroadcast<TracerBroadcast>(OnTracerBroadcast_Client);
            ServerManager.UnregisterBroadcast<TracerBroadcast>(OnTracerBroadcast_Server);
        }

        [Server]
        private void OnTracerBroadcast_Server(NetworkConnection conn, TracerBroadcast msg, Channel _)
        {
            //TODO: Tick compensation
            //TODO: prng state sync
            //MuzzleVFX_ObserverCommand(conn);
            
            var networkConnections = new HashSet<NetworkConnection>(ServerManager.Clients.Select(x => x.Value));
            ServerManager.BroadcastExcept(networkConnections, excludedConnection: conn, msg, requireAuthenticated: true, Channel.Unreliable);
        }

        private void OnTracerBroadcast_Client(TracerBroadcast msg, Channel _) => ShotVFX_Client(msg.Origin, msg.Direction, playMuzzleFlash: msg.ShooterId == OwnerId).Forget();

        [Server]
        private void InitPRNG_Server()
        {
            _seed = (uint)DateTime.Now.Ticks;
            _prng = new XorShift32(_seed);
            Debug.Log($"{LogUtils.Server} Init prng");
            Debug.Log($"{LogUtils.Server} XorShift32 init via " + _seed);
        }

        [Client]
        public void TryShot_Client()
        {
            if (_lastShootTime + _shootDelay >= Time.time || _ammo.BulletsLeft <= 0 )
                return;

            Vector3 dir = GetDirection();
            
            Shot_Client(_firePoint.position, dir);
            ShotDebugRay_ServerCmd(_firePoint.position, dir);
        }

        [ServerRpc]
        private void ShotDebugRay_ServerCmd(Vector3 origin, Vector3 dir)
        {
            ShotDebugRay_ObserverCmd(origin, dir);
            ShotDebugRay_Server(origin, dir);
        }
        
        [Client]
        private void Shot_Client(Vector3 origin, Vector3 dir)
        {
            //TODO: Anim && particles
            ShotVFX_Client(origin, dir, true).Forget();
            
            //TODO: Check client can shoot, calculate where bullet actually is
            //TODO: Sync prng states
            ClientManager.Broadcast(new TracerBroadcast()
            {
                Origin = origin,
                Direction = dir,
                ShooterId = Owner.ClientId,
            }, Channel.Unreliable);

            _ammo.BulletsLeft--;
            
            if (Physics.Raycast(origin, dir, out RaycastHit hit, 100))
            {
                //TODO: Trail
                _lastShootTime = Time.time;
                
                if (hit.collider.TryGetComponent<PlayerHealth>(out var health))
                {
                    health.ReduceHealthPrediction_Client(15);
                    Shot_ServerCmd(origin, dir, health.transform.position, health.Owner);
                
                    Debug.Log($"{LogUtils.Client} Hit playerId = " + health.Owner.ClientId);
                }
            }
            else
            {
                _lastShootTime =  Time.time;
            }
        }

        [ServerRpc]
        private void Shot_ServerCmd(Vector3 originClient, Vector3 dirClient, Vector3 hittedPlayerOriginClient, NetworkConnection hittedPlayer, NetworkConnection conn=null)
        {
            Debug.Log($"{LogUtils.Server} Try validate shot from playerId {conn.ClientId}");
            Debug.Log($"{LogUtils.Server} fire point server {_firePoint.position} client {originClient} " +
                      $"dist {Vector3.Distance(originClient, _firePoint.position)} res {Vector3.Distance(originClient, _firePoint.position) < .5}");
            
            Debug.Log($"{LogUtils.Server} hitted point server {hittedPlayer.FirstObject.transform.position} client {hittedPlayerOriginClient} " +
                      $"dist {Vector3.Distance(hittedPlayerOriginClient, hittedPlayer.FirstObject.transform.position)} " +
                      $"res {Vector3.Distance(hittedPlayerOriginClient, hittedPlayer.FirstObject.transform.position) < .5}");
            
            
            if (Vector3.Distance(originClient, _firePoint.position) < .5 && 
                Vector3.Distance(hittedPlayerOriginClient, hittedPlayer.FirstObject.transform.position) < .5)
            {
                hittedPlayer.FirstObject.GetComponent<PlayerHealth>().ReduceHealth_Server(15);
                Debug.Log($"{LogUtils.Server} Confirm playerId {conn.ClientId}");
            }
        }

        [Client]
        private async UniTaskVoid ShotVFX_Client(Vector3 origin, Vector3 dir, bool playMuzzleFlash)
        {
            if (playMuzzleFlash)
            {
                MuzzleVFX_Client();
                ShotRecoil_Client();
            }
            
            TrailRenderer tracer = Instantiate(_tracerPrefab, origin, Quaternion.identity);
            tracer.transform.forward = dir;
            RaycastHit hit = default;
            Vector3 hitPoint = hit.collider == null ? tracer.transform.forward * 100 : hit.point;
            float distance = hit.collider == null ? 100 : hit.distance;
            float remainingDistance = distance;

            while (remainingDistance > Mathf.Epsilon)
            {
                tracer.transform.position = Vector3.Lerp(origin, hitPoint, 1 - (remainingDistance / distance));
                remainingDistance -= _bulletSpeed * Time.deltaTime;
                
                await UniTask.WaitForEndOfFrame();
            }
            Destroy(tracer.gameObject);
        }

        private void ShotRecoil_Client()
        {
            _weaponAnim.speed = 10;
            _weaponAnim.SetTrigger("Shot");
        }

        [Client]
        private void MuzzleVFX_Client()
        {
            if (_muzzleFlash.isPlaying)
                _muzzleFlash.Stop();
            
            _muzzleFlash.Play();
        }

        [ObserversRpc(ExcludeOwner = false)]
        private void ShotDebugRay_ObserverCmd(Vector3 origin, Vector3 dir)
        {
            Debug.DrawRay(origin, dir * 100, Color.red, 1);
        }
        
        [Server]
        private void ShotDebugRay_Server(Vector3 origin, Vector3 dir)
        {
            Debug.DrawRay(origin, dir * 100, Color.red, 1);
        }

        private Vector3 GetDirection()
        {
            Vector3 dir = _firePoint.forward;
            
            dir += new Vector3(
                (_prng.NextFloat() * 2f - 1f) * _spreadRange.x,
                (_prng.NextFloat() * 2f - 1f) * _spreadRange.y,
                0f);

            return dir.normalized; // _firePoint.forward;
        }

        [ServerRpc]
        private void RequestData_ServerCmd(NetworkConnection conn=null)
        {
            InitPRNG_ClientCmd(conn, _seed/*(uint)DateTime.Now.Ticks*/);
        }
        
        [TargetRpc]
        private void InitPRNG_ClientCmd(NetworkConnection target, uint seed)
        {
            _seed = seed;
            _shotIndex = 0;
            
            _prng = new XorShift32(seed);
            Debug.Log($"{LogUtils.Client} Init prng");   
            Debug.Log($"{LogUtils.Client} XorShift32 init via " + seed);
        }
    }
}