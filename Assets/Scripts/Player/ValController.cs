using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ValController : EntityController, IAttackLandListener {

	protected AudioResource dashSound;

	protected override void Awake() {
		base.Awake();
		dashSound = Resources.Load<AudioResource>("Runtime/DashSound");
	}

	override protected void Update() {
		base.Update();
		Dash();
	}


	void Dash() {
		void EndDashCooldown() {
			if (canDash) return;
			entityShader.FlashCyan();
			canDash = true;
		}

		if (frozeInputs && !currentAttack) return;

		if (input.ButtonDown(Buttons.SPECIAL) && canDash && input.HasHorizontalInput() && input.VerticalInput()<0.5) {
			if (!groundData.grounded && currentAirDashes <= 0) return;
			dashSound.PlayFrom(gameObject);
			animator.SetTrigger(inputBackwards ? "BackDash" : "Dash");
			entityShader.FlashWhite();
			canDash = false;
			dashing = true;
			fMod = 0;
			// dash at the max direction indicated by the stick
			// if already moving in that way, make it additive
			float speed = movement.runSpeed+movement.dashSpeed;
			if ((inputForwards && movingForwards) || (inputBackwards && movingBackwards)) {
				speed = Mathf.Max(Mathf.Abs(rb2d.velocity.x)+movement.dashSpeed, speed);
			}
			rb2d.velocity = new Vector2(
				speed * Mathf.Sign(input.HorizontalInput()),
				Mathf.Max(rb2d.velocity.y, 0)
			);
			if (!groundData.grounded) currentAirDashes--;
			this.WaitAndExecute(EndDashCooldown, movement.dashCooldown);
		}
	}
}
