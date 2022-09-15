using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackBehaviour : StateMachineBehaviour {
	PlayerSnapshotInfo snapshotInfo;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!snapshotInfo) snapshotInfo = animator.GetComponent<PlayerSnapshotInfo>();
		if (snapshotInfo) snapshotInfo.inAttack = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (snapshotInfo) snapshotInfo.inAttack = false;
	}
}
