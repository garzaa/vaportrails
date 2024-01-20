using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FancyUnlock : ItemBehaviour {
	public override void OnPickup(Item parentItem, Inventory inventory, bool silent) {
		EntityController e = inventory.GetComponentInParent<EntityController>();
		if (!silent) {
			if (e.GetComponent<PlayerInput>().isHuman) {
				e.GetComponent<Animator>().Play("ValPickupAbility"); 
			}
			GameObject.FindObjectOfType<AbilityUnlockUI>().Show(parentItem);
		}
	}

	public override string GetDescription() {
		return "<color=#00cdf9ff>" + base.GetDescription() + "</color>";
	}
}
