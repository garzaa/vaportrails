using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackItem : ItemBehaviour {
	public override void OnPickup(Item parentItem, Inventory inventory, bool silent) {
		EntityController e = inventory.GetComponentInParent<EntityController>();
		if (!silent) {
			if (e.GetComponent<PlayerInput>().isHuman) {
				e.GetComponent<Animator>().Play("ValPickupAttack"); 
			}
			GameObject.FindObjectOfType<AttackUnlockUI>().Show(parentItem);
		}
	}

	public override bool HasCutscene() {
		return true;
	}
}
