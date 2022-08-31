using UnityEngine;
using Rewired;
using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class InputRecorder : MonoBehaviour {
	PlayerInput input;
	Player player;

	public bool recording { get; private set; }
	public GameObject recordingIndicator;
	float lastPollTime;

	Dictionary<int, int> axisValues = new Dictionary<int, int>();
	HashSet<int> buttons = new HashSet<int>();
	List<FrameInput> frameInputs = new List<FrameInput>();

	Coroutine snapshotRoutine;

	const float pollInterval = 1f/12f;

	int counter = 0;

	void Start() {
		recordingIndicator.SetActive(false);
	}

	public void Arm(PlayerInput input) {
		this.input = input;
		player = input.GetPlayer();
	}

	public void StartRecording() {
		counter++;
		lastPollTime = Time.unscaledTime;
		recordingIndicator.SetActive(true);
		input.GetPlayer().AddInputEventDelegate(SaveInput, Rewired.UpdateLoopType.Update);
		snapshotRoutine = StartCoroutine(SaveSnapshot());
	}

	public void StopRecording() {
		StopCoroutine(snapshotRoutine);
		recordingIndicator.SetActive(false);
		input.GetPlayer().RemoveInputEventDelegate(SaveInput);
		string fileName = $"{Application.dataPath}/{input.name} {counter}.json";
		Debug.Log(frameInputs.Count);
		File.WriteAllText(
			fileName,
			JsonUtility.ToJson(new FrameInputs(frameInputs), true)
		);
		Terminal.Log("File saved as "+fileName);
		frameInputs.Clear();
	}

	void SaveInput(Rewired.InputActionEventData e) {
		if (Mathf.Abs(player.GetAxis(e.actionId)) > 0.3f) {
			Debug.Log("saving axis");
			axisValues[e.actionId] = (int) Mathf.Sign(player.GetAxis(e.actionId));
		} else if (player.GetButtonDown(e.actionId)) {
			Debug.Log("saving button");
			buttons.Add(e.actionId);
		}
	}

	IEnumerator SaveSnapshot() {
		yield return new WaitForSecondsRealtime(pollInterval);
		FrameInput snapshot = new FrameInput(axisValues, buttons.ToList());
		frameInputs.Add(snapshot);
		axisValues.Clear();
		buttons.Clear();
		StartCoroutine(SaveSnapshot());
	}
}

[Serializable]
public struct FrameInput {
	public Dictionary<int, int> axisValues;
	public List<int> buttons;

	public FrameInput(Dictionary<int, int> d, List<int> l) {
		axisValues = d;
		buttons = l;
	}
}

public class FrameInputs {
	public List<FrameInput> frameInputs;

	public FrameInputs(List<FrameInput> inputs) {
		frameInputs = inputs;
	}
}
