using Noname.Core.Enums;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 적이 드롭할 자원 종류와 확률 정보를 표현합니다.
    /// </summary>
    [System.Serializable]
    public struct EnemyDropDefinition
    {
        /// <summary>드롭 종류.</summary>
        public ResourceDropType type;

        /// <summary>드롭 수량.</summary>
        public float amount;

        [UnityEngine.Range(0f, 1f)]
        /// <summary>드롭 확률(0~1).</summary>
        public float probability;

        [UnityEngine.Tooltip("확률을 무시하고 항상 지급할지 여부")]
        /// <summary>항상 지급 여부.</summary>
        public bool guaranteed;
    }
}
