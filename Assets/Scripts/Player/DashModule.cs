using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EntityController))]
public class DashModule : MonoBehaviour {
	[SerializeField] AudioResource dashSound;

	EntityController entity;
	PlayerInput input;
	GroundData groundData;

	void Awake() {
		dashSound = Resources.Load<AudioResource>("Runtime/DashSound");
		entity = GetComponent<EntityController>();
		input = GetComponent<PlayerInput>();
		groundData = GetComponent<GroundCheck>().groundData;
	}

	void Update() {
		if (entity.frozeInputs && !entity.inAttack) return;

		if (input.ButtonDown(Buttons.SPECIAL) && entity.canDash && input.HasHorizontalInput() && input.VerticalInput()<0.5) {
			entity.DashIfPossible(dashSound);
		}
	}
}
