using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 적 스폰 후보와 가중치를 나타냅니다.
    /// </summary>
    [System.Serializable]
    public struct EnemySpawnEntry
    {
        [Tooltip("스폰할 적 정의 ScriptableObject")]
        /// <summary>적 정의.</summary>
        public EnemyDefinition definition;

        [Tooltip("랜덤 선택 시 참고할 가중치 (0 이하이면 1로 처리)")]
        /// <summary>스폰 가중치.</summary>
        public float weight;
    }
}
