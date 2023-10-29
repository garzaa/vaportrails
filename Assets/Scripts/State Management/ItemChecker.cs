using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemChecker : MonoBehaviour {
	public Item item;
	public int count = 1;

	public List<Item> multiItems;

	public UnityEngine.Events.UnityEvent OnPass;
	public UnityEngine.Events.UnityEvent OnFail;

	public void Check() {
		Inventory inv = PlayerInput.GetPlayerOneInput().GetComponentInChildren<Inventory>();
		if (multiItems.Count > 0) {
			bool hasAll = true;
			foreach (Item item in multiItems) {
				if (!inv.Has(item)) {
					hasAll = false;
				}
			}
			if (hasAll) OnPass.Invoke();
			else OnFail.Invoke();
		} else {
			if (inv.Has(item) && inv.GetCount(item) >= count) OnPass.Invoke();
			else OnFail.Invoke();
		}
	}
}
