using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// TODO: save this as a hashset of enabled objects in the scene
// just, if this item's name is in the scene enabled hashset, then its children are enabled
// otherwise no
public class SavedEnabled : SavedObject {
	bool childrenActive = true;

	protected override void LoadFromProperties() {
		SetEnabled(Get<bool>("enabled"));
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["enabled"] = childrenActive;
	}

	public void Disable() {
		childrenActive = false;
		foreach (Transform t in transform) {
			t.gameObject.SetActive(false);
		}
	}

	public void Enable() {
		childrenActive = true;
		foreach (Transform t in transform) {
			t.gameObject.SetActive(true);
		}
	}

	public void SetEnabled(bool enabled) {
		if (enabled) Enable();
		else Disable();
	}
}
