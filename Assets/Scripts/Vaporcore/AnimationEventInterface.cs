using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class AnimationEventInterface : MonoBehaviour {
	
	public UnityEvent[] events;

	public void RaiseEvent(GameEvent e) {
		e.Raise();
	}

	public void RaiseChildSceneEvent(int index) {
		events[index].Invoke();
	}
}
