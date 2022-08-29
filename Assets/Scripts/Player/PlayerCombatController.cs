using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombatController : MonoBehaviour, IAttackLandListener, IHitListener {
	const float combatStanceLength = 4f;
	float combatLayerWeight = 0f;

	PlayerController player;
	GroundData groundData;
	PlayerAttackGraph currentGraph;
	Rigidbody2D rb2d;
	WallCheckData wallData;
	Animator animator;
	AttackHitbox attackHitbox;
	Gun gunEyes;
	PlayerTargetingSystem targetingSystem;
	PlayerInput input;

	public PlayerAttackGraph groundAttackGraph;
	public PlayerAttackGraph airAttackGraph;

	public SubscriptableInt currentEP;
	public SubscriptableInt maxEP;

	#pragma warning disable 0649
	[SerializeField] GameObject chargeIndicator;
	[SerializeField] AudioResource fullChargeSound;
	[SerializeField] AudioResource emptyChargeSound;
	#pragma warning restore 0649

	const string fullMessage = "FULLY CHARGED";
	const string emptyMessage = "SOLENOID EMPTY";

	public float diStrength = 3f;

	bool canFlipKick = true;

	const float techWindow = 0.2f;
	const float techLockoutLength = 0.3f;
	bool canTech = false;
	bool techLockout = false;
	GameObject techEffect;
	Collider2D collider2d;

	void Start() {
		player = GetComponent<PlayerController>();
		groundData = GetComponent<GroundCheck>().groundData;
		rb2d = GetComponent<Rigidbody2D>();
		wallData = GetComponent<WallCheck>().wallData;
		animator = GetComponent<Animator>();
		attackHitbox = GetComponentInChildren<AttackHitbox>();
		gunEyes = GetComponentInChildren<Gun>();
		targetingSystem = GetComponentInChildren<PlayerTargetingSystem>();
		input = GetComponent<PlayerInput>();
		collider2d = GetComponent<Collider2D>();
		groundAttackGraph.Initialize(
			this,
			animator,
			GetComponent<AttackBuffer>(),
			GetComponent<AirAttackTracker>(),
			input
		);
		airAttackGraph.Initialize(
			this,
			animator,
			GetComponent<AttackBuffer>(),
			GetComponent<AirAttackTracker>(),
			input
		);
		currentEP.Initialize();
		currentEP.OnChange.AddListener(OnEnergyChange);
		maxEP.Initialize();
		chargeIndicator.SetActive(false);
		techEffect = Resources.Load<GameObject>("Runtime/TechEffect");
	}

	public void OnAttackLand(Hurtbox hurtbox) {
		if (currentGraph) currentGraph.OnAttackLand();
	}

	public void OnEnergyChange(int energy) {
		targetingSystem.enabled = (energy > 0);
	}

	void Update() {
		if (!player.frozeInputs && currentGraph == null) {
			if (input.ButtonDown(Buttons.PUNCH) || input.ButtonDown(Buttons.KICK)) {
				if (groundData.grounded) {
					EnterAttackGraph(groundAttackGraph);
				} else if (!wallData.touchingWall) {
					EnterAttackGraph(airAttackGraph);
				}
			} else if (
				(!player.frozeInputs || currentGraph!=null)
				&& input.ButtonDown(Buttons.SPECIAL)
				&& !wallData.touchingWall
			) {
				// dash should take priority over everything else
				// then orca flip only if there's a sizeable up input
				if (input.VerticalInput() > 0.5) {
					FlipKick();
				}

				// then meteor only if there's barely any down input
			}
		}

		if (currentGraph != null) {
			currentGraph.UpdateGrounded(groundData.grounded);
			currentGraph.Update();
			if (wallData.hitWall) {
				if (currentGraph) currentGraph.ExitGraph();
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

		Shoot();

		if (!techLockout && player.stunned && !canTech) {
			if (input.ButtonDown(Buttons.SPECIAL)) {
				canTech = true;
				this.WaitAndExecute(
					() => canTech = false,
					techWindow
				);
				Invoke(nameof(EndTechWindow), techWindow);
			}
		}

		CheckForTech();
	}

	void CheckForTech() {
		if (!techLockout && player.stunned && canTech && (groundData.hitGround || wallData.hitWall)) {
			OnSuccessfulTech();
		}
	}

	void OnSuccessfulTech() {
		if (wallData.touchingWall) {
			rb2d.velocity = Vector2.zero;
			Instantiate(
				techEffect,
				transform.position + new Vector3(wallData.direction * collider2d.bounds.extents.x, 0, 0),
				Quaternion.identity,
				null
			);
		} else if (groundData.grounded) {
			rb2d.velocity = new Vector2(
				PlayerController.runSpeed * Mathf.Sign(input.HorizontalInput()),
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
		player.CancelStun();
		StartAttackStance();
	}

	void EndTechWindow() {
		techLockout = true;
		this.WaitAndExecute(() => techLockout = false, techLockoutLength);
	}

	void Shoot() {
		if (player.frozeInputs && currentGraph==null) return;

		if (input.ButtonDown(Buttons.PROJECTILE) && currentEP.Get() > 0) {
			animator.SetBool("WhiteEyes", true);
			this.WaitAndExecute(() => animator.SetBool("WhiteEyes", false), 2);
			gunEyes.Fire();
			LoseEnergy(1);
		}
	}

	public void OnHit(AttackHitbox attack) {
		// sideways DI is stronger than towards/away
		// (sin(2x - (1/4 circle))) * 0.4 + 0.6
		// ↑ this is a sinewave between 0.2 and 1.0 that peaks at (1, 0) and (-1, 0)
		Vector2 selfKnockback = player.GetKnockback(attack);
		Vector2 leftStick = input.LeftStick();
		float angle = Vector2.SignedAngle(selfKnockback, leftStick);
		float diMagnitude = (Mathf.Cos(angle * Mathf.Deg2Rad)* 0.4f) + 0.6f;
		rb2d.velocity += leftStick * diMagnitude * diStrength;
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
		player.OnAttackGraphEnter();
		currentGraph = graph;
		StartAttackStance();
		graph.EnterGraph(entryNode);
	}

	public void OnAttackNodeEnter(CombatNode combatNode) {
		if (combatNode is AttackNode) {
			AttackNode attackNode = combatNode as AttackNode;
			player.OnAttackNodeEnter(attackNode.attackData);
			attackHitbox.data = attackNode.attackData;
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

	public void GainEnergy() {
		bool wasFull = currentEP.Get()==maxEP.Get();
		currentEP.Set(Mathf.Min(maxEP.Get(), currentEP.Get()+1));
		if (currentEP.Get()==maxEP.Get() && !wasFull) {
			chargeIndicator.SetActive(false);
			chargeIndicator.GetComponentInChildren<Text>().text = fullMessage;
			chargeIndicator.SetActive(true);
			fullChargeSound.PlayFrom(gameObject);
		}
	}

	public void LoseEnergy(int amount) {
		bool wasEmpty = currentEP.Get()==0;
		currentEP.Set(Mathf.Max(0, currentEP.Get()-amount));
		if (currentEP.Get()==0 && !wasEmpty) {
			chargeIndicator.SetActive(false);
			chargeIndicator.GetComponentInChildren<Text>().text = emptyMessage;
			chargeIndicator.SetActive(true);
			emptyChargeSound.PlayFrom(gameObject);
		}
	}
}
