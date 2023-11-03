using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMenuUI : MonoBehaviour {
	public GameObject ui;

	InventoryUI inventoryUI;
	PlayerInput input;
	Entity player;

	void Start() {
		inventoryUI = GetComponentInChildren<InventoryUI>();
		input = PlayerInput.GetPlayerOneInput();
		player = input.GetComponent<Entity>();
		ui.gameObject.SetActive(false);
	}

	void Update() {
		if (input.ButtonDown(RewiredConsts.Action.Inventory)) {
			if (ui.activeSelf) {
				ui.SetActive(false);
				player.ExitCutscene(this.gameObject);
			} else if (!player.inCutscene) {
				Open();
			}
		}
		else if (input.GenericEscapeInput() && ui.activeSelf) {
			ui.SetActive(false);
			player.ExitCutscene(this.gameObject);
		}
	}

	public void Open() {
		if (player.inCutscene) return;
		ui.SetActive(true);
		inventoryUI.Populate();
		player.EnterCutscene(this.gameObject);
	}
}
