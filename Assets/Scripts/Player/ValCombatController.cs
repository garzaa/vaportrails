using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Rewired;
using System.Collections.Generic;

public class ValCombatController : CombatController, IHitListener {
	public SubscriptableInt currentEP;
	public SubscriptableInt maxEP;

	Gun gunEyes;
	PlayerTargetingSystem targetingSystem;

	const float combatStanceLength = 4f;
	float combatLayerWeight = 0f;
	bool canFlipKick = true;
	bool parryActive = false;
	bool autoparry = false;

	#pragma warning disable 0649
	[SerializeField] AttackNode orcaFlipNode;
	[SerializeField] CombatNode groundOrcaFlipNode;
	[SerializeField] CombatNode airParryNode;
	[SerializeField] CombatNode groundParryNode;
	[SerializeField] CombatNode healNode;
	[SerializeField] GameObject chargeIndicator;
	[SerializeField] AudioResource fullChargeSound;
	[SerializeField] AudioResource emptyChargeSound;
	[SerializeField] GameObject parrySuccessEffect;
	[SerializeField] GameObject autoParryArm;
	#pragma warning restore 0649

	const string fullMessage = "FULLY CHARGED";
	const string emptyMessage = "SOLENOID EMPTY";

	const float autoparryDuration = 0.5f;
	CameraZoom cameraZoom;
	GameObject firstParryEffect;
	AudioResource poiseBreak;
	HP hp;

	protected override void Start() {
		base.Start();
		gunEyes = GetComponentInChildren<Gun>();
		targetingSystem = GetComponentInChildren<PlayerTargetingSystem>();
		currentEP.Initialize();
		currentEP.OnChange.AddListener(OnEnergyChange);
		maxEP.Initialize();
		chargeIndicator.SetActive(false);
		cameraZoom = GameObject.FindObjectOfType<CameraZoom>();
		poiseBreak = Resources.Load<AudioResource>("Runtime/PoiseBreak");
		hp = GetComponent<HP>();
	}

	public void OnEnergyChange(int energy) {
		targetingSystem.enabled = (energy > 0);
	}

	protected override void Update() {
		base.Update();

		Shoot();
		Parry();
		CheckHealInput();
	
		if (combatLayerWeight == 0) {
			animator.SetLayerWeight(1, Mathf.MoveTowards(animator.GetLayerWeight(1), combatLayerWeight, 4*Time.deltaTime));
		}

		targetingSystem.gameObject.SetActive(player.HasAbility(Ability.Projectile));
	}

	void CheckHealInput() {
		if (player.frozeInputs) return;
		if (
			input.ButtonDown(Buttons.SPECIAL)
			&& input.VerticalInput() < -0.4f
			&& groundData.grounded
			&& currentEP.Get() > 0
			&& hp.GetCurrent() < hp.GetMax()
			&& player.HasAbility(Ability.Heal)
		) {
			EnterAttackGraph(groundAttackGraph, healNode);
		}
	}

	public void HealFromAnimation() {
		// heal at most 8 HP per animation
		int toHeal = hp.GetMax() - hp.GetCurrent();
		toHeal = Mathf.Min(toHeal, 8);
		// don't overheal and get negative energy
		toHeal = Mathf.Min(toHeal, currentEP.Get());
		hp.AdjustCurrent(toHeal);
		currentEP.Set(currentEP.Get() - toHeal);
		chargeIndicator.SetActive(false);
		chargeIndicator.GetComponentInChildren<Text>().text = "WAVE REPAIR";
		chargeIndicator.SetActive(true);
		fullChargeSound.PlayFrom(this.gameObject);
	}

	void Parry() {
		if (player.frozeInputs) return;

		if (!player.HasAbility(Ability.Parry)) return;

		if (input.ButtonDown(Buttons.PARRY) && currentEP.Get() > 0) {
			if (groundData.grounded) {
				EnterAttackGraph(groundAttackGraph, groundParryNode);
			} else {
				EnterAttackGraph(airAttackGraph, airParryNode);
			}
		}
	}

	public bool CanBeHit(AttackHitbox attack) {
		return (attack is EnvironmentHitbox) || !(parryActive || autoparry);
	}

	public void OnHitCheck(AttackHitbox attack) {
		if (parryActive) {
			firstParryEffect = Instantiate(parrySuccessEffect, this.transform);
			parryActive = false;
			autoparry = true;
			Invoke(nameof(DisableAutoparry), autoparryDuration);
			cameraZoom.ZoomFor(2, 0.5f);
			FreezeFrame.Run(0.5f);
			graphTraverser.ExitGraph();
			// not a combat node, so it can be acted out of
			if (groundData.grounded) {
				animator.Play("Ground Parry Success", 0);
			} else {
				animator.Play("Air Parry Success", 0);
			}
		} else if (autoparry) {
			CancelInvoke(nameof(DisableAutoparry));
			Invoke(nameof(DisableAutoparry), autoparryDuration);
			GameObject g = Instantiate(autoParryArm, this.transform);
			g.transform.position = attack.GetComponent<Collider2D>().ClosestPoint(transform.position);
		}

		if (parryActive || autoparry) {
			// if insufficient energy
			// poise break, BUT parry the attack
			bool poiseBroke = false;
			if (currentEP.Get() < attack.data.GetDamage()) {
				player.StunFor(1f, 1f);
				player.DoHitstop(1f, Vector2.zero, priority: true);
				poiseBreak.PlayFrom(this.gameObject);
				if (!firstParryEffect) {
					firstParryEffect = Instantiate(parrySuccessEffect, this.transform.position, Quaternion.identity);
				}
				firstParryEffect.GetComponentInChildren<Text>().text = "POISE BREAK";
				shader.Flinch(Vector2.right, 1f);
				poiseBroke = true;
				// setting autoparry active now will let the incoming attack land
				// since this all happens on the attack probe
				StartCoroutine(WaitAndPoiseBreak());
				parryActive = false;
			}
			LoseEnergy(attack.data.GetDamage());
			if (poiseBroke) {
				// don't show the empty indicator on top of the poise break
				chargeIndicator.SetActive(false);
			} else {
				shader.FlinchOnce(player.GetKnockback(attack));
			}
		}
	}

	IEnumerator WaitAndPoiseBreak() {
		yield return new WaitForEndOfFrame();
		autoparry = false;
	}

	void StartParryWindow() {
		parryActive = true;
	}

	void EndParryWindow() {
		parryActive = false;
	}

	void DisableAutoparry() {
		autoparry = false;
	}

	void Shoot() {
		if (player.frozeInputs && !player.inAttack) return;

		if (!player.HasAbility(Ability.Projectile)) return;

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
			player.SetJustJumped();
		} else if (!player.inAttack) {
			EnterAttackGraph(groundOrcaFlipNode.graph as AttackGraph, groundOrcaFlipNode);
		}
	}

	void StartAttackStance() {
		combatLayerWeight = 1;
		animator.SetLayerWeight(1, 1);
		animator.SetFloat("AttackStance", 1);
		if (IsInvoking(nameof(DisableAttackStance))) {
			CancelInvoke(nameof(DisableAttackStance));
		}
		Invoke(nameof(DisableAttackStance), combatStanceLength);
	}

	public void DisableAttackStance() {
		combatLayerWeight = 0;
		animator.SetFloat("AttackStance", 0);
	}

	public override void RefreshAirAttacks() {
		canFlipKick = true;
	}

	// this is invoked from the basic attack hitbox
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
			(!player.frozeInputs || graphTraverser.InGraph())
			&& (input.ButtonDown(RewiredConsts.Action.Special) || input.ButtonDown(RewiredConsts.Action.Dash))
			&& !wallData.touchingWall
			&& player.HasAbility(Ability.FlipKick)
		) {
			// dash should take priority over everything else
			// then orca flip only if there's a sizeable up input
			if (input.VerticalInput() > 0.5) {
				FlipKick();
			}

			// then meteor only if there's barely any down input
		}
	}

	public override void OnTech() {
		base.OnTech();
		StartAttackStance();
	}

	public override void EnterAttackGraph(AttackGraph graph, CombatNode entryNode = null) {
		base.EnterAttackGraph(graph, entryNode);
		StartAttackStance();
	}

	public override void OnCombatNodeEnter(CombatNode combatNode) {
		base.OnCombatNodeEnter(combatNode);
	}

	public void OnDivekickLand() {
		player.RefreshAirMovement();
		player.SetJustJumped();
		animator.SetBool("DivekickRecoil", true);

		this.WaitAndExecute(() => animator.SetBool("DivekickRecoil", false), 0.2f);
	}

}
