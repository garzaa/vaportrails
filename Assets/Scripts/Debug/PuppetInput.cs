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


	public CustomController controller { get; private set; }
	Player player;

	HashSet<int> axes = new HashSet<int>();
	HashSet<int> buttons = new HashSet<int>();

	// TODO: this is something per-button, does it need to be set up?
	// yeah take this out, unless it's possible to instantiate it for every action necessary
	// is there a way to get it for a specific controller?
	// do controller maps have to be added at runtime?
	ActionElementMap actionElementMap;

	void Start() {
		Debug.Log("puppet input initializing");
		int playerNum = GetComponent<PlayerInput>().playerNum;
		player = ReInput.players.GetPlayer(playerNum);
		Debug.Log("player number "+player.id);
	}

	public void EnableInput() {
		if (player == null) Start();
		player.controllers.AddController(ReInput.controllers.CustomControllers[0], true);
		controller = player.controllers.GetController<CustomController>(0);
		Debug.Log("added custom controller: "+controller.name);
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
