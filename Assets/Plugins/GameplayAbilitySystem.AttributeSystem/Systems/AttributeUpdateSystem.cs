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
    public struct AttributeChangedEventArgs {
        public Entity Actor;
        public AttributeBufferElement[] OldAttribute;
        public AttributeBufferElement[] NewAttribute;
    }
    public class AttributeChangedEventManager : AbilitySystemEventManager<Entity, AttributeChangedEventArgs, AttributeChangedEventArgs> {
        public override Entity KeyFromArgs(AttributeChangedEventArgs e) {
            return e.Actor;
        }
    }
    [UpdateInGroup(typeof(AttributeGroupUpdateBeginSystem))]
    public class AttributeUpdateSystem : SystemBase {
        private EntityQuery m_Query;
        public int nAttributes = 0;
        private int nOperators = 3;

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

            AttributeChanged[entities[0]].OnEvent += (o, e) => {
                Debug.Log(e.NewAttribute.Length);

            };

            entities.Dispose();
        }

        protected override void OnCreate() {
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

            var modifiedAttributesEntities = new NativeArray<Entity>(nEntities * nAttributes, Allocator.TempJob);
            var modifiedAttributesOld = new NativeArray<AttributeBufferElement>(nEntities * nAttributes, Allocator.TempJob);
            var modifiedAttributesNew = new NativeArray<AttributeBufferElement>(nEntities * nAttributes, Allocator.TempJob);

            AttributeUpdateJob(_nAttributes, _nOperators, attributeModifierArray, modifiedAttributesEntities, modifiedAttributesOld, modifiedAttributesNew);

            Job
                .WithoutBurst()
                .WithCode(() => {
                    // Iterate through, grouped by attribute
                    // for (var attributeIndex = 0; attributeIndex < _nAttributes; attributeIndex++) {
                    //     // the attributeIndex index is the starting offset
                    //     // We increment by [_nAttributes] until we get to the end to collect all instances of a changed attribute

                    // }

                    // Iterate through, grouped by actor
                    for (var entityArrayOffset = 0; entityArrayOffset < nEntities; entityArrayOffset += _nAttributes) {

                        var actorEntity = modifiedAttributesEntities[entityArrayOffset];

                        // If the first element is an Entity.Null, this attribute hasn't changed.
                        if (actorEntity == Entity.Null) {
                            continue;
                        }

                        var eventArgs = new AttributeChangedEventArgs
                        {
                            Actor = actorEntity,
                            NewAttribute = modifiedAttributesNew.Slice(entityArrayOffset, _nAttributes).ToArray(),
                            OldAttribute = modifiedAttributesOld.Slice(entityArrayOffset, _nAttributes).ToArray()
                        };

                        AttributeChanged[actorEntity].RaiseEvent(eventArgs);

                    }
                })
                .Run();

            modifiedAttributesEntities.Dispose(this.Dependency);
            modifiedAttributesOld.Dispose(this.Dependency);
            modifiedAttributesNew.Dispose(this.Dependency);
            attributeModifierArray.Dispose(this.Dependency);
        }

        private void AttributeUpdateJob(int _nAttributes, int _nOperators, NativeArray<float> attributeModifierArray, NativeArray<Entity> modifiedAttributesEntities, NativeArray<AttributeBufferElement> modifiedAttributesOld, NativeArray<AttributeBufferElement> modifiedAttributesNew) {
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
                    // Iterate 0 -> N-1
                    for (var i = 0; i < attributeBuffer.Length; i++) {
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
                        }

                        var oldAttribute = attributeElement;
                        var arrayOffset = entityArrayOffset + i;
                        attributeElement.CurrentValue = newValue;
                        attributeBuffer[i] = attributeElement;
                        var newAttribute = attributeElement;
                        modifiedAttributesEntities[arrayOffset] = entity;
                        modifiedAttributesOld[arrayOffset] = oldAttribute;
                        modifiedAttributesNew[arrayOffset] = newAttribute;
                    }

                    // If no attributes changed for this actor, mark the first index as Entity.Null
                    // We will use this marker to tell us to not raise a change notification
                    if (!valueChanged) {
                        modifiedAttributesEntities[entityArrayOffset] = Entity.Null;
                    }
                })
                .ScheduleParallel();
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
    }
}