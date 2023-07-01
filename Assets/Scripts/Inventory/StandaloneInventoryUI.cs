using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StandaloneInventoryUI : MonoBehaviour {
	// press I to toggle inventoiry (or tab?)
	Entity player;
	GameObject ui;
	InventoryUI inventoryUI;

	void Start() {
		player = PlayerInput.GetPlayerOneInput().GetComponent<Entity>();
		ui = transform.GetChild(0).gameObject;
		ui.SetActive(false);
		inventoryUI = GetComponent<InventoryUI>();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Tab)) {
			if (ui.activeSelf) {
				Debug.Log("player exiting ctuscene from tyhis");
				ui.SetActive(false);
				player.ExitCutscene(this.gameObject);
			} else if (!player.inCutscene) {
				Debug.Log("player enterunt ctuscene from tyhis");
				ui.SetActive(true);
				inventoryUI.Populate();
				player.EnterCutscene(this.gameObject);
			}
		}
	}
}
