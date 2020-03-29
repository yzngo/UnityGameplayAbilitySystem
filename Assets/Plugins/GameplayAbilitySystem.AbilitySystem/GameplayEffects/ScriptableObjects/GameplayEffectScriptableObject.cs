using System.Collections;
using System.Collections.Generic;
using GameplayAbilitySystem.AbilitySystem.GameplayTags.ScriptableObjects;
using UnityEngine;

namespace GameplayAbilitySystem.AbilitySystem.GameplayEffects.ScriptableObjects {
    /// <summary>
    /// This class defines the data required to instantiate a GameplayEffect
    /// </summary>
    public class GameplayEffectScriptableObject : ScriptableObject {
        public GameplayTagsContainerScriptableObject AssetTags;
        public GameplayTagsContainerScriptableObject GrantedTags;
        public GameplayTagsContainerScriptableObject OngoingTagRequirements;
        public GameplayTagsContainerScriptableObject RemoveGameplayEffectsWithTags;
        public GameplayTagsContainerScriptableObject ApplicationTagRequirements;
        public GameplayTagsContainerScriptableObject GrantedApplicationImmunityTags;

    }
}