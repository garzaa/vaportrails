using UnityEngine;

public class GroundAttackGraphBehaviour : StateMachineBehaviour {

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.GetComponent<PlayerController>().OnAttackNodeEnter();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.GetComponent<PlayerController>().OnAttackGraphExit();
	}
}

