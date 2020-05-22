using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamStateBehaviour : StateMachineBehaviour {
    public SetParamStateData[] EntryParamStateData;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        for (var i = 0; i < EntryParamStateData.Length; i++) {
            var param = EntryParamStateData[i];
            animator.SetBool(param.paramName, param.setDefaultState);
        }
    }

    [Serializable]
    public struct SetParamStateData {
        public string paramName;
        public bool setDefaultState;
    }
}
