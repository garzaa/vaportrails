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

	HashSet<int> bDowns = new HashSet<int>();
	HashSet<int> bUps = new HashSet<int>();

	public void Update() {
		bDowns.Clear();
		bUps.Clear();
	}

	public void SetActionAxis(int actionID, float val) {
		axes[actionID] = val;
	}

	public void SetActionButton(int actionID) {
		SetActionButton(actionID, true);
	}

	public void SetActionButton(int actionID, bool b) {
		bool oldB = GetButton(actionID);
		if (b && !oldB) bDowns.Add(actionID);
		else if (!b && oldB) bUps.Add(actionID);

		buttons[actionID] = b;
	}

	public bool GetButton(int actionID) {
		if (buttons.ContainsKey(actionID)) return buttons[actionID];
		return false; 
	}

	public bool GetButtonDown(int actionID) {
		return bDowns.Contains(actionID);
	}

	public bool GetButtonUp(int actionID) {
		return bUps.Contains(actionID);
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
