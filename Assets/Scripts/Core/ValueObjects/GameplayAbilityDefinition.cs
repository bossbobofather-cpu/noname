using System;
using System.Collections.Generic;
using Noname.Core.Entities;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 플레이어에게 적용할 수 있는 능력(패시브)의 정의입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Defense/Gameplay Ability", fileName = "GameplayAbility")]
    public sealed class GameplayAbilityDefinition : ScriptableObject
    {
        [SerializeField] private string abilityId = Guid.NewGuid().ToString();
        [SerializeField] private string title;
        [SerializeField, TextArea] private string description;
        [SerializeField] private GameplayEffectDefinition[] effects = Array.Empty<GameplayEffectDefinition>();

        /// <summary>고유 ID (비어있으면 Asset 이름 사용).</summary>
        public string Id => string.IsNullOrWhiteSpace(abilityId) ? name : abilityId;

        /// <summary>표시용 이름.</summary>
        public string Title => string.IsNullOrWhiteSpace(title) ? name : title;

        /// <summary>설명 텍스트.</summary>
        public string Description => description ?? string.Empty;

        /// <summary>적용될 효과 목록.</summary>
        public IReadOnlyList<GameplayEffectDefinition> Effects => effects ?? Array.Empty<GameplayEffectDefinition>();

        /// <summary>
        /// 모든 GameplayEffect를 순차적으로 적용합니다.
        /// </summary>
        public void Apply(PlayerEntity target)
        {
            if (target == null)
            {
                return;
            }

            var list = Effects;
            for (int i = 0; i < list.Count; i++)
            {
                list[i]?.Apply(target);
            }
        }
    }
}
