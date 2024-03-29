using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimeSinceSave : SavedObject {
	Timer timer;

	protected override void Initialize() {
		timer = GetComponent<Timer>();
	}

	protected override void LoadFromProperties() {
		timer.SetTime(Get<float>("time"));
		timer.Unpause();
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["time"] = timer.GetTime();
	}

	public void OnSave() {
		timer.Restart();
	}
}
