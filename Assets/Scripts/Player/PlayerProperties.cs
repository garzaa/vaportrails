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

	protected override void LoadFromProperties(bool startingUp) {
		hp.SetCurrent(Get<int>("currentHP"));
		hp.SetMax(Get<int>("maxHP"));
		combatController.currentEP.Set(Get<int>("currentEP"));
		combatController.maxEP.Set(Get<int>("maxEP"));
		if (!startingUp) transform.position = Get<Vector3>("pos");
		bool right = Get<bool>("facingRight");
		if ((right && !player.facingRight) || (!right && player.facingRight)) {
			player._Flip();
		}

		player.LoadAbilities(GetList<Ability>("abilities"));
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["currentHP"] = hp.GetCurrent();
		properties["maxHP"] = hp.GetMax();
		properties["currentEP"] = combatController.currentEP.Get();
		properties["maxEP"] = combatController.maxEP.Get();
		properties["pos"] = transform.position;
		properties["facingRight"] = player.facingRight;
		properties["abilities"] = player.GetAbilities();
	}
}
