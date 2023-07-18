using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnableOnItem : ItemChangeListener {
	public Item item;
	public bool disableOnItem;
	public bool log;

	Inventory inventory;

	void Awake() {
		inventory = PlayerInput.GetPlayerOneInput().GetComponentInChildren<Inventory>();
		CheckEnabled();
	}
	
	public override void OnItemAdd() {
		CheckEnabled();
	}

	public override void OnItemRemove() {
		CheckEnabled();
	}

	public void CheckEnabled() {
		// this can be called from the inventory as it loads, so call it here
		if (!inventory) {
			inventory = PlayerInput.GetPlayerOneInput().GetComponentInChildren<Inventory>();
		}

		if (log) Debug.Log("inventory has "+item+": "+inventory.Has(item));

		if (disableOnItem) {
			gameObject.SetActive(!inventory.Has(item));
		} else {
			gameObject.SetActive(inventory.Has(item));
		}
	}
}
