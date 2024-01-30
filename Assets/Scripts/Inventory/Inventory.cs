using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : SavedObject {
	Dictionary<string, int> items = new Dictionary<string, int>();

	ItemChangeListener[] itemChangeListeners = null;

	Dictionary<string, Item> itemCache = new Dictionary<string, Item>();

	protected override void LoadFromProperties() {
		items = Get<Dictionary<string, int>>("items");
		CheckItemChangeListeners();
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["items"] = items;
	}

	public bool Has(Item item) {
		return items.ContainsKey(item.name);
	}

	public bool Has(Item item, int count) {
		return items.ContainsKey(item.name) && items[item.name] >= count;
	}

	public void AddItem(Item item) {
		AddItem(item, 1, false);
	}

	public void AddItem(Item item, int count, bool quiet) {

		if (Has(item)) {
			if (item.stackable) items[item.name] += count;
		} else {
			items[item.name] = count;
		}
		item.OnPickup(this, quiet);
		CheckItemChangeListeners();
	}

	public void AddItemsQuietly(List<Item> l) {
		// batch calls since this happens on scene load
		foreach (Item item in l) {
			if (Has(item)) {
				if (item.stackable) items[item.name] += 1;
			} else {
				items[item.name] = 1;
			}
			item.OnPickup(this, true);
		}
		CheckItemChangeListeners();
		Debug.Log("added items quietly");
	}

	void CheckItemChangeListeners() {
		if (GetComponentInParent<PlayerInput>()?.isHuman ?? false) {
			if (itemChangeListeners == null) itemChangeListeners = FindObjectsOfType<ItemChangeListener>(includeInactive: true);
			for (int i=0; i<itemChangeListeners.Length; i++) {
				itemChangeListeners[i].OnItemAdd();
			}
		}
	}

	public void RemoveItem(Item item) {
		RemoveItem(item, 1);
	}

	public void RemoveItem(Item item, int count) {
		if (Has(item)) {
			items[item.name] -= count;
			if (items[item.name] <= 0) {
				items.Remove(item.name);
			}
		}

		if (GetComponentInParent<PlayerInput>()?.isHuman ?? false) {
			for (int i=0; i<itemChangeListeners.Length; i++) {
				itemChangeListeners[i].OnItemRemove();
			}
		}
	}

	public int GetCount(Item item) {
		if (!items.ContainsKey(item.name)) return 0;
		else return items[item.name];
	}

	public List<Item> GetItems() {
		List<Item> x = new List<Item>();
		foreach (string itemName in items.Keys) {
			if (itemCache.ContainsKey(itemName)) {
				x.Add(itemCache[itemName]);
			} else {
				Item i = Resources.Load("Runtime/Items/"+itemName) as Item;
				itemCache[itemName] = i;
				x.Add(i);
			}
		}
		return x;
	}
}
