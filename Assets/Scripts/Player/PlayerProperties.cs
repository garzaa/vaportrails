using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerProperties : SavedObject {
	EntityController player;
	ValCombatController combatController;
	HP hp;

	override protected void Initialize() {
		player = GetComponent<EntityController>();
		combatController = GetComponent<ValCombatController>();
		hp = GetComponent<HP>();
	}

	protected override void LoadFromProperties(Dictionary<string, object> properties) {

	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["currentHP"] = hp.GetCurrent();
		properties["maxHP"] = hp.GetMax();
		properties["currentEP"] = combatController.currentEP.Get();
		properties["maxEP"] = combatController.maxEP.Get();
	}
}
