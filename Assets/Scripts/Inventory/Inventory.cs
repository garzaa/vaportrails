using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : SavedObject {
	[SerializeField]
	List<StoredItem> items = new List<StoredItem>();

	//TODO: make this a hashset, wtf. why is it a list lol. oh right the stored items. huhh
	
	protected override void LoadFromProperties() {
		items = GetList<StoredItem>("items");
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["items"] = items;
	}

	public bool Has(Item item) {
		foreach (StoredItem i in items) {
			if (i.name == item.name) {
				return true;
			}
		}

		return false;
	}

	public bool Has(StoredItem item) {
		foreach (StoredItem i in items) {
			if (i.name == item.name && i.count >= item.count) {
				return true;
			}
		}

		return false;
	}

	public StoredItem GetItem(string itemName) {
        foreach (StoredItem i in items) {
            if (i.name.Equals(itemName)) {
                return i;
            }
        }
        return null;
    }

	public void AddItem(StoredItem item) {
		if (Has(item)) {
			foreach (StoredItem i in items) {
				if (i.name == item.name) {
					i.count += item.count;
				}
			}
		} else {
			items.Add(item);
		}
	}

	public void AddItem(Item item) {
		AddItem(new StoredItem(item.name, 1));
	}
}
