using FishNet.Object;
using UnityEngine;

namespace Core
{
    public class RoomController : NetworkBehaviour
    {
        public override void OnStartServer()
        {
            Debug.Log("RoomCtrl OnStartServer" + ObjectId);
        }

        public override void OnStartClient()
        {
            Debug.Log("RoomCtrl OnStartClient" + ObjectId);
        }
    }
}