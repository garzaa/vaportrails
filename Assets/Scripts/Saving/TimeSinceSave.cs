using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimeSinceSave : MonoBehaviour {
	Timer timer;

	void Start() {
		timer = GetComponent<Timer>();
	}

	public void OnSave() {
		timer.Restart();
	}
}
