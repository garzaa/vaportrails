using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/*
	this exists to mimic a rewired.customController but one per game object
	(instead of one per player, which is impossible if there are a lot of enemies reading input)
*/
public class ComputerController {
	Dictionary<int, float> axes = new Dictionary<int, float>();
	Dictionary<int, bool> buttons = new Dictionary<int, bool>();

	// int: action
	// bool: whether it's passed the 1-frame expiry deadline
	Dictionary<int, bool> bDowns = new Dictionary<int, bool>();
	Dictionary<int, bool> bUps = new Dictionary<int, bool>();

	// there's no guarantee for button-down flushing after it's been read
	// by input buffer that frame. so we do this instead of a flat clear
	// lateupdate means it stays false for a frame
	public void LateUpdate() {
		IncrementAndPrune(bDowns);
		IncrementAndPrune(bUps);
	}

	void IncrementAndPrune(Dictionary<int, bool> buttonEvents) {
		foreach (int id in buttonEvents.Keys.ToList()) {
			if (buttonEvents[id]) buttonEvents.Remove(id);
			else buttonEvents[id] = true;
		}
	}

	public void SetActionAxis(int actionID, float val) {
		axes[actionID] = val;
	}

	public void SetActionButton(int actionID) {
		SetActionButton(actionID, true);
	}

	public void SetActionButton(int actionID, bool b) {
		bool oldB = GetButton(actionID);
		if (b && !oldB) bDowns[actionID] = false;
		else if (!b && oldB) bUps[actionID] = false;

		buttons[actionID] = b;
	}

	public bool GetButton(int actionID) {
		if (buttons.ContainsKey(actionID)) return buttons[actionID];
		return false; 
	}

	public bool GetButtonDown(int actionID) {
		return bDowns.ContainsKey(actionID);
	}

	public bool GetButtonUp(int actionID) {
		return bUps.ContainsKey(actionID);
	}

	public float GetAxis(int actionID) {
		if (axes.ContainsKey(actionID)) return axes[actionID];
		return 0;
	}

	public void Zero() {
		foreach (int id in axes.Keys.ToList()) {
			axes[id] = 0;
		}

		foreach (int id in buttons.Keys.ToList()) {
			buttons[id] = false;
		}
	}
}
