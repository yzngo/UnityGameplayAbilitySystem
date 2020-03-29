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

using System.Collections;
using GameplayAbilitySystem.AbilitySystem.GameplayEffects.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace GameplayAbilitySystem.AbilitySystem.GameplayEffects.Systems {
    public delegate void GameplayEffectExpiredEventHandler(object sender, GameplayEffectExpiredEventArgs e);
    public struct GameplayEffectExpiredEventArgs {
        public Entity GameplayEffectEntity;
        public GameplayEffectDurationSpec Spec;
        public GameplayEffectIdentifier Id;
        public GameplayEffectActorSpec ActorSpec;
    }

    public class GameplayEffectExpiredEventManager : AbilitySystemEventManager<int, GameplayEffectExpiredEventArgs> {
        public override int KeyFromArgs(GameplayEffectExpiredEventArgs e) {
            return e.Id.Value;
        }
    }

    public class GameplayEffectExpiredOnTargetEventManager : AbilitySystemEventManager<Entity, GameplayEffectExpiredEventArgs> {
        public override Entity KeyFromArgs(GameplayEffectExpiredEventArgs e) {
            return e.ActorSpec.Target;
        }
    }

    public class GameplayEffectExpiredOnSourceEventManager : AbilitySystemEventManager<Entity, GameplayEffectExpiredEventArgs> {
        public override Entity KeyFromArgs(GameplayEffectExpiredEventArgs e) {
            return e.ActorSpec.Source;
        }
    }

    [UpdateInGroup(typeof(GameplayEffectGroupUpdateBeginSystem))]
    public class GameplayEffectDurationUpdateSystem : SystemBase {
        public GameplayEffectExpiredEventManager GameplayEffectExpired;
        public GameplayEffectExpiredOnTargetEventManager GameplayEffectExpiredOnTarget;
        public GameplayEffectExpiredOnSourceEventManager GameplayEffectExpiredOnSource;
        private EntityQuery query;
        /// Using event system based on Code Monkey tutorial on ECS events https://youtu.be/fkJ-7pqnRGo
        private NativeQueue<GameplayEffectExpiredEventArgs> gameplayEffectExpiredQueue;

        private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
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
                Debug.Log("GE with ID: " + e.Id.Value + " expired.");
            };
        }

        protected override void OnCreate() {
            m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            gameplayEffectExpiredQueue = new NativeQueue<GameplayEffectExpiredEventArgs>(Allocator.Persistent);
            GameplayEffectExpired = new GameplayEffectExpiredEventManager();
            GameplayEffectExpiredOnSource = new GameplayEffectExpiredOnSourceEventManager();
            GameplayEffectExpiredOnTarget = new GameplayEffectExpiredOnTargetEventManager();
            Test();
        }

        protected override void OnUpdate() {
            // var gameplayEffectsExpiredArray = new NativeArray<GameplayEffectExpiredEventArgs>(query.CalculateEntityCount(), Allocator.TempJob);
            // var gameplayEffectsExpiredQueue = new NativeQueue<GameplayEffectExpiredEventArgs>(Allocator.TempJob);
            var gameplayEffectsExpiredList = new NativeList<GameplayEffectExpiredEventArgs>(query.CalculateEntityCount(), Allocator.TempJob);
            var gameplayEffectsExpiredListParallel = gameplayEffectsExpiredList.AsParallelWriter();
            var deltaTime = Time.DeltaTime;

            // Substract time from duration remaining, and get a list of all GameplayEffects that have expired.
            // This only operates on entities which need to tick with time
            Entities
                .WithName("DurationUpdate_Tick")
                .WithAll<Tag.GameplayEffectTickWithTime>()
                .ForEach
                    ((Entity entity, int entityInQueryIndex, ref GameplayEffectDurationRemaining durationRemaining, in GameplayEffectDurationSpec durationSpec, in GameplayEffectIdentifier id) => {
                        durationRemaining.Value -= deltaTime;
                        if (durationRemaining.Value <= 0) {
                            gameplayEffectsExpiredListParallel.AddNoResize(new GameplayEffectExpiredEventArgs() { GameplayEffectEntity = entity, Id = id, Spec = durationSpec });
                        }
                    }
                    )
                .WithStoreEntityQueryInField(ref query)
                .ScheduleParallel();

            // This is for manually ticking from an external system (e.g. for turn based ticking)
            // The actual ticking happens externally.  This marks the GE ready for removal if duration <= 0.
            // Entities
            //     .WithName("DurationUpdate_Manual")
            //     .WithAll<Tag.GameplayEffectTickWithCustom>()
            //     .ForEach(
            //             (Entity entity, int entityInQueryIndex, ref GameplayEffectDurationRemaining durationRemaining, in GameplayEffectDurationSpec durationSpec, in GameplayEffectIdentifier id) => {
            //                 if (durationRemaining.Value <= 0) {
            //                     gameplayEffectsExpiredArray[entityInQueryIndex] = new GameplayEffectExpiredEventArgs() { GameplayEffectEntity = entity, Id = id, Spec = durationSpec };
            //                 } else {
            //                     gameplayEffectsExpiredArray[entityInQueryIndex] = new GameplayEffectExpiredEventArgs() { GameplayEffectEntity = Entity.Null };
            //                 }
            //             }
            //     )
            //     .WithStoreEntityQueryInField(ref query)
            //     .ScheduleParallel();


            var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer();

            // Remove entity
            Job
                .WithName("DestroyEntity")
                .WithCode(() => {
                    for (var i = 0; i < gameplayEffectsExpiredList.Length; i++) {
                        var args = gameplayEffectsExpiredList[i];
                        if (args.GameplayEffectEntity != Entity.Null) {
                            ecb.DestroyEntity(args.GameplayEffectEntity);
                        }
                    }

                })
                .Schedule();

            m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

            // Raise event
            Job
                .WithoutBurst()
                .WithCode(() => {
                    for (var i = 0; i < gameplayEffectsExpiredList.Length; i++) {
                        var e = gameplayEffectsExpiredList[i];
                        // GE specific event
                        GameplayEffectExpired[e.Id.Value].RaiseEvent(ref e);

                        // General GE event
                        GameplayEffectExpired[-1].RaiseEvent(ref e);

                        // Raise target/source actor events
                        GameplayEffectExpiredOnSource[e.ActorSpec.Source].RaiseEvent(ref e);
                        GameplayEffectExpiredOnTarget[e.ActorSpec.Target].RaiseEvent(ref e);
                    }
                })
                .Run();

            //gameplayEffectsExpiredArray.Dispose(this.Dependency);
            gameplayEffectsExpiredList.Dispose(this.Dependency);
        }

        protected override void OnDestroy() {
            gameplayEffectExpiredQueue.Dispose();
            GameplayEffectExpired.Dispose();
            GameplayEffectExpiredOnSource.Dispose();
            GameplayEffectExpiredOnTarget.Dispose();
        }
    }
}