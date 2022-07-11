using UnityEngine;
using System.Collections.Generic;

public class PlayerCombatController : MonoBehaviour {
	const float combatStanceLength = 4f;
	float combatLayerWeight = 0f;

	PlayerController player;
	GroundData groundData;
	PlayerAttackGraph currentGraph;
	Rigidbody2D rb2d;
	WallCheckData wallData;
	Animator animator;

	public PlayerAttackGraph groundAttackGraph;

	void Start() {
		player = GetComponent<PlayerController>();
		groundData = GetComponent<GroundCheck>().groundData;
		rb2d = GetComponent<Rigidbody2D>();
		wallData = GetComponent<WallCheck>().wallData;
		animator = GetComponent<Animator>();
		groundAttackGraph.Initialize(
			this,
			animator,
			GetComponent<AttackBuffer>(),
			GetComponent<AirAttackTracker>()
		);
	}

	void Update() {
		if (currentGraph == null) {
			if (InputManager.ButtonDown(Buttons.PUNCH) || InputManager.ButtonDown(Buttons.KICK)) {
				if (groundData.grounded) {
					player.OnAttackGraphEnter();
					currentGraph = groundAttackGraph;
					StartAttackStance();
					groundAttackGraph.EnterGraph();
				}
			}
		}
		if (currentGraph != null) {
			currentGraph.Update();
			if (wallData.hitWall) {
				if (currentGraph != null) currentGraph.ExitGraph();
			}
		}

		if (combatLayerWeight == 0) {
			animator.SetLayerWeight(1, Mathf.MoveTowards(animator.GetLayerWeight(1), combatLayerWeight, 4*Time.deltaTime));
		}
	}

	public void OnAttackNodeEnter(CombatNode combatNode) {
		if (combatNode is AttackNode) {
			player.OnAttackNodeEnter((combatNode as AttackNode).attackData);
		} else {
			player.OnAttackNodeEnter(null);
		}
	}

	public void OnAttackNodeExit() {
		player.OnAttackNodeExit();
	}

	public void OnGraphExit() {
		player.OnAttackGraphExit();
		currentGraph = null;
	}

	public void SetFriction(float f) {
		player.SetFmod(f);
	}

	public void AddImpulse(Vector2 impulse) {
		rb2d.AddForce(impulse * player.Forward(), ForceMode2D.Impulse);
	}

	public float GetSpeed() {
		return Mathf.Abs(rb2d.velocity.x);
	}

	public bool IsSpeeding() {
		return player.IsSpeeding();
	}

	void StartAttackStance() {
		combatLayerWeight = 1;
		animator.SetLayerWeight(1, 1);
		if (IsInvoking(nameof(DisableAttackStance))) {
			CancelInvoke(nameof(DisableAttackStance));
		}
		Invoke(nameof(DisableAttackStance), combatStanceLength);
	}

	void DisableAttackStance() {
		combatLayerWeight = 0;
	}
}
