using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class EventOnEnable : MonoBehaviour {
	public UnityEvent enableEvent;

	void OnEnable() {
		StartCoroutine(_Execute());
	}

	IEnumerator _Execute() {
		yield return new WaitForEndOfFrame();
		enableEvent.Invoke();
	}
}
