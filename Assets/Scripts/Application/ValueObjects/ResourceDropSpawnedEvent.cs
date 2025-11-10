using Noname.Core.Enums;
using Noname.Core.Primitives;

namespace Noname.Application.ValueObjects
{
    /// <summary>
    /// 새 자원 드롭이 필드에 생성됐을 때 전달되는 이벤트입니다.
    /// </summary>
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

        /// <summary>드롭 ID.</summary>
        public int DropId { get; }

        /// <summary>드롭 종류.</summary>
        public ResourceDropType Type { get; }

        /// <summary>스폰 위치.</summary>
        public Float2 Position { get; }

        /// <summary>드롭 양.</summary>
        public float Amount { get; }

        /// <summary>자동 수집까지 남은 지연.</summary>
        public float PickupDelay { get; }
    }
}
