using System;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 스테이지 진행에 필요한 기본 파라미터를 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Defense/Stage Definition", fileName = "StageDefinition")]
    public sealed class StageDefinition : ScriptableObject
    {
        public const uint DifficultyScaleDenominator = 10000;
        public const uint DefaultDifficultyScaleRaw = DifficultyScaleDenominator;

        [Tooltip("스테이지를 구분하기 위한 코드")]
        public string stageCode = "Stage001";

        [Tooltip("격자 열 개수 (예: 100 = 100열)")]
        public uint waveColumns = 10;

        [Tooltip("각 행에서 동시에 스폰할 몬스터 수")]
        public uint spawnCountPerRow = 1;

        [Tooltip("난이도 배율을 만분율로 표현 (10000 = 1.0배)")]
        public uint difficultyScaleRaw = DefaultDifficultyScaleRaw;

        [Tooltip("몬스터 스폰 가중치")]
        public StageMonsterWeight[] monsterWeights = Array.Empty<StageMonsterWeight>();

        /// <summary>
        /// 난이도 배율 (예: 12000 -> 1.2f)
        /// </summary>
        public float DifficultyMultiplier => difficultyScaleRaw / (float)DifficultyScaleDenominator;
    }

    [Serializable]
    public struct StageMonsterWeight
    {
        public EnemyDefinition enemy;
        public float weight;
    }
}
