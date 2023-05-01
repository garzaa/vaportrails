using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class EventOnEnable : MonoBehaviour {
	public UnityEvent enableEvent;

	void OnEnable() {
		enableEvent.Invoke();
	}
}
