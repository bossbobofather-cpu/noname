using UnityEngine;

namespace Noname.Core.ValueObjects
{
    [System.Serializable]
    public struct GameplayEffectModifier
    {
        public GameplayAttribute attribute;
        public ModifierOperation operation;
        [Tooltip("연산 방식(Add/Multiply)에 따라 더해지거나 곱해지는 값")]
        public float value;
    }
}
