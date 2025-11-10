using Noname.Core.Enums;

namespace Noname.Application.ValueObjects
{
    /// <summary>
    /// 대기 중이던 드롭이 수집됐을 때 발생하는 이벤트입니다.
    /// </summary>
    public readonly struct ResourceDropCollectedEvent
    {
        public ResourceDropCollectedEvent(int dropId, ResourceDropType type, float amount)
        {
            DropId = dropId;
            Type = type;
            Amount = amount;
        }

        /// <summary>드롭 ID.</summary>
        public int DropId { get; }

        /// <summary>드롭 종류.</summary>
        public ResourceDropType Type { get; }

        /// <summary>수집된 양.</summary>
        public float Amount { get; }
    }
}
