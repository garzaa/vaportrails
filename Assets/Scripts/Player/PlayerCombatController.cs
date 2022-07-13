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
	public PlayerAttackGraph airAttackGraph;

	bool canFlipKick = true;

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
		airAttackGraph.Initialize(
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
					EnterAttackGraph(groundAttackGraph);
				} else if (!wallData.touchingWall) {
					EnterAttackGraph(airAttackGraph);
				}
			} else if (
				(!player.frozeInputs || currentGraph!=null)
				&& InputManager.ButtonDown(Buttons.SPECIAL)
				&& !wallData.touchingWall
			) {
				// dash should take priority over everything else
				// then orca flip only if there's a sizeable up input
				if (InputManager.VerticalInput() > 0.5) {
					FlipKick();
				}

				// then meteor only if there's barely any down input
			}
		}

		if (currentGraph != null) {
			currentGraph.Update();
			if (wallData.hitWall) {
				currentGraph.ExitGraph();
			}
			if (groundData.hitGround) {
				RefreshAirAttacks();
			}
		}

		if (groundData.hitGround || wallData.hitWall) {
			RefreshAirAttacks();
		}

		if (combatLayerWeight == 0) {
			animator.SetLayerWeight(1, Mathf.MoveTowards(animator.GetLayerWeight(1), combatLayerWeight, 4*Time.deltaTime));
		}
	}

	public void FlipKick() {
		if (!groundData.grounded) {
			if (!canFlipKick) return;
			canFlipKick = false;
			player.DisableShortHop();
			// just enter the ground attack graph at the orca flip node? h mmm...need that impulse
			rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(rb2d.velocity.y, player.jumpSpeed));
			animator.Play("OrcaFlip");
		}
	}

	public void EnterAttackGraph(PlayerAttackGraph graph, CombatNode entryNode=null) {
		Debug.Log("entering graph "+graph.name);
		player.OnAttackGraphEnter();
		currentGraph = graph;
		StartAttackStance();
		graph.EnterGraph(entryNode);
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

	public void RefreshAirAttacks() {
		canFlipKick = true;
	}
}
