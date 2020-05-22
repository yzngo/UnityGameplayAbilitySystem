using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyEquippableBehaviour : StateMachineBehaviour {
    public bool AllowWeaponEquip;
    public bool AllowWeaponUnequip;

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool("LetWeaponEquip", AllowWeaponEquip);
        animator.SetBool("LetWeaponUnequip", AllowWeaponUnequip);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // animator.SetBool("LetWeaponEquip", false);
        // animator.SetBool("LetWeaponUnequip", false);
    }

}
