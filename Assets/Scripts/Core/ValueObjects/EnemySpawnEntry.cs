using UnityEngine;

namespace Noname.Core.ValueObjects
{
    [System.Serializable]
    public struct EnemySpawnEntry
    {
        [Tooltip("스폰할 적 정의 ScriptableObject")]
        public EnemyDefinition definition;
        [Tooltip("랜덤 선택 시 사용할 가중치 (0 이하이면 1로 처리)")]
        public float weight;
    }
}
