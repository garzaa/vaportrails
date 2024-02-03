using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VaporPlasmaBehaviour : ItemBehaviour {
	// ok so on pickup
	// tell the heart UI to play the cutscene
	// health upgrade UI should be responsible for managing cutscene states 
	// and then actually upgradeing the health at the end
	public override void OnPickup(Item parentItem, Inventory inventory, bool silent) {
		if (!silent) {
			FindObjectOfType<HealthUpgradeUI>().PlayUnlock(inventory);
		}		
	}

	public override bool HasCutscene() {
		return true;
	}
}
