using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;

[RequireComponent(typeof(PuppetInput))]
public class AIPlayer : MonoBehaviour {
	
	PuppetInput puppetInput;
	Player player;

	Replay currentReplay;
	FrameInput currentInput;
	
	float startTime;
	int lastFrame;

	// convert from actionIDs to controller input buttons
	Dictionary<int, int> actionButtonIDMap = new Dictionary<int, int>();

	void Start() {
		puppetInput = GetComponent<PuppetInput>();
		player = ReInput.players.GetPlayer(GetComponent<PlayerInput>().playerNum);
	}

	public void PlayReplay(Replay replay) {
		startTime = Time.unscaledTime;
		currentReplay = replay;
		lastFrame = 0;
		puppetInput.EnableInput();
	}

	public void StopReplay() {
		currentReplay = null;
		puppetInput.ZeroInput();
	}

	public bool IsPlaying() {
		return currentReplay != null;
	}

	void Update() {
		if (IsPlaying()) {
			int currentFrame = (int) ((Time.unscaledTime-startTime)/(InputRecorder.pollInterval));
			if (currentFrame!=lastFrame && currentFrame<currentReplay.length) {
				puppetInput.ZeroInput();
				FrameInput inputs = currentReplay.frameInputs[currentFrame];

				foreach (KeyValuePair<int, int> input in inputs.axisValues) {
					// if not in the map, then do above
					// otherwise just look it up
					AddLookupEntryIfUnavailable(input.Key);
					puppetInput.SetAxis(actionButtonIDMap[input.Key], input.Value);
				}
				foreach (int actionID in inputs.buttons) {
					AddLookupEntryIfUnavailable(actionID);
					puppetInput.SetButton(actionButtonIDMap[actionID]);
				}
			}
			lastFrame = currentFrame;
			if (currentFrame == currentReplay.length-1) {
				Terminal.Log("Replay on "+gameObject.name+" finished, stopping and zeroing inputs");
				StopReplay();
			}
		}
	}

	void AddLookupEntryIfUnavailable(int actionID) {
		if (!actionButtonIDMap.ContainsKey(actionID)) {
			actionButtonIDMap[actionID] = player.controllers.maps.GetMap(0).GetFirstButtonMapWithAction(actionID).elementIdentifierId;
		}
	}
}
