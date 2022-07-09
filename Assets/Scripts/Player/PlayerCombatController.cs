using UnityEngine;
using System.Collections.Generic;

public class PlayerCombatController : MonoBehaviour {
	PlayerController player;
	GroundData groundData;
	PlayerAttackGraph currentGraph;

	public PlayerAttackGraph groundAttackGraph;

	void Start() {
		player = GetComponent<PlayerController>();
		groundData = GetComponent<GroundCheck>().groundData;
		groundAttackGraph.Initialize(
			this,
			GetComponent<Animator>(),
			GetComponent<AttackBuffer>(),
			GetComponent<Rigidbody2D>(),
			GetComponent<AirAttackTracker>()
		);
	}

	void Update() {
		if (currentGraph == null) {
			if (InputManager.ButtonDown(Buttons.PUNCH) || InputManager.ButtonDown(Buttons.KICK)) {
				if (groundData.grounded) {
					groundAttackGraph.EnterGraph();
					currentGraph = groundAttackGraph;
				}
			}
		}
		if (currentGraph != null) {
			currentGraph.Update();
		}
	}

	public void OnAttackNodeEnter(CombatNode combatNode) {
		player.OnAttackNodeEnter();
	}

	public void OnAttackNodeExit() {
		player.OnAttackNodeExit();
	}

	public void OnGraphExit() {
		player.OnAttackNodeExit();
		currentGraph = null;
	}

	public void SetFriction(float f) {
		player.SetFmod(f);
	}
}
