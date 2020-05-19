/*
 * Created on Mon Dec 23 2019
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameplayAbilitySystem.AbilitySystem.Components;
using MyGameplayAbilitySystem.AbilitySystem.MonoBehaviours;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem.InputSystem;

[RequireComponent(typeof(ActorAbilitySystem))]
public class ActorCast : MonoBehaviour, ICastActions, IEquipActions {

    private InputSystem.InputSystem controls;
    private ActorAbilitySystem actorAbilitySystem;

    private EntityQuery grantedAbilityQuery;

    public List<int> CastAbilityButtonMap = new List<int>();

    public GameObject Fire1Prefab;

    private Animator animator;

    private WeaponHandler weaponHandler;

    List<Entity> GrantedAbilityEntities;
    // List<(IAbilityTagComponent AbilityTag, ComponentType ComponentType, Entity GrantedAbilityEntity)> GrantedAbilities;
    // Start is called before the first frame update

    void Start() {
        actorAbilitySystem = GetComponent<ActorAbilitySystem>();
        if (controls == null) {
            controls = new InputSystem.InputSystem();
            controls.Cast.SetCallbacks(this);
            controls.Equip.SetCallbacks(this);
        }
        controls.Cast.Enable();
        controls.Equip.Enable();

        GetAllAbilities();
        // Get list of grantedAbility entities
        GetGrantedAbilities(World.DefaultGameObjectInjectionWorld.EntityManager);

        this.animator = GetComponent<Animator>();
        this.weaponHandler = GetComponent<WeaponHandler>();
    }

    public void OnCast1(InputAction.CallbackContext context) {
        if (!context.performed) return;
        Cast(1);
    }

    public void OnCast2(InputAction.CallbackContext context) {
        if (!context.performed) return;
        Cast(2);
    }

    public void OnCast3(InputAction.CallbackContext context) {
        if (!context.performed) return;
        Cast(3);
    }

    public void OnCast4(InputAction.CallbackContext context) {
        if (!context.performed) return;
        Cast(4);
    }

    public void OnCast5(InputAction.CallbackContext context) {
        if (!context.performed) return;
        Cast(5);
    }

    public void OnCast6(InputAction.CallbackContext context) {
        if (!context.performed) return;
        Cast(6);
    }

    public void OnCast7(InputAction.CallbackContext context) {
        if (!context.performed) return;
        Cast(7);
    }

    public void OnCast8(InputAction.CallbackContext context) {
        if (!context.performed) return;
        Cast(8);
    }

    public void Update() {
        weaponHandler.IsWeaponEquipped = animator.GetBool("IsWeaponEquipped");
    }



    void GetAllAbilities() {
        //var abilityComponentTypes = AbilityManager.AbilityComponentTypes();
    }

    /// <summary>
    /// Get list of granted abilities and store them
    /// </summary>
    /// <param name="entityManager">Entity Manager</param>
    void GetGrantedAbilities(EntityManager entityManager) {
        // MethodInfo methodInfo = typeof(EntityManager).GetMethod("GetComponentData");
        // if (GrantedAbilities == null) GrantedAbilities = new List<(IAbilityTagComponent AbilityTag, ComponentType ComponentType, Entity GrantedAbilityEntity)>();

        // grantedAbilityQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<AbilityOwnerComponent>(), ComponentType.ReadOnly<AbilityStateFlags>());
        // var grantedAbilityEntities = grantedAbilityQuery.ToEntityArrayAsync(Allocator.TempJob, out var jobHandle);
        // jobHandle.Complete();
        // var abilities = AbilityManager.AbilityComponentTypes().ToList();
        // for (var i = 0; i < grantedAbilityEntities.Length; i++) {
        //     var abilityOwner = entityManager.GetComponentData<AbilityOwnerComponent>(grantedAbilityEntities[i]);
        //     // We need to do this only for abilities that have been granted to the actor that has this script.  Others should be ignored
        //     if (abilityOwner != actorAbilitySystem.AbilityOwnerEntity) continue;

        //     var abilityState = entityManager.GetComponentData<AbilityStateFlags>(grantedAbilityEntities[i]);
        //     var grantedAbilityEntity = grantedAbilityEntities[i];

        //     for (var iComponentType = 0; iComponentType < abilities.Count; iComponentType++) {

        //         if (entityManager.HasComponent(grantedAbilityEntity, abilities[iComponentType])) {
        //             // Create instance of the ability to store locally, cast to the IAbilityTagComponent interface, so we can call things on it
        //             IAbilityTagComponent abilityTagComponent = (IAbilityTagComponent)Activator.CreateInstance(abilities[iComponentType].GetManagedType()); ;
        //             GrantedAbilities.Add((abilityTagComponent, abilities[iComponentType], grantedAbilityEntity));
        //         }
        //     }
        // }
        // grantedAbilityEntities.Dispose();
    }



    void Cast(int abilityId) {
        // Find this ability in the GrantedAbilities array
        // var ability = GrantedAbilities.Find(x => x.AbilityTag.AbilityIdentifier == abilityId);
        // if (ability.AbilityTag == null) return;
        // var untypedPayload = ability.AbilityTag.EmptyPayload;
        // switch (untypedPayload) {
        //     case BasicMeleeAbilityPayload payload:
        //         payload.ActorAbilitySystem = this.actorAbilitySystem;
        //         payload.ActorTransform = this.transform;
        //         payload.EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //         payload.GrantedAbilityEntity = ability.GrantedAbilityEntity;
        //         StartCoroutine(ability.AbilityTag.DoAbility(payload));
        //         break;
        //     case BasicRangeAbilityPayload payload:
        //         payload.ActorAbilitySystem = this.actorAbilitySystem;
        //         payload.ActorTransform = this.transform;
        //         payload.EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //         payload.GrantedAbilityEntity = ability.GrantedAbilityEntity;
        //         payload.AbilityPrefab = Fire1Prefab;
        //         StartCoroutine(ability.AbilityTag.DoAbility(payload));
        //         break;
        // }

    }

    public void OnToggleEquip(InputAction.CallbackContext context) {
        if (!context.performed) return;
        animator.SetBool("IsWeaponEquipped", !weaponHandler.IsWeaponEquipped);
    }

}
