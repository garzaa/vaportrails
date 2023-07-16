using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CutsceneQueue : MonoBehaviour {
	Queue<Action> cutsceneQueue = new Queue<Action>();

	static CutsceneQueue instance;

	void Awake() {
		instance = this;
	}

	public static void Add(Action cutsceneAction) {
		if (instance.cutsceneQueue.Count == 0) {
			cutsceneAction.Invoke();
			return;
		}
		instance.cutsceneQueue.Enqueue(cutsceneAction);
	}

	public static void OnCutsceneFinish() {
		if (instance.cutsceneQueue.Count > 0) {
			instance.cutsceneQueue.Dequeue().Invoke();
		}
	}
}
