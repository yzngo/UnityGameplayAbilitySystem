using System.Collections;
using System.Collections.Generic;
using GameplayAbilitySystem.AbilitySystem.GameplayEffects.ScriptableObjects;
using GameplayAbilitySystem.AbilitySystem.GameplayTags.ScriptableObjects;
using UnityEngine;

namespace GameplayAbilitySystem.AbilitySystem.Abilities.ScriptableObjects {
    [CreateAssetMenu(fileName = "Ability", menuName = "Gameplay Ability System/Abilities/Ability")]
    public class AbilityDefinitionScriptableObject : ScriptableObject {
        public SpritePreview Sprite;
        public AbilityTagsSpecScriptableObject AbilityTags;
        public GameplayEffectScriptableObject Cost;
        public GameplayEffectScriptableObject Cooldown;
        public AbilityExecutionFlow ExecutionFlow;
    }
}
