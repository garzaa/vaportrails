using UnityEngine;
using System.Collections;

public class FreezeFrame : MonoBehaviour {

	public static FreezeFrame instance;

	static Coroutine freezeRoutine;

	static bool currentPriority = false;

	void Awake() {
		instance = this;
	}

	public static void Run(float seconds, bool priority=false) {
		if (currentPriority && !priority) return;
		Interrupt();
		currentPriority = priority;
		Time.timeScale = 0.01f;
		freezeRoutine = instance.StartCoroutine(EndFreeze(seconds));
	}

	static IEnumerator EndFreeze(float seconds) {
		yield return new WaitForSecondsRealtime(seconds);
		Time.timeScale = 1f;
		currentPriority = false;
	}

	public static void Interrupt() {
		if (freezeRoutine != null) instance.StopCoroutine(freezeRoutine);
		currentPriority = false;
		Time.timeScale = 1f;
	}
}
