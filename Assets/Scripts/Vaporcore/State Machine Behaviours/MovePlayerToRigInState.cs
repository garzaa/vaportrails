using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovePlayerToRigInState : StateMachineBehaviour {
	GameObject player;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		Transform rig = animator.transform.Find("PlayerRig");
		player = PlayerInput.GetPlayerOneInput().gameObject;
		player.transform.position = rig.position;
		player.gameObject.SetActive(false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		player.gameObject.SetActive(true);
	}
}
