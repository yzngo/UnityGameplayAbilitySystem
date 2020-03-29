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
    }

    public class GameplayEffectExpiredEventManager : AbilitySystemEventManager<int, GameplayEffectExpiredEventArgs> {
        public override int KeyFromArgs(GameplayEffectExpiredEventArgs e) {
            return e.Id.Value;
        }
    }

    [UpdateInGroup(typeof(GameplayEffectGroupUpdateBeginSystem))]
    public class GameplayEffectDurationUpdateSystem : SystemBase {
        public GameplayEffectExpiredEventManager GameplayEffectExpired;
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

            var entityCount = 500;

            var entities = new NativeArray<Entity>(entityCount, Allocator.Temp);
            em.CreateEntity(archetype, entities);
            var startWorldTime = Time.ElapsedTime;
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(1);
            for (int i = 0; i < entities.Length; i++) {
                var duration = random.NextFloat(0, 30);
                var id = random.NextInt(1, 10);
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
            //Test();
        }

        protected override void OnUpdate() {
            var gameplayEffectsArray = new NativeArray<GameplayEffectExpiredEventArgs>(query.CalculateEntityCount(), Allocator.TempJob);
            var gameplayEffectExpiredQueueLocal = gameplayEffectExpiredQueue;
            var gameplayEffectExpiredQueueLocalConcurrent = gameplayEffectExpiredQueue.AsParallelWriter();
            var deltaTime = Time.DeltaTime;
            // Substract time from duration remaining, and get a list of all GameplayEffects that have expired.
            Entities
                .WithName("DurationUpdate")
                .WithAll<Tag.GameplayEffectTickWithTime>()
                .ForEach(
                        (Entity entity, int entityInQueryIndex, ref GameplayEffectDurationRemaining durationRemaining, in GameplayEffectDurationSpec durationSpec, in GameplayEffectIdentifier id) => {
                            durationRemaining.Value -= deltaTime;

                            if (durationRemaining.Value <= 0) {
                                gameplayEffectsArray[entityInQueryIndex] = new GameplayEffectExpiredEventArgs() { GameplayEffectEntity = entity, Id = id, Spec = durationSpec };
                            } else {
                                gameplayEffectsArray[entityInQueryIndex] = new GameplayEffectExpiredEventArgs() { GameplayEffectEntity = Entity.Null };
                            }
                        }
                )
                .WithStoreEntityQueryInField(ref query)
                .ScheduleParallel();


            var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer();

            // Remove entity
            Job
                .WithName("DestroyEntity")
                .WithCode(() => {
                    for (var i = 0; i < gameplayEffectsArray.Length; i++) {
                        var args = gameplayEffectsArray[i];
                        if (args.GameplayEffectEntity != Entity.Null) {
                            ecb.DestroyEntity(args.GameplayEffectEntity);
                            gameplayEffectExpiredQueueLocalConcurrent.Enqueue(args);
                        }
                    }

                })
                .Schedule();

            // Raise event
            Job
                .WithoutBurst()
                .WithCode(() => {
                    while (gameplayEffectExpiredQueueLocal.TryDequeue(out var e)) {
                        // GE specific event
                        GameplayEffectExpired[e.Id.Value].RaiseEvent(ref e);

                        // General GE event
                        GameplayEffectExpired[-1].RaiseEvent(ref e);
                    }
                })
                .Run();

            gameplayEffectsArray.Dispose(this.Dependency);
            m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        }

        protected override void OnDestroy() {
            gameplayEffectExpiredQueue.Dispose();
        }
    }
}