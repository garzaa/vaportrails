using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallKick : StateMachineBehaviour {
    EntityController e;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!e) e = animator.GetComponent<EntityController>();
		e.OnWallKickExit();
    }
}
