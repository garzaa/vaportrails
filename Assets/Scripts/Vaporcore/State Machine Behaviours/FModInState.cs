using UnityEngine;

public class FModInState : StateMachineBehaviour {
	public float fmod;
	PlayerController player;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		player = animator.GetComponent<PlayerController>();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		player.SetFmod(fmod);
	}
}
