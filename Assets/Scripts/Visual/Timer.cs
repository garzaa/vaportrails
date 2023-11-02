using UnityEngine;
using UnityEngine.UI;
using System;
 
public class Timer : MonoBehaviour {
	public Text timerLabel;
	public bool realTime = false;
	public bool useDecimals = true;
	float time = 0f;

	bool paused = true;

	void Update() {
		if (paused) return;
		time += realTime ? Time.unscaledDeltaTime : Time.deltaTime;
		timerLabel.text = FormattedTime(time);
	}

	public void ForceUpdate() {
		timerLabel.text = FormattedTime(time);
	}

	public string FormattedTime(float t) {
		if (!useDecimals) return TimeSpan.FromSeconds(t).ToString(@"mm\:ss");
		else return TimeSpan.FromSeconds(t).ToString(@"mm\:ss\.ff");
	}

	public void Pause() {
		paused = true;
	}

	public void Unpause() {
		paused = false;
	}

	public void Restart() {
		time = 0;
		paused = false;
	}

	public void SetTime(float t) {
		time = t;
	}

	public float GetTime() {
		return this.time;
	}
}
