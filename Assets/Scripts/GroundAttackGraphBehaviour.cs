using UnityEngine;

public class GroundAttackGraphBehaviour : StateMachineBehaviour {

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//Debug.Log("entered state with clip "+animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
	}
}

