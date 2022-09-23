using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoDashCancel : StateMachineBehaviour {
	DashModule dashModule;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!dashModule) dashModule = animator.GetComponent<DashModule>();
		dashModule.DisableDash();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		dashModule.EnableDash();
	}
}
