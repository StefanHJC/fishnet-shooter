using Experimental;
using UnityEngine;

public class WeaponCallbacks : MonoBehaviour
{
    [SerializeField] private PlayerShoot _player;
    
    private void OnBulletThrown()
    {
        _player.BulletMist.Play();
    }
}
