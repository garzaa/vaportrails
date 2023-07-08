using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StandaloneInventoryUI : MonoBehaviour {
	// press I to toggle inventoiry (or tab?)
	Entity player;
	GameObject ui;
	InventoryUI inventoryUI;
	PlayerInput input;

	void Start() {
		player = PlayerInput.GetPlayerOneInput().GetComponent<Entity>();
		input = PlayerInput.GetPlayerOneInput();
		ui = transform.GetChild(0).gameObject;
		ui.SetActive(false);
		inventoryUI = GetComponent<InventoryUI>();
	}

	void Update() {
		if (input.ButtonDown(RewiredConsts.Action.Inventory)) {
			if (ui.activeSelf) {
				ui.SetActive(false);
				player.ExitCutscene(this.gameObject);
			} else if (!player.inCutscene) {
				ui.SetActive(true);
				inventoryUI.Populate();
				player.EnterCutscene(this.gameObject);
			}
		}
	}
}
