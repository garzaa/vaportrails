using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ValController : EntityController {
	[SerializeField] SpriteRenderer heldItem;

	protected override void UpdateAnimator() {
		base.UpdateAnimator();
		if (groundData.hitGround) {
			HairForwards();
		}
		if (Input.GetKeyDown(KeyCode.T)) {
			animator.Play("ValPickupAbility");
			this.WaitAndExecute(() => animator.SetTrigger("ReturnToIdle"), 3f);
		}
	}

	public void HairBackwards() {
		animator.SetTrigger("HairBackwards");
	}

	public void HairForwards() {
		animator.SetTrigger("HairForwards");
	}

	public void FlourishItem(Sprite item) {
		heldItem.sprite = item;
		animator.Play("ValFlourishItem");
	}
	
	public void HandItem(Sprite item) {
		heldItem.sprite = item;
		animator.Play("ValHandItem");
	}
}
