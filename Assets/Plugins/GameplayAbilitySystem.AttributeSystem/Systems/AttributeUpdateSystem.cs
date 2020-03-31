/*
 * Created on Sun Mar 29 2020
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

using System.Collections.Generic;
using GameplayAbilitySystem.AttributeSystem._Systems;
using GameplayAbilitySystem.AttributeSystem.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameplayAbilitySystem.AttributeSystem.Systems {
    public struct ActorAttributeChangedEventArgs {
        public Entity Actor;
        public AttributeBufferElement[] OldAttribute;
        public AttributeBufferElement[] NewAttribute;
    }

    public struct AttributeChangedEventArgs {
        public int Attribute;
        public Entity[] Actor;
        public AttributeBufferElement[] OldAttribute;
        public AttributeBufferElement[] NewAttribute;
    }

    public class ActorAttributeChangedEventManager : AbilitySystemEventManager<Entity, ActorAttributeChangedEventArgs, ActorAttributeChangedEventArgs> {
        public override Entity KeyFromArgs(ActorAttributeChangedEventArgs e) {
            return e.Actor;
        }
    }

    public class AttributeChangedEventManager : AbilitySystemEventManager<int, AttributeChangedEventArgs, AttributeChangedEventArgs> {
        public override int KeyFromArgs(AttributeChangedEventArgs e) {
            return e.Attribute;
        }
    }

    [UpdateInGroup(typeof(AttributeGroupUpdateBeginSystem))]
    public class AttributeUpdateSystem : SystemBase {
        private EntityQuery m_Query;
        public int nAttributes = 0;
        private int nOperators = 3;

        public ActorAttributeChangedEventManager ActorAttributeChanged;
        public AttributeChangedEventManager AttributeChanged;

        private void Test() {
            this.nAttributes = 5;

            var em = this.EntityManager;
            var archetype = em.CreateArchetype(
                typeof(AttributeBufferElement),
                typeof(AttributeModifierBufferElement)
            );

            var entityCount = 5000;

            var entities = new NativeArray<Entity>(entityCount, Allocator.Temp);
            em.CreateEntity(archetype, entities);
            var startWorldTime = Time.ElapsedTime;
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(1);
            for (int i = 0; i < entities.Length; i++) {
                var attributeBuffer = em.GetBuffer<AttributeBufferElement>(entities[i]);
                var attributeModifierBuffer = em.GetBuffer<AttributeModifierBufferElement>(entities[i]);

                attributeBuffer.Add(new AttributeBufferElement() { BaseValue = 100, CurrentValue = 100 });
                attributeBuffer.Add(new AttributeBufferElement() { BaseValue = 100, CurrentValue = 100 });
                attributeBuffer.Add(new AttributeBufferElement() { BaseValue = 20, CurrentValue = 20 });
                attributeBuffer.Add(new AttributeBufferElement() { BaseValue = 20, CurrentValue = 20 });
                attributeBuffer.Add(new AttributeBufferElement() { BaseValue = 5, CurrentValue = 5 });

                for (var j = 0; j < nOperators; j++) {
                    for (var k = 0; k < this.nAttributes; k++) {
                        var randFloat1 = random.NextFloat(0, 1);
                        attributeModifierBuffer.Add(new AttributeModifierBufferElement() { AttributeId = k, ModifierValue = randFloat1, OperatorId = j });
                    }
                }
            }

            ActorAttributeChanged[entities[0]].OnEvent += (o, e) => {
                Debug.Log(e.NewAttribute.Length);

            };

            entities.Dispose();
        }

        protected override void OnCreate() {
            ActorAttributeChanged = new ActorAttributeChangedEventManager();
            AttributeChanged = new AttributeChangedEventManager();
            Test();
        }
        protected override void OnUpdate() {
            var _nAttributes = nAttributes;
            var _nOperators = nOperators;
            var nEntities = m_Query.CalculateEntityCount();
            // Reserve enough space for each modifier for each attribute for each entity
            var maxArrayLength = _nAttributes * nOperators * nEntities;
            var attributeModifierArray = new NativeArray<float>(maxArrayLength, Allocator.TempJob);

            GatherAttributesJob(_nAttributes, _nOperators, maxArrayLength, attributeModifierArray);

            var modifiedAttributesEntities_ByEntity = new NativeArray<Entity>(nEntities * nAttributes, Allocator.TempJob);
            var modifiedAttributesOld_ByEntity = new NativeArray<AttributeBufferElement>(nEntities * nAttributes, Allocator.TempJob);
            var modifiedAttributesNew_ByEntity = new NativeArray<AttributeBufferElement>(nEntities * nAttributes, Allocator.TempJob);

            var modifiedAttributesEntities_ByAttribute = new NativeArray<Entity>(nEntities * nAttributes, Allocator.TempJob);
            var modifiedAttributesOld_ByAttribute = new NativeArray<AttributeBufferElement>(nEntities * nAttributes, Allocator.TempJob);
            var modifiedAttributesNew_ByAttribute = new NativeArray<AttributeBufferElement>(nEntities * nAttributes, Allocator.TempJob);

            AttributeUpdateJob(_nAttributes, _nOperators, attributeModifierArray,
                                modifiedAttributesEntities_ByEntity, modifiedAttributesOld_ByEntity, modifiedAttributesNew_ByEntity,
                                modifiedAttributesEntities_ByAttribute, modifiedAttributesOld_ByAttribute, modifiedAttributesNew_ByAttribute
                                );

            RaiseEvents(_nAttributes, nEntities, modifiedAttributesEntities_ByEntity, modifiedAttributesOld_ByEntity, modifiedAttributesNew_ByEntity, modifiedAttributesEntities_ByAttribute, modifiedAttributesOld_ByAttribute, modifiedAttributesNew_ByAttribute);

            modifiedAttributesEntities_ByEntity.Dispose(this.Dependency);
            modifiedAttributesOld_ByEntity.Dispose(this.Dependency);
            modifiedAttributesNew_ByEntity.Dispose(this.Dependency);
            modifiedAttributesEntities_ByAttribute.Dispose(this.Dependency);
            modifiedAttributesOld_ByAttribute.Dispose(this.Dependency);
            modifiedAttributesNew_ByAttribute.Dispose(this.Dependency);
            attributeModifierArray.Dispose(this.Dependency);
        }

        private void RaiseEvents(int _nAttributes, int nEntities, NativeArray<Entity> modifiedAttributesEntities_ByEntity, NativeArray<AttributeBufferElement> modifiedAttributesOld_ByEntity, NativeArray<AttributeBufferElement> modifiedAttributesNew_ByEntity, NativeArray<Entity> modifiedAttributesEntities_ByAttribute, NativeArray<AttributeBufferElement> modifiedAttributesOld_ByAttribute, NativeArray<AttributeBufferElement> modifiedAttributesNew_ByAttribute) {
            Job
                .WithoutBurst()
                .WithCode(() => {
                    // Iterate through, grouped by attribute
                    for (var attributeIndex = 0; attributeIndex < _nAttributes; attributeIndex++) {
                        var calculatedArrayOffset = attributeIndex * nEntities;
                        // the attributeIndex index is the starting offset
                        // We increment by [_nAttributes] until we get to the end to collect all instances of a changed attribute
                        var eventArgs = new AttributeChangedEventArgs()
                        {
                            Attribute = attributeIndex,
                            Actor = modifiedAttributesEntities_ByAttribute.Slice(calculatedArrayOffset, nEntities).ToArray(),
                            OldAttribute = modifiedAttributesOld_ByAttribute.Slice(calculatedArrayOffset, nEntities).ToArray(),
                            NewAttribute = modifiedAttributesNew_ByAttribute.Slice(calculatedArrayOffset, nEntities).ToArray()
                        };

                        AttributeChanged[attributeIndex].RaiseEvent(eventArgs);
                    }

                    // Iterate through, grouped by actor
                    for (var entityArrayOffset = 0; entityArrayOffset < nEntities; entityArrayOffset += _nAttributes) {

                        var actorEntity = modifiedAttributesEntities_ByEntity[entityArrayOffset];

                        // If the first element is an Entity.Null, this attribute hasn't changed.
                        if (actorEntity == Entity.Null) {
                            continue;
                        }

                        var eventArgs = new ActorAttributeChangedEventArgs
                        {
                            Actor = actorEntity,
                            NewAttribute = modifiedAttributesNew_ByEntity.Slice(entityArrayOffset, _nAttributes).ToArray(),
                            OldAttribute = modifiedAttributesOld_ByEntity.Slice(entityArrayOffset, _nAttributes).ToArray()
                        };

                        ActorAttributeChanged[actorEntity].RaiseEvent(eventArgs);

                    }
                })
                .Run();
        }

        private void GatherAttributesJob(int _nAttributes, int _nOperators, int maxArrayLength, NativeArray<float> attributeModifierArray) {
            Entities
                .WithName("AttributeGather")
                //.WithoutBurst()
                .ForEach((Entity entity, int entityInQueryIndex, DynamicBuffer<AttributeBufferElement> attributeBuffer, DynamicBuffer<AttributeModifierBufferElement> attributeModifierBuffer) => {
                    var entityOffset = _nAttributes * _nOperators * entityInQueryIndex;

                    // First, collect all additions, multipliers, dividers for each attribute id
                    for (var i = 0; i < attributeModifierBuffer.Length; i++) {
                        /******* ATTRIBUTE ID *******/
                        // Attribute ID: 0 - Health
                        // Attribute ID: 1 - MaxHealth
                        // Attribute ID: 2 - Mana
                        // Attribute ID: 3 - MaxMana
                        // Attribute ID: 4 - SpeedMana
                        /*****************************/

                        /******** OPERATOR ID *******/
                        // Attribute ID: 0 - Add
                        // Attribute ID: 1 - Multiply
                        // Attribute ID: 2 - Divide
                        /*****************************/

                        var modifierElement = attributeModifierBuffer[i];
                        if (modifierElement.AttributeId >= _nAttributes) continue;
                        if (modifierElement.AttributeId < 0) continue;
                        if (modifierElement.OperatorId >= _nOperators) continue;
                        if (modifierElement.OperatorId < 0) continue;
                        var attributeOffset = modifierElement.AttributeId * _nOperators;
                        var modifierOffset = entityOffset + attributeOffset + modifierElement.OperatorId;
                        if (modifierOffset > maxArrayLength - 1) continue;

                        attributeModifierArray[modifierOffset] += modifierElement.ModifierValue;
                    }
                })
                .WithStoreEntityQueryInField(ref m_Query)
                .ScheduleParallel();
        }

        private void AttributeUpdateJob(int _nAttributes, int _nOperators, NativeArray<float> attributeModifierArray,
                                    NativeArray<Entity> modifiedAttributesEntities_ByEntity, NativeArray<AttributeBufferElement> modifiedAttributesOld_ByEntity, NativeArray<AttributeBufferElement> modifiedAttributesNew_ByEntity,
                                    NativeArray<Entity> modifiedAttributesEntities_ByAttribute, NativeArray<AttributeBufferElement> modifiedAttributesOld_ByAttribute, NativeArray<AttributeBufferElement> modifiedAttributesNew_ByAttribute
                                    ) {
            Entities
                .WithName("AttributeUpdate")
                //.WithoutBurst()
                .ForEach((Entity entity, int entityInQueryIndex, DynamicBuffer<AttributeBufferElement> attributeBuffer, DynamicBuffer<AttributeModifierBufferElement> attributeModifierBuffer) => {
                    var entityOffset = _nAttributes * _nOperators * entityInQueryIndex;

                    /******* ATTRIBUTE ID *******/
                    // Attribute ID: 0 - Health
                    // Attribute ID: 1 - MaxHealth
                    // Attribute ID: 2 - Mana
                    // Attribute ID: 3 - MaxMana
                    // Attribute ID: 4 - SpeedMana
                    /*****************************/

                    /******** OPERATOR ID *******/
                    // Attribute ID: 0 - Add
                    // Attribute ID: 1 - Multiply
                    // Attribute ID: 2 - Divide
                    /*****************************/
                    var valueChanged = false;

                    var entityArrayOffset = entityInQueryIndex * _nAttributes;
                    var iterateLength = math.min(_nAttributes, attributeBuffer.Length);
                    // Iterate 0 -> N-1
                    for (var i = 0; i < iterateLength; i++) {
                        var thisValueChanged = false;
                        var attributeElement = attributeBuffer[i];
                        var attributeOffset = (i) * _nOperators;
                        var addModifierOffset = entityOffset + attributeOffset + 0;
                        var multiplyModifierOffset = entityOffset + attributeOffset + 1;
                        var divideModifierOffset = entityOffset + attributeOffset + 2;
                        var addValue = attributeModifierArray[addModifierOffset];
                        var multiplyValue = attributeModifierArray[multiplyModifierOffset];
                        var divideValue = attributeModifierArray[divideModifierOffset];

                        // Sanity check on multiply values
                        multiplyValue = math.select(0f, multiplyValue, multiplyValue > 0);
                        divideValue = math.select(0f, divideValue, divideValue > 0);

                        var newValue = ((attributeElement.BaseValue + addValue) * (1 + multiplyValue) / (1 + divideValue));
                        if (newValue != attributeElement.CurrentValue) {
                            valueChanged = true;
                            thisValueChanged = true;
                        }

                        var oldAttribute = attributeElement;
                        var arrayOffset_ByEntity = entityArrayOffset + i;
                        var arrayOffset_ByAttribute = i * _nAttributes + entityInQueryIndex;
                        attributeElement.CurrentValue = newValue;
                        attributeBuffer[i] = attributeElement;
                        var newAttribute = attributeElement;

                        modifiedAttributesEntities_ByEntity[arrayOffset_ByEntity] = entity;
                        modifiedAttributesOld_ByEntity[arrayOffset_ByEntity] = oldAttribute;
                        modifiedAttributesNew_ByEntity[arrayOffset_ByEntity] = newAttribute;

                        modifiedAttributesEntities_ByAttribute[arrayOffset_ByAttribute] = entity;
                        modifiedAttributesOld_ByAttribute[arrayOffset_ByAttribute] = oldAttribute;
                        modifiedAttributesNew_ByAttribute[arrayOffset_ByAttribute] = newAttribute;


                    }

                    // If no attributes changed for this actor, mark the first index as Entity.Null
                    // We will use this marker to tell us to not raise a change notification
                    if (!valueChanged) {
                        modifiedAttributesEntities_ByEntity[entityArrayOffset] = Entity.Null;
                    }
                })
                .ScheduleParallel();
        }

    }
}