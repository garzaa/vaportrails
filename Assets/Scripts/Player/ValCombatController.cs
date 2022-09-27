using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ValCombatController : CombatController {
	public SubscriptableInt currentEP;
	public SubscriptableInt maxEP;

	public AttackNode orcaFlipNode;

	Gun gunEyes;
	PlayerTargetingSystem targetingSystem;

	const float combatStanceLength = 4f;
	float combatLayerWeight = 0f;
	bool canFlipKick = true;

	#pragma warning disable 0649
	[SerializeField] GameObject chargeIndicator;
	[SerializeField] AudioResource fullChargeSound;
	[SerializeField] AudioResource emptyChargeSound;
	#pragma warning restore 0649

	const string fullMessage = "FULLY CHARGED";
	const string emptyMessage = "SOLENOID EMPTY";

	protected override void Start() {
		base.Start();
		gunEyes = GetComponentInChildren<Gun>();
		targetingSystem = GetComponentInChildren<PlayerTargetingSystem>();
		currentEP.Initialize();
		currentEP.OnChange.AddListener(OnEnergyChange);
		maxEP.Initialize();
		chargeIndicator.SetActive(false);
	}

	public void OnEnergyChange(int energy) {
		targetingSystem.enabled = (energy > 0);
	}

	protected override void Update() {
		base.Update();

		Shoot();
	
		if (combatLayerWeight == 0) {
			animator.SetLayerWeight(1, Mathf.MoveTowards(animator.GetLayerWeight(1), combatLayerWeight, 4*Time.deltaTime));
		}
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

	void FlipKick() {
		if (!groundData.grounded) {
			if (!canFlipKick) return;
			canFlipKick = false;
			player.DisableShortHop();
			rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(rb2d.velocity.y, player.movement.jumpSpeed));
			EnterAttackGraph(orcaFlipNode.graph as AttackGraph, orcaFlipNode);
		}
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

	public override void RefreshAirAttacks() {
		base.RefreshAirAttacks();
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

	override public void CheckAttackInputs() {
		base.CheckAttackInputs();
		if (
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

	protected override void OnTech() {
		base.OnTech();
		StartAttackStance();
	}

	public override void EnterAttackGraph(AttackGraph graph, CombatNode entryNode = null) {
		base.EnterAttackGraph(graph, entryNode);
		StartAttackStance();
	}

}
