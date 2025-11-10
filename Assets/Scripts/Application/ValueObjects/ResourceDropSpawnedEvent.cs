using Noname.Core.Enums;
using Noname.Core.Primitives;

namespace Noname.Application.ValueObjects
{
    public readonly struct ResourceDropSpawnedEvent
    {
        public ResourceDropSpawnedEvent(int dropId, ResourceDropType type, Float2 position, float amount, float pickupDelay)
        {
            DropId = dropId;
            Type = type;
            Position = position;
            Amount = amount;
            PickupDelay = pickupDelay;
        }

        public int DropId { get; }
        public ResourceDropType Type { get; }
        public Float2 Position { get; }
        public float Amount { get; }
        public float PickupDelay { get; }
    }
}
