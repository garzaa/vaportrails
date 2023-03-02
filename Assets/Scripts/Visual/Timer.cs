using UnityEngine;
using UnityEngine.UI;
using System;
 
public class Timer : MonoBehaviour {
	public Text timerLabel;
	float time = 0f;

	bool paused = true;

	void Update() {
		if (paused) return;
		time += Time.deltaTime;
		timerLabel.text = FormattedTime(time);
	}

	public string FormattedTime(float t) {
		return TimeSpan.FromSeconds(t).ToString(@"mm\:ss\.ff");
	}

	public void Pause() {
		paused = true;
	}

	public void Restart() {
		time = 0;
		paused = false;
	}
}
