/*
 * Created on Fri Dec 13 2019
 *
 * The MIT License (MIT)
 * Copyright (c) 2019 Sahil Jain
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

using GameplayAbilitySystem.GameplayEffects.Components;
using GameplayAbilitySystem.GameplayEffects.Interfaces;
using Unity.Entities;

namespace MyGameplayAbilitySystem.GameplayEffects.Components {
    public struct PoisonGameplayEffectComponent : IGameplayEffectTagComponent, IComponentData {
        public Entity Instantiate(EntityManager dstManager, Entity actorEntity, float duration) {
            var archetype = dstManager.CreateArchetype(
                                    typeof(GameplayEffectDurationComponent),
                                    typeof(GameplayEffectTargetComponent),
                                    this.GetType());

            var effectEntity = dstManager.CreateEntity(archetype);
            dstManager.SetComponentData<GameplayEffectTargetComponent>(effectEntity, actorEntity);
            dstManager.SetComponentData<GameplayEffectDurationComponent>(effectEntity, GameplayEffectDurationComponent.Initialise(duration, UnityEngine.Time.time));
            return effectEntity;
        }

        public Entity Instantiate(int jobIndex, EntityCommandBuffer.Concurrent Ecb, Entity actorEntity, float duration) {
            throw new System.NotImplementedException();
        }
    }
}
