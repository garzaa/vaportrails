using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ValController : EntityController {
	protected override void UpdateAnimator() {
		base.UpdateAnimator();
		if (groundData.hitGround) {
			HairForwards();

		}
	}

	public void HairBackwards() {
		animator.SetTrigger("HairBackwards");
	}

	public void HairForwards() {
		animator.SetTrigger("HairForwards");
	}
}
