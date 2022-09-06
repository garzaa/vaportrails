using UnityEngine;
using Rewired;
using System;
using System.IO;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

public class InputRecorder : MonoBehaviour {
	PlayerInput input;
	Player player;

	public bool recording { get; private set; }
	public GameObject recordingIndicator;

	Dictionary<int, int> actionIDAxes = new Dictionary<int, int>();
	HashSet<int> actionIDs = new HashSet<int>();
	List<FrameInput> frameInputs = new List<FrameInput>();

	// TODO: normalize input based on facing/not facing player (halve game state space)

	public const float pollInterval = 1f/12f;
	float lastPollTime;

	int recordingCounter = 0;

	void Start() {
		recordingIndicator.SetActive(false);
	}

	public void Arm(PlayerInput input) {
		this.input = input;
		player = input.GetPlayer();
	}

	public void StartRecording() {
		recordingCounter++;
		lastPollTime = Time.unscaledTime;
		recordingIndicator.SetActive(true);
		recording = true;
		input.GetPlayer().AddInputEventDelegate(SaveInput, Rewired.UpdateLoopType.Update);
	}

	public void StopRecording() {
		recording = false;
		recordingIndicator.SetActive(false);
		input.GetPlayer().RemoveInputEventDelegate(SaveInput);
		string fileName = $"{Application.dataPath}/{input.name}_{recordingCounter}.json";
		File.WriteAllText(
			fileName,
			JsonConvert.SerializeObject(new Replay(frameInputs), Formatting.Indented)
		);
		Terminal.Log("File saved as "+fileName);
		frameInputs.Clear();
	}

	void Update() {
		if (recording && (Time.unscaledTime > lastPollTime+pollInterval)) {
			SaveSnapshot();
			lastPollTime = Time.unscaledTime;
		}
	}

	void SaveInput(Rewired.InputActionEventData e) {
		if (Mathf.Abs(player.GetAxis(e.actionId)) > 0.3f) {
			Debug.Log("saving axis");
			actionIDAxes[e.actionId] = (int) Mathf.Sign(player.GetAxis(e.actionId));
		} else if (player.GetButtonDown(e.actionId)) {
			Debug.Log("saving button");
			actionIDs.Add(e.actionId);
		}
	}

	void SaveSnapshot() {
		FrameInput snapshot = new FrameInput(actionIDAxes, actionIDs.ToList());
		frameInputs.Add(snapshot);
		actionIDAxes = new Dictionary<int, int>();
		actionIDs = new HashSet<int>();
	}
}
