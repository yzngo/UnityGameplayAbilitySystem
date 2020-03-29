using System.Collections;
using System.Collections.Generic;
using GameplayAbilitySystem.AbilitySystem.GameplayTags.ScriptableObjects;
using UnityEngine;

namespace GameplayAbilitySystem.AbilitySystem.Abilities.ScriptableObjects {
    [CreateAssetMenu(fileName = "Ability", menuName = "Gameplay Ability System/Abilities/Ability Tag Spec")]
    public class AbilityTagsSpecScriptableObject : ScriptableObject {
        public GameplayTagsContainerScriptableObject AbilityTags;
        public GameplayTagsContainerScriptableObject CancelAbilitiesWithTags;
        public GameplayTagsContainerScriptableObject BlockAbilitiesWithTags;
        public GameplayTagsContainerScriptableObject ActivationOwnedTags;
        public GameplayTagsContainerScriptableObject ActivationRequiredTags;
        public GameplayTagsContainerScriptableObject ActivationBlockedTags;
        public GameplayTagsContainerScriptableObject SourceRequiredTags;
        public GameplayTagsContainerScriptableObject SourceBlockedTags;
        public GameplayTagsContainerScriptableObject TargetRequiredTags;
        public GameplayTagsContainerScriptableObject TargetBlockedTags;
    }
}