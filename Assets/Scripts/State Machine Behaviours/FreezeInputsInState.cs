using UnityEngine;

public class FreezeInputsInState : StateMachineBehaviour {
	EntityController EntityController;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (EntityController == null) {
			EntityController = animator.GetComponent<EntityController>();
		}
		EntityController.FreezeInputs();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		EntityController.UnfreezeInputs();
	}
}
