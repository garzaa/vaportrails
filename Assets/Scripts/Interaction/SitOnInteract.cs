using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SitOnInteract : Interactable {
	public enum SitType {
		LEDGE,
		KNEEL,
	}

	public enum Direction {
		LEFT,
		RIGHT
	}

	public SitType sitType;
	public Direction direction;

	PlayerInput input;
	float sitTime;
	bool playerSitting = false;
	EntityController player;

	public override void OnInteract(EntityController player) {
		this.player = player;
		player.GetComponent<Animator>().Play(sitType==SitType.LEDGE ? "ValSittingLedge" : "ValSitting");
		if (direction == Direction.LEFT && player.facingRight) player.Flip();
		else if (direction == Direction.RIGHT && !player.facingRight) player.Flip();
		HidePrompt();
		input = player.GetComponent<PlayerInput>();
		player.EnterCutscene(this.gameObject);
		sitTime = Time.time;
		playerSitting = true;
	}

	void Update() {
		if (playerSitting && (Time.time > sitTime+1f) && input.HasHorizontalInput()) {
			playerSitting = false;
			player.ExitCutscene(this.gameObject);
			player.GetComponent<Animator>().SetTrigger("ResetToIdle");
			ShowPrompt();
		}
	}
}
