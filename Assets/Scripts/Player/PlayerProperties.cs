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
		// if transition is empty, then it's a from-disk load
		// as opposed to a transition load
		bool transitionEmpty = FindObjectOfType<TransitionManager>().transition.IsEmpty();

		hp.SetCurrent(Get<int>("currentHP"));
		hp.SetMax(Get<int>("maxHP"));
		combatController.currentEP.Set(Get<int>("currentEP"));
		combatController.maxEP.Set(Get<int>("maxEP"));
		if (transitionEmpty) {
			transform.position = Get<Vector3>("pos");
		}
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
		// reload the player on the ground next time, if they're not already
		Vector3 pos = transform.position;
		if (!player.GetComponent<GroundCheck>().groundData.grounded) {
			float neutralDistance = GetComponent<CapsuleCollider2D>().bounds.extents.y;
			pos.y -= GetComponent<GroundCheck>().groundData.distance;
			pos.y += neutralDistance + 4f/64f;
		}
		properties["pos"] = pos;
		properties["facingRight"] = player.facingRight;
		properties["abilities"] = player.GetAbilities();
	}
}
