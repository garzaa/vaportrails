using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoFlip : StateMachineBehaviour {

    Entity e;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (!e) e = animator.GetComponent<Entity>();
		e.DisableFlip();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        e.EnableFlip();
    }
}
