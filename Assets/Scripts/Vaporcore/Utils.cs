using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Utils {
    static System.Random random = new System.Random();

	public static void WaitAndExecute(this MonoBehaviour mono, Action action, float seconds) {
		mono.StartCoroutine(_WaitAndExecute(action, seconds));
	}

	static IEnumerator _WaitAndExecute(Action action, float seconds) {
		yield return new WaitForSeconds(seconds);
		action();
	}

	public static T RandomDictValue<U, T>(IDictionary<U, T> dict) {
		List<T> values = Enumerable.ToList(dict.Values);
		return values[random.Next(values.Count)];
	}
}
