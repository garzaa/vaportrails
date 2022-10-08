using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnergyCostAttackHitbox : AttackHitbox {
	public int energyCost = 1;

	ValCombatController combatController;

	void Awake() {
		combatController = GetComponentInParent<ValCombatController>();
	}

	protected override bool CanHit(Hurtbox hurtbox) {
		return base.CanHit(hurtbox) && combatController.currentEP.Get() >= energyCost;
	}

	protected override void Hit(Hurtbox hurtbox, Collider2D other) {
		base.Hit(hurtbox, other);
		combatController.LoseEnergy(energyCost);
	}
}
