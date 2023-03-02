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
	protected EntityShader shader;
	protected PlayerInput input;
	AttackBuffer buffer;

	public AttackGraph groundAttackGraph;
	public AttackGraph airAttackGraph;

	protected AttackGraphTraverser graphTraverser;

	public float diStrength = 2f;

	Collider2D collider2d;

	protected virtual void Start() {
		player = GetComponent<EntityController>();
		groundData = GetComponent<GroundCheck>().groundData;
		rb2d = GetComponent<Rigidbody2D>();
		wallData = GetComponent<WallCheck>().wallData;
		animator = GetComponent<Animator>();
		input = GetComponent<PlayerInput>();
		collider2d = GetComponent<Collider2D>();
		buffer = GetComponent<AttackBuffer>();
		shader = GetComponent<EntityShader>();
		
		graphTraverser = new AttackGraphTraverser(this);
	}

	public void OnAttackLand(AttackData attack, Hurtbox hurtbox) {
		player.DisableShortHop();
		if (attack.hasSelfKnockback) {
			Vector2 v = attack.selfKnockback;
			if (v.x == 0) {
				v.x = rb2d.velocity.x;
			} else {
				v.x *= player.Forward();
			}
			rb2d.velocity = v;
		}
		player.DoHitstop(attack.hitstop, rb2d.velocity);
		graphTraverser.OnAttackLand(attack, hurtbox);
	}

	protected virtual void Update() {
		CheckAttackInputs();

		graphTraverser.Update();

		if (groundData.hitGround || wallData.hitWall) {
			RefreshAirAttacks();
		}
	}

	public virtual void CheckAttackInputs() {
		if (player.frozeInputs || graphTraverser.InGraph()) {
			return;
		}

		if (buffer.Ready()) {
			if (groundData.grounded) {
				EnterAttackGraph(groundAttackGraph);
			} else if (!groundData.grounded && !wallData.touchingWall) {
				EnterAttackGraph(airAttackGraph);
			}
		}
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

	public virtual void OnTech() {}

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
		graphTraverser.EnterGraph(graph, entryNode);
	}

	public virtual void OnCombatNodeEnter(CombatNode combatNode) {
		if (combatNode is AttackNode) {
			AttackNode attackNode = combatNode as AttackNode;
			player.OnAttackNodeEnter(attackNode);
			hitbox.data = attackNode.attackData;
			if (altHitbox) {
				altHitbox.data = attackNode?.attackData?.altHitbox;
			}
		} else {
			player.OnAttackNodeEnter(null);
		}
	}

	public void OnAttackGraphExit() {
		player.OnAttackGraphExit();
	}

	public float GetSpeed() {
		return Mathf.Abs(rb2d.velocity.x);
	}

	public bool IsSpeeding() {
		return player.IsSpeeding();
	}

	virtual public void RefreshAirAttacks() {}

	public void LandingLag(float time) {
		animator.Play("LandingLag", 0);
		animator.SetBool("LandingLag", true);
		player.FreezeInputs();
		this.WaitAndExecute(
			() => {
				animator.SetBool("LandingLag", false);
				player.UnfreezeInputs();
			},
			time
		);
	}
}
