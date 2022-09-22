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
	}

	// TODO: have enough custom controllers so everyone gets one :~)
	// just get custom controllers[idx] from playerNum
	// or create one?
	// EnableInput with that looks like it can be called at start
	// unless it's being added via script, which it is...ah well
	// https://guavaman.com/projects/rewired/docs/api-reference/html/M_Rewired_ReInput_ControllerHelper_CreateCustomController.htm
	public void EnableInput() {
		if (player == null) Start();
		player.controllers.AddController(ReInput.controllers.CustomControllers[0], true);
		controller = player.controllers.GetController<CustomController>(0);
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
