using Noname.Core.Enums;

namespace Noname.Core.ValueObjects
{
    [System.Serializable]
    public struct EnemyDropDefinition
    {
        public ResourceDropType type;
        public float amount;
        [UnityEngine.Range(0f, 1f)]
        public float probability;
        [UnityEngine.Tooltip("확률과 무관하게 항상 드랍할지 여부")]
        public bool guaranteed;
    }
}
