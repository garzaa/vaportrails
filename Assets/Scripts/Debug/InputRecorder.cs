using UnityEngine;
using Rewired;
using System;
using System.IO;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

public class InputRecorder : MonoBehaviour {
	protected PlayerInput input;

	public bool recording { get; private set; }
	public GameObject recordingIndicator;

	protected Dictionary<int, int> actionIDAxes = new Dictionary<int, int>();
	protected HashSet<int> actionIDs = new HashSet<int>();
	protected List<FrameInput> frameInputs = new List<FrameInput>();

	HashSet<int> seenActions = new HashSet<int>();

	public const float pollInterval = 1f/12f;
	float lastPollTime;

	protected int recordingCounter = 0;

	void Start() {
		recordingIndicator.SetActive(false);
	}

	public void Arm(PlayerInput input) {
		this.input = input;
		seenActions.Clear();
	}

	public void StartRecording() {
		recordingCounter++;
		lastPollTime = Time.time;
		recordingIndicator.SetActive(true);
		recording = true;
		input.GetPlayer().AddInputEventDelegate(
			SaveInput,
			Rewired.UpdateLoopType.Update,
			InputActionEventType.AxisActive
		);
	}

	public void StopRecording() {
		if (!recording) {
			Terminal.Log("not currently recording, aborting");
			return;
		}
		recording = false;
		recordingIndicator.SetActive(false);
		input.GetPlayer().RemoveInputEventDelegate(SaveInput);
		SaveRecording();
	}

	protected virtual void SaveRecording() {
		string fileName = $"{Application.dataPath}/{input.name}_{recordingCounter}.json";
		File.WriteAllText(
			fileName,
			JsonConvert.SerializeObject(new Replay(frameInputs), Formatting.Indented)
		);
		Terminal.Log("Replay saved as "+fileName);
		frameInputs.Clear();
	}

	protected void Update() {
		if (Terminal.IsOpen()) return;
		if (recording && (Time.time > lastPollTime+pollInterval)) {
			SaveSnapshot();
			lastPollTime = Time.time;
		}
	}

	virtual protected void SaveInput(InputActionEventData e) {
		if (!seenActions.Contains(e.actionId)) {
			seenActions.Add(e.actionId);
			Debug.Log("saved new action: "+e.actionName);
		}
		if (ReInput.mapping.GetAction(e.actionId).type.Equals(InputActionType.Axis)) {
			actionIDAxes[e.actionId] = (int) Mathf.Sign(e.GetAxis());
		} else {
			if (e.GetButtonDown()) actionIDs.Add(e.actionId);
		}
	}

	virtual protected void SaveSnapshot() {
		// should save these to the object and then make them public for the GhostRecorder
		FrameInput snapshot = new FrameInput(actionIDAxes, actionIDs);
		frameInputs.Add(snapshot);
		actionIDAxes = new Dictionary<int, int>();
		actionIDs = new HashSet<int>();
	}
}
