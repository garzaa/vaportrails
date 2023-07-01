using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityItem : ItemBehaviour {
	public Ability ability;

	public override void OnPickup(Item parentItem, Inventory inventory, bool silent) {
		inventory.GetComponentInParent<EntityController>().AddAbility(ability);
	}

	public override void OnRemove(Inventory inventory) {
		inventory.GetComponentInParent<EntityController>().RemoveAbility(ability);
	}
}
