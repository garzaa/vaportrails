using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityPickup : MonoBehaviour, IPickup {
	public Ability ability;

	public void OnPickup(GameObject player, bool silent) {
		player.GetComponent<EntityController>().AddAbility(ability);

		// then if not silent, do some UI thing
	}
}
