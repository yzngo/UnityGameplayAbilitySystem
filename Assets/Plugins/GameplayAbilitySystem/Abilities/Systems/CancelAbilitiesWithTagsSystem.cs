/*
 * Created on Fri Feb 07 2020
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

using Unity.Entities;
using Unity.Jobs;

/// <summary>
/// This system cancels active abilities on an actor when a new ability is activated by that actor.
/// The Gameplay Tags to check for are defined in the "Cancel Abilities With Tags", and the comparison
/// is done on the abilities that are currently marked as active (with the "AbilityIsActive" component tag).
/// 
/// This system will only run on abilities that are recently activated
/// </summary>
public class CancelAbilitiesWithTagsSystem : JobComponentSystem {
    protected override void OnCreate() {

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        // Iterate through each active ability, and collect list of tags that need to be cancelled.

        // 
        return inputDeps;
        // throw new System.NotImplementedException();
    }
}