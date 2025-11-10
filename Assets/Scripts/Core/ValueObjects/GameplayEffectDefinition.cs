using System;
using System.Collections.Generic;
using Noname.Core.Entities;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    [CreateAssetMenu(menuName = "Defense/Gameplay Effect", fileName = "GameplayEffect")]
    public sealed class GameplayEffectDefinition : ScriptableObject
    {
        [SerializeField] private GameplayEffectModifier[] modifiers = Array.Empty<GameplayEffectModifier>();

        public IReadOnlyList<GameplayEffectModifier> Modifiers => modifiers ?? Array.Empty<GameplayEffectModifier>();

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
