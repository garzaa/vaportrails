using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AttackBuffer))]
public class CombatController : MonoBehaviour, IAttackLandListener, IHitListener {	
	#pragma warning disable 0649
	[SerializeField] AttackHitbox hitbox;
	[SerializeField] AttackHitbox altHitbox;
	#pragma warning restore 0649

	protected WallCheckData wallData;
	protected Rigidbody2D rb2d;
	protected GroundData groundData;
	protected EntityController player;
	protected Animator animator;
	public AttackGraph currentGraph = null;
	protected PlayerInput input;
	AttackBuffer buffer;

	public AttackGraph groundAttackGraph;
	public AttackGraph airAttackGraph;

	public float diStrength = 2f;

	const float techWindow = 0.3f;
	const float techLockoutLength = 0.6f;
	bool canTech = false;
	bool techLockout = false;
	GameObject techEffect;
	Collider2D collider2d;

	AirAttackTracker airAttackTracker = new AirAttackTracker();

	protected virtual void Start() {
		player = GetComponent<EntityController>();
		groundData = GetComponent<GroundCheck>().groundData;
		rb2d = GetComponent<Rigidbody2D>();
		wallData = GetComponent<WallCheck>().wallData;
		animator = GetComponent<Animator>();
		input = GetComponent<PlayerInput>();
		collider2d = GetComponent<Collider2D>();
		buffer = GetComponent<AttackBuffer>();

		techEffect = Resources.Load<GameObject>("Runtime/TechEffect");

		groundAttackGraph.Initialize(
			this,
			animator,
			buffer,
			airAttackTracker,
			input
		);
		airAttackGraph.Initialize(
			this,
			animator,
			buffer,
			airAttackTracker,
			input
		);
	}

	public void OnAttackLand(AttackData attack, Hurtbox hurtbox) {
		player.DisableShortHop();
		if (attack.hasSelfKnockback) {
			Vector2 v = attack.selfKnockback;
			if (v.x == 0) {
				v.x = rb2d.velocity.x;
			} else {
				v.x *= player.ForwardScalar();
			}
			rb2d.velocity = v;
		}
		if (currentGraph) currentGraph.OnAttackLand(attack, hurtbox);
	}

	protected virtual void Update() {
		CheckAttackInputs();

		if (currentGraph != null) {
			currentGraph.UpdateGrounded(groundData.grounded);
			currentGraph.Update();
		}

		if (groundData.hitGround || wallData.hitWall) {
			RefreshAirAttacks();
		}

		if (!techLockout && player.stunned && !canTech) {
			if (input.ButtonDown(Buttons.SPECIAL)) {
				canTech = true;
				Invoke(nameof(EndTechWindow), techWindow);
			}
		}

		CheckForTech();
	}

	public virtual void CheckAttackInputs() {
		if (player.frozeInputs || currentGraph != null) {
			return;
		}

		if (input.ButtonDown(Buttons.PUNCH) || input.ButtonDown(Buttons.KICK)) {
			if (groundData.grounded) {
				EnterAttackGraph(groundAttackGraph);
			} else if (!groundData.grounded && !wallData.touchingWall) {
				EnterAttackGraph(airAttackGraph);
			}
		}
	}

	void CheckForTech() {
		if (player.stunned && (groundData.hitGround || wallData.hitWall)) {
			if (!techLockout && canTech) {
				OnTech();
			}
		}
	}

	protected virtual void OnTech() {
		if (wallData.touchingWall) {
			rb2d.velocity = Vector2.zero;
			player.RefreshAirMovement();
			RefreshAirAttacks();
			Instantiate(
				techEffect,
				transform.position + new Vector3(wallData.direction * collider2d.bounds.extents.x, 0, 0),
				Quaternion.identity,
				null
			);
		} else if (groundData.grounded) {
			rb2d.velocity = new Vector2(
				player.movement.runSpeed * Mathf.Sign(input.HorizontalInput()),
				0
			);
			Instantiate(
				techEffect,
				transform.position + Vector3.down*collider2d.bounds.extents.y,
				Quaternion.identity,
				null
			);
		}
		animator.SetTrigger("TechSuccess");
		GetComponent<EntityShader>().FlashCyan();
		canTech = false;
		CancelInvoke(nameof(EndTechWindow));
		player.CancelStun();
	}

	public void EndGroundStunAnimation() {
		if (Mathf.Abs(input.HorizontalInput()) > 0.2f) {
			rb2d.velocity = new Vector2(
				player.movement.runSpeed * Mathf.Sign(input.HorizontalInput()),
				0
			);
			animator.SetTrigger("TechSuccess");
			GetComponent<EntityShader>().FlashCyan();
			player.CancelStun();
		}
	}

	void EndTechWindow() {
		canTech = false;
		techLockout = true;
		this.WaitAndExecute(() => techLockout = false, techLockoutLength);
	}

	public void OnHit(AttackHitbox attack) {
		DI(attack);
	}

	void DI(AttackHitbox attack) {
		if (attack.data.GetKnockback(attack, this.gameObject).sqrMagnitude < 1) {
			return;
		}
		// sideways DI is stronger than towards/away
		// (sin(2x - (1/4 circle))) * 0.4 + 0.6
		// ↑ this is a sinewave between 0.2 and 1.0 that peaks at (1, 0) and (-1, 0)
		Vector2 selfKnockback = player.GetKnockback(attack);
		Vector2 leftStick = input.LeftStick();
		float angle = Vector2.SignedAngle(selfKnockback, leftStick);
		float diMagnitude = (Mathf.Cos(angle * Mathf.Deg2Rad)* 0.4f) + 0.6f;
		rb2d.velocity += leftStick * diMagnitude * diStrength * attack.data.diRatio;
	}

	public virtual void EnterAttackGraph(AttackGraph graph, CombatNode entryNode=null) {
		player.OnAttackGraphEnter();
		currentGraph = graph;
		graph.EnterGraph(entryNode);
	}

	public void OnAttackNodeEnter(CombatNode combatNode) {
		if (combatNode is AttackNode) {
			AttackNode attackNode = combatNode as AttackNode;
			player.OnAttackNodeEnter(attackNode.attackData);
			hitbox.data = attackNode.attackData;
			if (altHitbox) {
				altHitbox.data = attackNode?.attackData?.altHitbox;
			}
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

	public float GetSpeed() {
		return Mathf.Abs(rb2d.velocity.x);
	}

	public bool IsSpeeding() {
		return player.IsSpeeding();
	}

	virtual public void RefreshAirAttacks() {}
}
