using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ValController : EntityController {
	[SerializeField] SpriteRenderer heldItem;
	[SerializeField] Animator healthBarAnimator;

	protected override void UpdateAnimator() {
		base.UpdateAnimator();
		if (groundData.hitGround) {
			HairForwards();
		}
		if (Input.GetKeyDown(KeyCode.T)) {
			animator.Play("ValPickupAbility");
			this.WaitAndExecute(() => animator.SetTrigger("ResetToIdle"), 3f);
		}
	}

    protected override void OnEffectGroundHit(float fallDistance) {
        base.OnEffectGroundHit(fallDistance);
		if (fallDistance > 7f) {
			animator.Play("ValHardLanding");
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

    public override void OnHit(AttackHitbox hitbox) {
        base.OnHit(hitbox);
		if (input.isHuman && GetComponent<HP>().current.Get() <= 0) {
			if (GameOptions.SecondWind) {
				FindObjectOfType<PlayerDeath>().Run(hitbox);
				animator.Play("ValDie");
				return;
			}
			CancelInvoke();
			EndHitstop();
			Die();
			animator.Play("ValDie");
			FindObjectOfType<PlayerDeath>().Run(hitbox);
		}
		healthBarAnimator.SetTrigger("OnHit");
    }
}
