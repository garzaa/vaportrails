using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(ItemPane))]
public class InventoryUI : MonoBehaviour {
	public Inventory inventory;
	public ItemPane itemPaneTemplate;

	GameObject ui;
	Entity player;

	public GameObject paneContainer;
	public ScrollRect scrollRect;
	ItemPane selfItemPane;

	void Start() {
		player = PlayerInput.GetPlayerOneInput().GetComponent<Entity>();
		ui = transform.GetChild(0).gameObject;
		selfItemPane = GetComponent<ItemPane>();
		Debug.Log("inventory ui started");
	}

	public void Populate() {
		// re-render all the items
		// destroy all the old ones
		// this has to happen because we need a new reference to the list of transforms
		foreach (Transform oldItem in paneContainer.transform.Cast<Transform>().ToArray()) {
			// Destroy is called after the Update loop, which screws up the first child selection logic
            // so we do this so it's not shown
			Destroy(oldItem.gameObject);
			// keep the engine from reeing at you
			oldItem.SetParent(null, worldPositionStays: false);
		}

		foreach (Item item in inventory.GetItems()) {
			ItemPane pane = Instantiate(itemPaneTemplate, paneContainer.transform);
			pane.transform.SetAsLastSibling();
			pane.SetItem(item);
			ButtonExtras b = pane.GetComponent<ButtonExtras>();
			b.onHover.AddListener(() => ReactToItemClick(pane));
		}

		SelectFirstChild();
	}

	void SelectFirstChild() {
		if (paneContainer.transform.childCount == 0) return;

		Button b = paneContainer.transform.GetChild(0).GetComponent<Button>();
		b.Select();
		b.OnSelect(null);

		scrollRect.content.localPosition = Vector2.zero;
	}

	public void ReactToItemClick(ItemPane itemPane) {
		selfItemPane.SetItem(itemPane.GetItem());
	}

	public void LinkLeftRight(Button left, Button right) {
		
	}
}
