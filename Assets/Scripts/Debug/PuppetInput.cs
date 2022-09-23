using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;

public class PuppetInput : MonoBehaviour {
	public CustomController controller { get; private set; }
	Player player;

	HashSet<int> axes = new HashSet<int>();
	HashSet<int> buttons = new HashSet<int>();

	void Start() {
		int playerNum = GetComponent<PlayerInput>().playerNum;
		player = ReInput.players.GetPlayer(playerNum);
		if (controller == null) {
			controller = ReInput.controllers.CreateCustomController(1);
			player.controllers.AddController(controller, false);
		}
	}

	public void EnableInput() {
		if (player == null) Start();
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
			controller.SetButtonValueById(buttonId, false);
		}
	}
}
