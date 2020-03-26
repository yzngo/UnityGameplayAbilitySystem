using System.Collections;
using System.Collections.Generic;
using GameplayAbilitySystem.Abilities.ScriptableObjects;
using UnityEngine;

public abstract class AbilityExecutionFlow : ScriptableObject {
    public abstract void Execute(AbilityTagsSpecScriptableObject ability);

}
