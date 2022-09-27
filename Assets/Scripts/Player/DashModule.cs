using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EntityController))]
public class DashModule : MonoBehaviour {
	[SerializeField] AudioResource dashSound;

	EntityController entity;
	PlayerInput input;
	GroundData groundData;

	public bool canDash { get; private set; }

	void Awake() {
		if (dashSound == null) dashSound = Resources.Load<AudioResource>("Runtime/DashSound");
		entity = GetComponent<EntityController>();
		input = GetComponent<PlayerInput>();
		groundData = GetComponent<GroundCheck>().groundData;
		EnableDash();
	}

	void Update() {
		if (entity.stunned) return;

		if (!canDash) return;

		if (entity.frozeInputs && !entity.inAttack) return;

		if (input.ButtonDown(Buttons.SPECIAL) && entity.canDash && input.HasHorizontalInput() && input.VerticalInput()<0.5) {
			entity.DashIfPossible(dashSound);
		}
	}

	public void DisableDash() {
		canDash = false;
	}

	public void EnableDash() {
		canDash = true;
	}
}
