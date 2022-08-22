using UnityEngine;

public class FreezeInputsInState : StateMachineBehaviour {
	PlayerController playerController;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (playerController == null) {
			playerController = animator.GetComponent<PlayerController>();
		}
		playerController.FreezeInputs();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		playerController.UnfreezeInputs();
	}
}
