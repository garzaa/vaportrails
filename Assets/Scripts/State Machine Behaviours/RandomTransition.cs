using UnityEngine;

public class RandomTransition : StateMachineBehaviour {
	public int triggers;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		string triggerName = "Random"+(1+Random.Range(0, triggers));
		animator.SetTrigger(triggerName);
	}
}
