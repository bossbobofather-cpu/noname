using System;
using System.Collections.Generic;
using Noname.Core.Entities;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 하나 이상의 스탯 수정자를 묶은 Gameplay Effect 정의입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Defense/Gameplay Effect", fileName = "GameplayEffect")]
    public sealed class GameplayEffectDefinition : ScriptableObject
    {
        [SerializeField] private GameplayEffectModifier[] modifiers = Array.Empty<GameplayEffectModifier>();

        /// <summary>실행될 수정자 목록.</summary>
        public IReadOnlyList<GameplayEffectModifier> Modifiers => modifiers ?? Array.Empty<GameplayEffectModifier>();

        /// <summary>
        /// 보유 중인 모든 Modifier를 플레이어에게 적용합니다.
        /// </summary>
        public void Apply(PlayerEntity target)
        {
            if (target == null)
            {
                return;
            }

            var list = Modifiers;
            for (int i = 0; i < list.Count; i++)
            {
                var modifier = list[i];
                target.ApplyModifier(modifier.attribute, modifier.operation, modifier.value);
            }
        }
    }
}
