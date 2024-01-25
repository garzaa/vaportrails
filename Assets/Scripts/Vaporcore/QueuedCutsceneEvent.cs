using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class QueuedCutsceneEvent : MonoBehaviour {
	public UnityEvent e;

	public void Invoke() {
		CutsceneQueue.Add(e.Invoke);
	}
}
