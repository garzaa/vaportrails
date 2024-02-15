using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EntityController))]
public class DashModule : MonoBehaviour {
	[SerializeField] AudioResource dashSound;

	EntityController entity;
	PlayerInput input;
	Rewired.Player rewiredPlayer;
	GroundData groundData;

	public bool canDash { get; private set; }

	void Awake() {
		if (dashSound == null) dashSound = Resources.Load<AudioResource>("Runtime/DashSound");
		entity = GetComponent<EntityController>();
		input = GetComponent<PlayerInput>();
		groundData = GetComponent<GroundCheck>().groundData;
		EnableDash();
		rewiredPlayer = Rewired.ReInput.players.GetPlayer(0);
	}

	void Update() {
		if (!entity.HasAbility(Ability.Dash)) return;

		if (entity.stunned) return;

		if (!canDash) return;

		bool inCancelableAttack = entity.inAttack && entity.GetAttack().moveCancelable;
		if (entity.frozeInputs && !inCancelableAttack) return;

		if (input.ButtonDown(RewiredConsts.Action.Dash) && entity.CanDash && Mathf.Abs(input.HorizontalInput()) > 0.5 && input.VerticalInput() < 0.5) {
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
