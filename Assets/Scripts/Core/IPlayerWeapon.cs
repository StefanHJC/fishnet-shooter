using FishNet.Broadcast;
using UnityEngine;

namespace Core
{
    public interface IPlayerWeapon
    {
        int Id { get; }
        int MaxAmmo { get; }
        int CurrentAmmo { get; }

        bool TryShot();

        //bool TryReload();
    }

    public class ProjectileWeapon : IPlayerWeapon
    {
        public int Id { get; private set; }
        public int MaxAmmo { get; private set; }
        public int CurrentAmmo { get; private set; }
        
        public bool TryShot()
        {
            throw new System.NotImplementedException();
        }
    }

    public class HitScanWeapon : IPlayerWeapon
    {
        public int Id { get; private set; }
        public int MaxAmmo { get; private set; }
        public int CurrentAmmo { get; private set; }
        
        public bool TryShot()
        {
            throw new System.NotImplementedException();
        }
    }

    public readonly struct ClientShotRequest
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;
        public readonly int WeaponId;
    }
}