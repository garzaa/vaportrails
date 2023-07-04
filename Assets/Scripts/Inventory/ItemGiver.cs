using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemGiver : MonoBehaviour {
	public Item item;
	public int count = 1;
	public bool quiet;

	public void Give() {
		Inventory inv = PlayerInput.GetPlayerOneInput().GetComponentInChildren<Inventory>();
		inv.AddItem(item, count, quiet);
	}
}
