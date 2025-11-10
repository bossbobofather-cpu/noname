using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 특정 스탯을 어떻게 변경할지에 대한 단일 규칙입니다.
    /// </summary>
    [System.Serializable]
    public struct GameplayEffectModifier
    {
        /// <summary>대상 속성.</summary>
        public GameplayAttribute attribute;

        /// <summary>덧셈/곱셈 등 적용 방식.</summary>
        public ModifierOperation operation;

        [Tooltip("계산 방식(Add/Multiply)에 따라 더하거나 곱해질 값")]
        /// <summary>적용 수치.</summary>
        public float value;
    }
}
