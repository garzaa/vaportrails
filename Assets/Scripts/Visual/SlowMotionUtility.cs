using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlowMotionUtility : MonoBehaviour {

	public void FreezeFor(float duration) {
		StartCoroutine(_SlowMotionFor(0.1f, duration));
	}

	public void SlowMotionFor(float duration) {
		StartCoroutine(_SlowMotionFor(0.5f, duration));
	}

	IEnumerator _SlowMotionFor(float scale, float duration) {
		Time.timeScale = scale;
		yield return new WaitForSecondsRealtime(duration);
		Time.timeScale = 1;
	}
}
