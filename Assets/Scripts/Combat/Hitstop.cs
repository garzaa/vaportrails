using UnityEngine;
using System.Collections;

public class Hitstop : MonoBehaviour {

	public static Hitstop instance;

	static Coroutine currentHitstopRoutine;

	static bool currentPriority = false;

	void Awake() {
		instance = this;
	}

	public static void Run(float seconds, bool priority=false) {
		if (currentPriority && !priority) return;
		Interrupt();
		currentPriority = priority;
		Time.timeScale = 0.01f;
		currentHitstopRoutine = instance.StartCoroutine(EndHitstop(seconds));
	}

	static IEnumerator EndHitstop(float seconds) {
		yield return new WaitForSecondsRealtime(seconds);
		Time.timeScale = 1f;
		currentPriority = false;
	}

	public static void Interrupt() {
		if (currentHitstopRoutine != null) instance.StopCoroutine(currentHitstopRoutine);
		currentPriority = false;
		Time.timeScale = 1f;
	}
}
