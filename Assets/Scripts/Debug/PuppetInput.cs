using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;

public class PuppetInput : MonoBehaviour {
	// https://guavaman.com/projects/rewired/docs/api-reference/html/T_Rewired_CustomController.htm
	// look at SetButtonValueById(? or without id is index, maybe that's the one? export constants whocares)
	// and SetAxisValue for axes
	// and then bobs yer uncle! well gotta translate button indices into button IDs
	// unless button IDs are shared across all controllers...in which case that would be poggers

	// then give it a method for playing a replay class


	CustomController controller;
	Player thisPlayer;

	HashSet<int> axes = new HashSet<int>();
	HashSet<int> buttons = new HashSet<int>();

	void Start() {
		int playerNum = GetComponent<PlayerInput>().playerNum;
		thisPlayer = ReInput.players.GetPlayer(playerNum);
	}

	public void EnableInput() {
		Terminal.Log(name + " ready for input");
		thisPlayer.controllers.AddController(ControllerType.Custom, 0, true);
		controller = thisPlayer.controllers.GetController<CustomController>(0);
		Debug.Log("current controller: "+controller);
	}

	public void SetAxis(int axisId, float val) {
		controller.SetAxisValueById(axisId, val);
		axes.Add(axisId);
	}

	public void SetButton(int buttonId) {
		controller.SetButtonValueById(buttonId, true);
		buttons.Add(buttonId);
	}

	public void ZeroInput() {
		foreach (int axisId in axes) {
			controller.SetAxisValueById(axisId, 0);
		}
		foreach (int buttonId in buttons) {
			controller.SetAxisValueById(buttonId, 0);
		}
	}
}
