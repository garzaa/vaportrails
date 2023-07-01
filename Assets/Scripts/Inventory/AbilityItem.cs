using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityItem : ItemBehaviour {
	public Ability ability;

	public override void OnPickup(Item parentItem, Inventory inventory, bool silent) {
		inventory.GetComponentInParent<EntityController>().AddAbility(ability);

		EntityController e = inventory.GetComponentInParent<EntityController>();
		if (!silent) {
			if (e.GetComponent<PlayerInput>().isHuman) {
				e.GetComponent<Animator>().Play("ValPickupAbility"); 
			}
			GameObject.FindObjectOfType<AbilityUnlockUI>().Show(parentItem);
		}
	}

	public override void OnRemove(Inventory inventory) {
		inventory.GetComponentInParent<EntityController>().RemoveAbility(ability);
	}

	public override string GetDescription() {
		return "<color=#00cdf9ff>" + base.GetDescription() + "</color>";
	}
}
