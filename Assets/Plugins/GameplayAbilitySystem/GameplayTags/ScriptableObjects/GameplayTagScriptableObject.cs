/*
 * Created on Tue Jan 07 2020
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


using System;
using GameplayAbilitySystem.GameplayTags.Components;
using Unity.Collections;
using UnityEngine;
namespace GameplayAbilitySystem.GameplayTags.ScriptableObjects {
    [CreateAssetMenu(fileName = "GameplayTag", menuName = "Gameplay Ability System/Gameplay Tag")]
    public class GameplayTagScriptableObject : ScriptableObject {
        public GameplayTagBuilder GameplayTag;
    }

    [Serializable]
    public struct GameplayTagBuilder {
        public Level0GameplayTagScriptableObject Level0Tag;
        public Level1GameplayTagScriptableObject Level1Tag;
        public Level2GameplayTagScriptableObject Level2Tag;
        public Level3GameplayTagScriptableObject Level3Tag;
        public GameplayTagComponent GameplayTagComponent => new GameplayTagComponent
        {
            TagIdLevel0 = Level0Tag == null ? (byte)0 : Level0Tag.Tag,
            TagIdLevel1 = Level1Tag == null ? (byte)0 : Level1Tag.Tag,
            TagIdLevel2 = Level2Tag == null ? (byte)0 : Level2Tag.Tag,
            TagIdLevel3 = Level3Tag == null ? (byte)0 : Level3Tag.Tag
        };

    }
}