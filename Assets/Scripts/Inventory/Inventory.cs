using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : SavedObject {
	Dictionary<string, int> items = new Dictionary<string, int>();

	bool onPlayer = false;
	ItemChangeListener[] itemChangeListeners;

	Dictionary<string, Item> itemCache = new Dictionary<string, Item>();

	protected override void Initialize() {
		onPlayer = GetComponentInParent<PlayerInput>()?.isHuman ?? false;
		itemChangeListeners = FindObjectsOfType<ItemChangeListener>(includeInactive: true);
	}

	protected override void LoadFromProperties(bool startingUp) {
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
		item.OnPickup(this, quiet);

		if (Has(item)) {
			items[item.name] += count;
		} else {
			items[item.name] = count;
		}
		CheckItemChangeListeners();
	}

	public void AddItemsQuietly(List<Item> l) {
		// batch calls since this happens on scene load
		foreach (Item item in l) {
			item.OnPickup(this, true);

			if (Has(item)) {
				items[item.name] += 1;
			} else {
				items[item.name] = 1;
			}
		}
		CheckItemChangeListeners();
	}

	void CheckItemChangeListeners() {
		if (onPlayer) {
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

		if (onPlayer) {
			for (int i=0; i<itemChangeListeners.Length; i++) {
				itemChangeListeners[i].OnItemAdd();
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
