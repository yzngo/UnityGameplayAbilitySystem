/*
 * Created on Sun Jan 05 2020
 *
 * The MIT License (MIT)
 * Copyright (c) 2020 Sahil Jain
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */



namespace GameplayAbilitySystem.AbilitySystem.GameplayTags.Interfaces {
    public interface IGameplayTagBufferElement { }

    /*********************************************************
    These gameplay tags are defined by abilities
    **********************************************************/

    /// <summary>
    /// These are tags an ability owns.  They are used to describe abilities.
    /// </summary>
    public interface IAbilityTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// Active abilities which own these tags are cancelled when this ability is activated
    /// </summary>
    public interface ICancelAbilitiesWithTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// Abilities that own these tags are blocked from activating (but not cancelled).
    /// </summary>
    public interface IBlockAbilitiesWithTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// These tags are given to the casting actor while the ability is active
    /// </summary>
    public interface IActivationOwnedTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// This ability can only be activated if the source actor has all these tags
    /// </summary>
    public interface IActivationRequiredTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// This ability is blocked from activating if the source actor has any of these tags
    /// </summary>
    public interface IActivationBlockedTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// This ability can only be activated if the source has all of these tags.
    /// Usually, this will be the same as ActivationRequiredTags
    /// </summary>
    public interface ISourceRequiredTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// This ability is blocked from activating if the source has any of these tags.
    /// Usually, this will be the same as ActivationBlockedTags
    /// </summary>
    public interface ISourceBlockedTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// This ability can only be activated if the target has all of these tags.
    /// </summary>
    public interface ITargetRequiredTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// This ability is blocked from activating if the source has any of these tags.
    /// </summary>
    public interface ITargetBlockedTagsBufferElement : IGameplayTagBufferElement { }

    /*********************************************************
    These gameplay abilities are defined by gameplay effects
    **********************************************************/

    /// <summary>
    /// These tags define the gameplay effect
    /// </summary>
    public interface IGameplayEffectAssetTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// These tags are given to the target actor, and removed when the gameplay effect expires
    /// </summary>
    public interface IGrantedTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// The target actor must have all of these tags for the gameplay effect to be active.
    /// If the target actor does not meet this requirement, the gameplay effect is "disabled"
    /// (but continues counting down) until the actor meets the condition, at which point the
    /// gameplay effect is "enabled".
    /// </summary>
    public interface IOngoingTagsRequirementsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// The target actor must have all of these tags for application of the gameplay effect
    /// </summary>
    public interface IApplicationTagRequirementsBufferElement : IGameplayTagBufferElement { }
    public interface IGrantedApplicationImmunityTagsBufferElement : IGameplayTagBufferElement { }

    /// <summary>
    /// Removes gameplay effects on the target that own or grant any of these tags
    /// </summary>
    public interface IRemoveGameplayEffectsWithTagsBufferElement : IGameplayTagBufferElement { }


    // These are the gameplay tags currently on the player
    public interface IActorOwnedGameplayTags : IGameplayTagBufferElement { }

}

