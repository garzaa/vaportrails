using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;

[RequireComponent(typeof(PuppetInput))]
public class AIPlayer : MonoBehaviour {
	
	PuppetInput puppetInput;
	Player player;

	public Replay currentReplay { get; private set; }
	FrameInput currentInput;
	Controller puppetController;
	
	float startTime;
	int lastFrame;

	// action ID to button/axis ID
	Dictionary<int, int> buttonMaps = new Dictionary<int, int>();
	Dictionary<int, int> axisMaps = new Dictionary<int, int>();

	void Start() {
		puppetInput = GetComponent<PuppetInput>();
		if (!puppetInput) {
			puppetInput = gameObject.AddComponent<PuppetInput>();
		}
		player = ReInput.players.GetPlayer(GetComponent<PlayerInput>().playerNum);
	}

	public void PlayReplay(Replay replay) {
		if (!puppetInput) Start();
		puppetInput.EnableInput();
		puppetController = puppetInput.controller;
		startTime = Time.unscaledTime;
		currentReplay = replay;
		lastFrame = 0;
	}

	public void StopReplay() {
		currentReplay = null;
		puppetInput.ZeroInput();
	}

	void Update() {
		if (currentReplay != null) {
			int currentFrame = (int) ((Time.unscaledTime-startTime)/(InputRecorder.pollInterval));
			// this needs to happen every frame according to Rewired docs
			if (currentFrame<currentReplay.length) {
				puppetInput.ZeroInput();
				FrameInput inputs = currentReplay.frameInputs[currentFrame];

				foreach (KeyValuePair<int, int> input in inputs.actionIDAxes) {
					// if not in the map, then do above
					// otherwise just look it up
					AddAxisMap(input.Key);
					puppetInput.SetAxis(axisMaps[input.Key], input.Value);
				}
				foreach (int actionID in inputs.actionIDs) {
					AddButtonMap(actionID);
					puppetInput.SetButton(buttonMaps[actionID]);
				}
			}
			lastFrame = currentFrame;
			if (currentFrame == currentReplay.length-1) {
				Terminal.Log("Replay on "+gameObject.name+" finished, stopping and zeroing inputs");
				StopReplay();
			}
		}
	}

	void AddAxisMap(int actionID) {
		if (!axisMaps.ContainsKey(actionID)) {
			ActionElementMap map = player.controllers.maps.GetFirstAxisMapWithAction(
				puppetController,
				actionID,
				skipDisabledMaps: true
			);
			if (map == null) {
				Debug.Log($"action id {actionID} is registered but doesn't have a map!");
			}
			Debug.Log("adding lookup table entry from action id "+actionID+" to axis "+map.elementIdentifierName);
			axisMaps[actionID] = map.elementIdentifierId;
		}
	}

	void AddButtonMap(int actionID) {
		if (!buttonMaps.ContainsKey(actionID)) {
			ActionElementMap map = player.controllers.maps.GetFirstButtonMapWithAction(
				puppetController,
				actionID,
				skipDisabledMaps: true
			);
			Debug.Log("adding lookup table entry from action id "+actionID+" to button "+map.elementIdentifierName);
			buttonMaps[actionID] = map.elementIdentifierId;
		}
	}
}
