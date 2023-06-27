using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneInState : StateMachineBehaviour {
	Entity player;
	public bool exitOnStateExit = true;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		player = PlayerInput.GetPlayerOneInput().GetComponent<Entity>();
		player.EnterCutscene(animator.gameObject);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (exitOnStateExit) player.ExitCutscene(animator.gameObject);
	}
}
