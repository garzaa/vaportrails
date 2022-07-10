using UnityEngine;
using System.Collections.Generic;

public class PlayerCombatController : MonoBehaviour {
	PlayerController player;
	GroundData groundData;
	PlayerAttackGraph currentGraph;
	Rigidbody2D rb2d;
	WallCheckData wallData;

	public PlayerAttackGraph groundAttackGraph;

	void Start() {
		player = GetComponent<PlayerController>();
		groundData = GetComponent<GroundCheck>().groundData;
		rb2d = GetComponent<Rigidbody2D>();
		wallData = GetComponent<WallCheck>().wallData;
		groundAttackGraph.Initialize(
			this,
			GetComponent<Animator>(),
			GetComponent<AttackBuffer>(),
			GetComponent<AirAttackTracker>()
		);
	}

	void Update() {
		if (currentGraph == null) {
			if (InputManager.ButtonDown(Buttons.PUNCH) || InputManager.ButtonDown(Buttons.KICK)) {
				if (groundData.grounded) {
					groundAttackGraph.EnterGraph();
					player.OnAttackGraphEnter();
					currentGraph = groundAttackGraph;
				}
			}
		}
		if (currentGraph != null) {
			currentGraph.Update();
			if (wallData.hitWall) {
				if (currentGraph != null) currentGraph.ExitGraph();
			}
		}
	}

	public void OnAttackNodeEnter(CombatNode combatNode) {
		player.OnAttackNodeEnter();
	}

	public void OnAttackNodeExit() {
		player.OnAttackNodeExit();
	}

	public void OnGraphExit() {
		// wait for the animation to finish to give the player back control
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

	public void SetMinSpeed(float minSpeed) {
		rb2d.velocity = new Vector2(
			Mathf.Max(Mathf.Abs(rb2d.velocity.x), minSpeed) * Mathf.Sign(rb2d.velocity.x),
			rb2d.velocity.y
		);
	}

	public bool IsSpeeding() {
		return player.IsSpeeding();
	}
}
