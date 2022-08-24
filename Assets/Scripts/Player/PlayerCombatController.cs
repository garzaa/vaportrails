using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombatController : MonoBehaviour, IAttackLandListener {
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

	public PlayerAttackGraph groundAttackGraph;
	public PlayerAttackGraph airAttackGraph;

	public SubscriptableInt currentEP;
	public SubscriptableInt maxEP;

	#pragma warning disable 0649
	[SerializeField] GameObject fullChargeIndicator;
	[SerializeField] AudioResource fullChargeSound;
	#pragma warning restore 0649

	bool canFlipKick = true;

	void Start() {
		player = GetComponent<PlayerController>();
		groundData = GetComponent<GroundCheck>().groundData;
		rb2d = GetComponent<Rigidbody2D>();
		wallData = GetComponent<WallCheck>().wallData;
		animator = GetComponent<Animator>();
		attackHitbox = GetComponentInChildren<AttackHitbox>();
		gunEyes = GetComponentInChildren<Gun>();
		targetingSystem = GetComponentInChildren<PlayerTargetingSystem>();
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
		currentEP.Initialize();
		currentEP.OnChange.AddListener(OnEnergyChange);
		maxEP.Initialize();
		fullChargeIndicator.SetActive(false);
	}

	public void OnAttackLand(Hurtbox hurtbox) {
		if (currentGraph) currentGraph.OnAttackLand();
	}

	public void OnEnergyChange(int energy) {
		targetingSystem.enabled = (energy > 0);
		if (energy == maxEP.Get()) {
			fullChargeIndicator.SetActive(false);
			fullChargeIndicator.SetActive(true);
			fullChargeSound.PlayFrom(gameObject);
		}
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
	}

	void Shoot() {
		if (player.frozeInputs && currentGraph==null) return;

		if (InputManager.ButtonDown(Buttons.PROJECTILE) && currentEP.Get() > 0) {
			animator.SetBool("WhiteEyes", true);
			this.WaitAndExecute(() => animator.SetBool("WhiteEyes", false), 2);
			gunEyes.Fire();
			LoseEnergy(1);
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
		if (currentEP.Get() != maxEP.Get()) {
			currentEP.Set(Mathf.Min(maxEP.Get(), currentEP.Get()+1));
		}
	}

	public void LoseEnergy(int amount) {
		currentEP.Set(Mathf.Max(0, currentEP.Get()-amount));
	}
}
