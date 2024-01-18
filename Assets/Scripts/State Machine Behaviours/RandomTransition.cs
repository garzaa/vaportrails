using UnityEngine;

public class RandomTransition : StateMachineBehaviour {
	public int triggers;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.SetTrigger("Random"+(1+Random.Range(0, triggers)));
	}
}
