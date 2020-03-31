/*
 * Created on Mon Nov 04 2019
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

using System;
using System.Collections;
using System.Collections.Generic;
using GameplayAbilitySystem.AbilitySystem.GameplayEffects.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace GameplayAbilitySystem.AbilitySystem.GameplayEffects.Systems {
    public struct GameplayEffectExpiredEventArgs {
        public Entity GameplayEffectEntity;
        public GameplayEffectDurationSpec Spec;
        public GameplayEffectIdentifier Id;
        public GameplayEffectActorSpec ActorSpec;
    }

    public class GameplayEffectExpiredEventManager : AbilitySystemEventManager<int, GameplayEffectExpiredEventArgs, List<GameplayEffectExpiredEventArgs>> {
        public override int KeyFromArgs(GameplayEffectExpiredEventArgs e) {
            return e.Id.Value;
        }
    }

    public class GameplayEffectExpiredOnTargetEventManager : AbilitySystemEventManager<Entity, GameplayEffectExpiredEventArgs, List<GameplayEffectExpiredEventArgs>> {
        public override Entity KeyFromArgs(GameplayEffectExpiredEventArgs e) {
            return e.ActorSpec.Target;
        }
    }

    public class GameplayEffectExpiredOnSourceEventManager : AbilitySystemEventManager<Entity, GameplayEffectExpiredEventArgs, List<GameplayEffectExpiredEventArgs>> {
        public override Entity KeyFromArgs(GameplayEffectExpiredEventArgs e) {
            return e.ActorSpec.Source;
        }
    }

    [UpdateInGroup(typeof(GameplayEffectGroupUpdateBeginSystem))]
    public class GameplayEffectDurationUpdateSystem : SystemBase {
        /// <summary>
        /// Enables raising of C# events 
        /// </summary>
        /// <value></value>
        public bool EnableEvents { get; set; }
        public GameplayEffectExpiredEventManager GameplayEffectExpired;
        public GameplayEffectExpiredOnTargetEventManager GameplayEffectExpiredOnTarget;
        public GameplayEffectExpiredOnSourceEventManager GameplayEffectExpiredOnSource;
        private EntityQuery query;
        /// Using event system based on Code Monkey tutorial on ECS events https://youtu.be/fkJ-7pqnRGo
        private NativeQueue<GameplayEffectExpiredEventArgs> gameplayEffectExpiredQueue;

        private BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
        private void Test() {
            var em = this.EntityManager;
            var archetype = em.CreateArchetype(
                typeof(GameplayEffectIdentifier),
                typeof(GameplayEffectDurationRemaining),
                typeof(GameplayEffectDurationSpec),
                typeof(Tag.GameplayEffectTickWithTime)
            );

            var entityCount = 50000;

            var entities = new NativeArray<Entity>(entityCount, Allocator.Temp);
            em.CreateEntity(archetype, entities);
            var startWorldTime = Time.ElapsedTime;
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(1);
            for (int i = 0; i < entities.Length; i++) {
                var duration = random.NextFloat(0, 30);
                var id = random.NextInt(0, entityCount / 100 + 1);
                if (i < entityCount / 4) duration = 0.01f;
                em.SetComponentData(entities[i], new GameplayEffectDurationSpec() { StartWorldTime = startWorldTime, Duration = duration });
                em.SetComponentData(entities[i], new GameplayEffectDurationRemaining() { Value = duration });
                em.SetComponentData(entities[i], new GameplayEffectIdentifier() { Value = id });
            }
            GameplayEffectExpired[5].OnEvent += (o, e) => {
                var eList = (e as List<GameplayEffectExpiredEventArgs>);
                // Debug.Log("[" + eList.Count +  "] GE with ID: " + eList[0].Id.Value + " expired.");
            };
        }

        protected override void OnCreate() {
            m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            gameplayEffectExpiredQueue = new NativeQueue<GameplayEffectExpiredEventArgs>(Allocator.Persistent);
            GameplayEffectExpired = new GameplayEffectExpiredEventManager();
            GameplayEffectExpiredOnSource = new GameplayEffectExpiredOnSourceEventManager();
            GameplayEffectExpiredOnTarget = new GameplayEffectExpiredOnTargetEventManager();
            this.EnableEvents = true;
            Test();
        }

        public void ResetEvents() {
            GameplayEffectExpired.Reset();
            GameplayEffectExpiredOnSource.Reset();
            GameplayEffectExpiredOnTarget.Reset();
        }

        protected override void OnUpdate() {
            var gameplayEffectsExpiredList = new NativeList<GameplayEffectExpiredEventArgs>(query.CalculateEntityCount(), Allocator.TempJob);
            var gameplayEffectsExpiredListParallel = gameplayEffectsExpiredList.AsParallelWriter();
            var deltaTime = Time.DeltaTime;

            var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
            // Substract time from duration remaining, and get a list of all GameplayEffects that have expired.
            // This only operates on entities which need to tick with time
            DurationUpdateTickAndRemoveJob(gameplayEffectsExpiredListParallel, deltaTime, ecb);

            if (this.EnableEvents) {
                RaiseGameplayEffectEventsJob(gameplayEffectsExpiredList);
            }

            //gameplayEffectsExpiredArray.Dispose(this.Dependency);
            gameplayEffectsExpiredList.Dispose(this.Dependency);
            m_EntityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);

        }

        private void DurationUpdateTickAndRemoveJob(NativeList<GameplayEffectExpiredEventArgs>.ParallelWriter gameplayEffectsExpiredListParallel, float deltaTime, EntityCommandBuffer.Concurrent ecb) {
            Entities
                .WithName("DurationUpdate_Tick")
                .WithAll<Tag.GameplayEffectTickWithTime>()
                .ForEach
                    ((Entity entity, int entityInQueryIndex, ref GameplayEffectDurationRemaining durationRemaining, in GameplayEffectDurationSpec durationSpec, in GameplayEffectIdentifier id) => {
                        durationRemaining.Value -= deltaTime;
                        if (durationRemaining.Value <= 0) {
                            gameplayEffectsExpiredListParallel.AddNoResize(new GameplayEffectExpiredEventArgs() { GameplayEffectEntity = entity, Id = id, Spec = durationSpec });
                            ecb.DestroyEntity(entityInQueryIndex, entity);
                        }
                    }
                    )
                .WithStoreEntityQueryInField(ref query)
                .ScheduleParallel();
        }

        private void RaiseGameplayEffectEventsJob(NativeList<GameplayEffectExpiredEventArgs> gameplayEffectsExpiredList) {
            // Raise event
            Job
                .WithoutBurst()
                .WithCode(() => {
                    if (gameplayEffectsExpiredList.Length == 0) return;
                    var eventsPerGameplayEffectId = new NativeMultiHashMap<int, GameplayEffectExpiredEventArgs>(gameplayEffectsExpiredList.Length, Allocator.Temp);
                    var eventsPerGameplayEffectTarget = new NativeMultiHashMap<Entity, GameplayEffectExpiredEventArgs>(gameplayEffectsExpiredList.Length, Allocator.Temp);
                    var eventsPerGameplayEffectSource = new NativeMultiHashMap<Entity, GameplayEffectExpiredEventArgs>(gameplayEffectsExpiredList.Length, Allocator.Temp);

                    // Collect and group all expired gameplay effects
                    for (var i = 0; i < gameplayEffectsExpiredList.Length; i++) {
                        var e = gameplayEffectsExpiredList[i];
                        // // GE specific event
                        // GameplayEffectExpired[e.Id.Value].RaiseEvent(ref e);
                        eventsPerGameplayEffectId.Add(e.Id.Value, e);

                        // // General GE event
                        // GameplayEffectExpired[-1].RaiseEvent(ref e);

                        // // Raise target/source actor events
                        // GameplayEffectExpiredOnSource[e.ActorSpec.Source].RaiseEvent(ref e);
                        // GameplayEffectExpiredOnTarget[e.ActorSpec.Target].RaiseEvent(ref e);
                        eventsPerGameplayEffectSource.Add(e.ActorSpec.Source, e);
                        eventsPerGameplayEffectTarget.Add(e.ActorSpec.Target, e);
                    }

                    // Ungroup and raise event for each unique key in NMHM
                    RaiseEvents<int, GameplayEffectExpiredEventArgs, GameplayEffectExpiredEventManager>(eventsPerGameplayEffectId, GameplayEffectExpired);
                    RaiseEvents<Entity, GameplayEffectExpiredEventArgs, GameplayEffectExpiredOnSourceEventManager>(eventsPerGameplayEffectSource, GameplayEffectExpiredOnSource);
                    RaiseEvents<Entity, GameplayEffectExpiredEventArgs, GameplayEffectExpiredOnTargetEventManager>(eventsPerGameplayEffectTarget, GameplayEffectExpiredOnTarget);

                    eventsPerGameplayEffectId.Dispose();
                    eventsPerGameplayEffectTarget.Dispose();
                    eventsPerGameplayEffectSource.Dispose();
                })
                .Run();
        }

        private void RaiseEvents<T1, T2, T3>(NativeMultiHashMap<T1, T2> eventsToRaise, T3 eventManager)
        where T1 : struct, IEquatable<T1>, IComparable<T1>
        where T2 : struct
        where T3 : AbilitySystemEventManager<T1, T2, List<T2>> {
            (var gameplayEffectIdKey, var gameplayEffectIdKeyLength) = eventsToRaise.GetUniqueKeyArray(Allocator.Temp);
            for (var i = 0; i < gameplayEffectIdKeyLength; i++) {
                var key = gameplayEffectIdKey[i];
                var values = eventsToRaise.GetValuesForKey(key);
                var size = eventsToRaise.CountValuesForKey(key);
                var groupedArray = new List<T2>(size);
                while (values.MoveNext()) {
                    var current = values.Current;
                    groupedArray.Add(current);
                }
                eventManager[key].RaiseEvent(groupedArray);
            }
        }

        protected override void OnDestroy() {
            gameplayEffectExpiredQueue.Dispose();
            GameplayEffectExpired.Reset();
            GameplayEffectExpiredOnSource.Reset();
            GameplayEffectExpiredOnTarget.Reset();
        }
    }
}