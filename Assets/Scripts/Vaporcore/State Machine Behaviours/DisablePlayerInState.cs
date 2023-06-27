using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisablePlayerInState : StateMachineBehaviour {
	GameObject player;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		player = PlayerInput.GetPlayerOneInput().transform.Find("PlayerRig").gameObject;
		player.gameObject.SetActive(false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		player.gameObject.SetActive(true);
	}
}
