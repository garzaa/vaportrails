using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using UnityEngine.Events;
using System.Linq;

public class ItemPane : MonoBehaviour {
	public List<Text> itemName;
	public Text itemDescription;
	public Image itemImage;
	public Text itemCount;
	public GameObject inputContainer;

	static GameObject inputTemplate = null;
	
	Item item = null;

	public UnityEvent OnSet;

	public void SetItem(Item i) {
		SetItem(i, 1);
	}

	public void SetItem(Item i, int count) {
		if (inputTemplate == null) {
			inputTemplate = Resources.Load<GameObject>("Runtime/InputTemplate");
		}

		this.item = i;
		foreach (Text t in itemName) {
			t.text = i.name;
		}
		if (itemDescription) itemDescription.text = i.GetDescription();
		if (itemImage) itemImage.sprite = i.icon;
		if (itemCount) itemCount.text = count != 1 ? count.ToString() : "";

		// if the item has move inputs, then append them to the move container
		if (inputContainer != null) {
			foreach (Transform child in inputContainer.transform.Cast<Transform>().ToArray()) {
				Destroy(child.gameObject);
				child.SetParent(null, worldPositionStays: false);
			}
			if (i.infoObject) {
				ItemMoveInputs moveInputs = i.infoObject.GetComponent<ItemMoveInputs>();
				if (moveInputs != null) {
					foreach (Sprite inputSprite in moveInputs.inputs) {
						GameObject g = Instantiate(inputTemplate, inputContainer.transform);
						g.transform.SetAsLastSibling();
						// it starts with the empty sprite
						if (inputSprite != null) g.GetComponent<Image>().sprite = inputSprite;
					}
				}
			}
		}

		OnSet.Invoke();
	}

	public Item GetItem() {
		return this.item;
	}
}
