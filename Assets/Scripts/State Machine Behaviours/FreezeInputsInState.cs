using UnityEngine;

public class FreezeInputsInState : StateMachineBehaviour {
	EntityController entityController;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (entityController == null) {
			entityController = animator.GetComponent<EntityController>();
		}
		entityController.FreezeInputs();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		entityController.UnfreezeInputs();
	}
}
