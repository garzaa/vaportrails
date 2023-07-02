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
	ScrollViewUtils scrollViewUtils;
	ItemPane selfItemPane;

	void Start() {
		player = PlayerInput.GetPlayerOneInput().GetComponent<Entity>();
		ui = transform.GetChild(0).gameObject;
		selfItemPane = GetComponent<ItemPane>();
		scrollViewUtils = scrollRect.GetComponent<ScrollViewUtils>();
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
			if (item.stackable) {
				AddPane(item, inventory.GetCount(item));
			} else {
				for (int i=0; i<inventory.GetCount(item); i++) {
					AddPane(item, 1);
				}
			}
		}

		SetGridNavigation(paneContainer.GetComponentsInChildren<Button>(), 5);

		SelectFirstChild();
	}

	void AddPane(Item item, int count) {
		ItemPane pane = Instantiate(itemPaneTemplate, paneContainer.transform);
		pane.transform.SetAsLastSibling();
		pane.SetItem(item, count);
		ButtonExtras b = pane.GetComponent<ButtonExtras>();
		b.onSelect.AddListener(() => ReactToItemClick(pane));
		b.onSelect.AddListener(() => scrollViewUtils.ScrollToChild(pane.GetComponent<RectTransform>()));
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

	void SetGridNavigation(Selectable[] buttons, int cols) {
		// navigation should wrap around between rows in left-right
		for (int i=0; i<buttons.Length; i++) {
			Navigation n = new Navigation();
			n.mode = Navigation.Mode.Explicit;
			if (i > cols) n.selectOnUp = buttons[i-cols];
			if (i > 0) n.selectOnLeft = buttons[i-1];
			if (i < buttons.Length-1) n.selectOnRight = buttons[i+1];
			if (i < buttons.Length - cols) n.selectOnDown = buttons[i+cols];
			buttons[i].navigation = n;
		}
	}
}
