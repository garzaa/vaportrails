using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class EventOnPlayerEnter : PlayerTriggeredObject {
	public UnityEvent OnEnter;
	public UnityEvent OnExit;

	protected override void OnPlayerEnter(Collider2D player) {
		OnEnter.Invoke();
	}

	protected override void OnPlayerExit(Collider2D player) {
		OnExit.Invoke();
	}
}
