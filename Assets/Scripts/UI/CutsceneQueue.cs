using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CutsceneQueue : MonoBehaviour {
	Queue<Action> cutsceneQueue = new Queue<Action>();

	static CutsceneQueue instance;

	static Action currentCutscene = null;

	void Awake() {
		instance = this;
	}

	public static void Add(Action cutsceneAction) {
		if (currentCutscene == null) {
			currentCutscene = cutsceneAction;
			cutsceneAction.Invoke();
		} else {
			instance.cutsceneQueue.Enqueue(cutsceneAction);
		}
	}

	public static void OnCutsceneFinish() {
		currentCutscene = null;
		if (instance.cutsceneQueue.Count > 0) {
			currentCutscene = instance.cutsceneQueue.Dequeue();
			currentCutscene.Invoke();
		}
	}
}
