using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnableOnItem : ItemChangeListener {
	public Item item;
	public bool disableOnItem;

	Inventory inventory;

	void Start() {
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
		if (disableOnItem) {
			gameObject.SetActive(!inventory.Has(item));
		} else {
			gameObject.SetActive(inventory.Has(item));
		}
	}
}
