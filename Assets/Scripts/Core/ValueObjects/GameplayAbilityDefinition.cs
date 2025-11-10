using System;
using System.Collections.Generic;
using Noname.Core.Entities;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    [CreateAssetMenu(menuName = "Defense/Gameplay Ability", fileName = "GameplayAbility")]
    public sealed class GameplayAbilityDefinition : ScriptableObject
    {
        [SerializeField] private string abilityId = Guid.NewGuid().ToString();
        [SerializeField] private string title;
        [SerializeField, TextArea] private string description;
        [SerializeField] private GameplayEffectDefinition[] effects = Array.Empty<GameplayEffectDefinition>();

        public string Id => string.IsNullOrWhiteSpace(abilityId) ? name : abilityId;
        public string Title => string.IsNullOrWhiteSpace(title) ? name : title;
        public string Description => description ?? string.Empty;
        public IReadOnlyList<GameplayEffectDefinition> Effects => effects ?? Array.Empty<GameplayEffectDefinition>();

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
