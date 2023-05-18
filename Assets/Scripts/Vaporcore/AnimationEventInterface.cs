using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class AnimationEventInterface : MonoBehaviour {
	
	public UnityEvent[] events;

	public void RaiseEvent(GameEvent e) {
		e.Raise();
	}

	public void RaiseSceneEvent(int index) {
		events[index].Invoke();
	}
}
