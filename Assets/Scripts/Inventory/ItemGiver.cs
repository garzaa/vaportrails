using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemGiver : PlayerTriggeredObject {
	public Item item;

	protected override void OnPlayerEnter(Collider2D player) {
		// can be called from player hurtboxes or something
		if (player.GetComponentInChildren<Inventory>() != null) {
			player.GetComponentInChildren<Inventory>().AddItem(item);
		}
	}
}
