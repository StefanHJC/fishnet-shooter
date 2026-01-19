using FishNet.Object;

namespace Core
{
    public class PlayerInstance : NetworkObject
    {
        public readonly PlayerCharacter Character;
        public readonly string Name;
        public readonly int Id;
        public readonly int TeamId;
    }
}