using UnityEngine;

public class TumbleState : StateMachineBehaviour {
	Entity entity;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (entity == null) {
			entity = animator.GetComponent<Entity>();
		}
		animator.SetBool("Tumbling", true);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		entity.LeaveTumbleAnimation();
	}
}
