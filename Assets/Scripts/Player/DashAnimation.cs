using UnityEngine;

public class DashAnimation : StateMachineBehaviour {
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.GetComponent<EntityController>().StopDashAnimation();
	}
}
