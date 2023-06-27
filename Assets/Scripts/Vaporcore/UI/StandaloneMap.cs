using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StandaloneMap : MonoBehaviour {
	Entity player;
	GameObject mapUI;

	void Start() {
		player = PlayerInput.GetPlayerOneInput().GetComponent<Entity>();
		mapUI = transform.GetChild(0).gameObject;
		mapUI.SetActive(false);
	}

	void Update() {
		if (Input.GetKeyUp(KeyCode.M)) {
			if (mapUI.activeSelf) {
				mapUI.SetActive(false);
			}
		} else if (Input.GetKeyDown(KeyCode.M) && !player.inCutscene) {
			mapUI.SetActive(true);
		}
	}
}
