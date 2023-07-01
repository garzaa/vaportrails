using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;

public class ItemPane : MonoBehaviour {
	public List<Text> itemName;
	public Text itemDescription;
	public Image itemImage;
	public Text itemCount;
	
	Item item = null;

	public void SetItem(Item i) {
		SetItem(i, 1);
	}

	public void SetItem(Item i, int count) {
		this.item = i;
		foreach (Text t in itemName) {
			t.text = i.name;
		}
		if (itemDescription) itemDescription.text = i.GetDescription();
		if (itemImage) itemImage.sprite = i.icon;
		if (itemCount) itemCount.text = count != 1 ? count.ToString() : "";
	}

	public Item GetItem() {
		return this.item;
	}
}
