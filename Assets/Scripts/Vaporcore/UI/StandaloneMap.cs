using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StandaloneMap : MonoBehaviour {
	Entity player;
	GameObject mapUI;
	PlayerInput input;

	public bool open => mapUI?.activeSelf ?? false;

	void Start() {
		player = PlayerInput.GetPlayerOneInput().GetComponent<Entity>();
		mapUI = transform.GetChild(0).gameObject;
		mapUI.SetActive(false);
		input = PlayerInput.GetPlayerOneInput();
	}

	void Update() {
		if (input.ButtonUp(RewiredConsts.Action.QuickMap)) {
			if (mapUI.activeSelf) {
				mapUI.SetActive(false);
			}
		} else if (input.ButtonDown(RewiredConsts.Action.QuickMap) && !player.inCutscene) {
			mapUI.SetActive(true);
		}
	}
}
