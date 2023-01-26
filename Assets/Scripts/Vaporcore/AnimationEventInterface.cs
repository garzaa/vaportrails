using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationEventInterface : MonoBehaviour {
	public void RaiseEvent(GameEvent e) {
		e.Raise();
	}
}
