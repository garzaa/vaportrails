using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class Utils {
	public static void WaitAndExecute(this MonoBehaviour mono, Action action, float seconds) {
		mono.StartCoroutine(_WaitAndExecute(action, seconds));
	}

	static IEnumerator _WaitAndExecute(Action action, float seconds) {
		yield return new WaitForSeconds(seconds);
		action();
	}
}
