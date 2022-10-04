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

	protected override void LoadFromProperties() {
		hp.SetCurrent(Get<int>("currentHP"));
		hp.SetMax(Get<int>("maxHP"));
		combatController.currentEP.Set(Get<int>("currentEP"));
		combatController.maxEP.Set(Get<int>("maxEP"));
		transform.position = new Vector3(
			Get<float>("posX"),
			Get<float>("posY"),
			Get<float>("posZ")
		);
		bool right = Get<bool>("facingRight");
		if ((right && !player.facingRight) || (!right && player.facingRight)) {
			player._Flip();
		}
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["currentHP"] = hp.GetCurrent();
		properties["maxHP"] = hp.GetMax();
		properties["currentEP"] = combatController.currentEP.Get();
		properties["maxEP"] = combatController.maxEP.Get();
		// avoid circular ref errors with normalization
		properties["posX"] = transform.position.x;
		properties["posY"] = transform.position.y;
		properties["posZ"] = transform.position.z;
		properties["facingRight"] = player.facingRight;
	}
}
