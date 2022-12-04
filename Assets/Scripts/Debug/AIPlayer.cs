using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using System.Linq;

[RequireComponent(typeof(PuppetInput))]
public class AIPlayer : MonoBehaviour {
	
	PuppetInput puppetInput;
	Player player;

	public Replay currentReplay { get; private set; }
	FrameInput currentInput;
	Controller puppetController;

	Dictionary<int, List<WeightedFrameInput>> ghost;
	FrameInput lastGhostInput;
	GameObject opponent;
	float lastGhostInputTime = 0;
	GameSnapshotSaver snapshotSaver = new GameSnapshotSaver();
	
	float startTime;
	int lastFrame;

	const float reactionTime = 1f/4f;

	// action ID to button/axis ID
	Dictionary<int, int> buttonMaps = new Dictionary<int, int>();
	Dictionary<int, int> axisMaps = new Dictionary<int, int>();

	void Start() {
		puppetInput = GetComponent<PuppetInput>();
		if (!puppetInput) {
			puppetInput = gameObject.AddComponent<PuppetInput>();
		}
		player = ReInput.players.GetPlayer(GetComponent<PlayerInput>().playerNum);
		puppetInput.EnableInput();
		puppetController = puppetInput.controller;
	}

	public void PlayReplay(Replay replay) {
		Start();
		startTime = Time.unscaledTime;
		currentReplay = replay;
		lastFrame = 0;
	}

	public void StartGhost(Ghost ghost) {
		PlayerInput opponent = PlayerInput.GetPlayerOneInput();
		StartGhost(ghost.Load(), opponent.gameObject);
	}

	public void StartGhost(Ghostfile ghostFile, GameObject opponent) {
		Start();
		snapshotSaver.Initialize(this.gameObject, opponent);
		ghost = ghostFile.ghost;
		this.opponent = opponent;
	}

	public void StopReplay() {
		currentReplay = null;
		puppetInput.ZeroInput();
		lastFrame = 0;
	}

	public void StopGhost() {
		ghost = null;
		puppetInput.ZeroInput();
	}

	void Update() {
		if (currentReplay != null) {
			int currentFrame = (int) ((Time.unscaledTime-startTime)/(InputRecorder.pollInterval));
			// this needs to happen every frame according to Rewired docs
			if (currentFrame<currentReplay.length) {
				puppetInput.ZeroInput();
				FrameInput inputs = currentReplay.frameInputs[currentFrame];
				SetPuppetInput(inputs);
			}
			lastFrame = currentFrame;
			if (currentFrame == currentReplay.length-1) {
				Terminal.Log("Replay on "+gameObject.name+" finished, stopping and zeroing inputs");
				StopReplay();
			}
		} else if (ghost != null) {
			PlayGhost();
		}
	}

	void SetPuppetInput(FrameInput input) {
		foreach (KeyValuePair<int, int> IDAxis in input.actionIDAxes) {
			// if not in the map, then do above
			// otherwise just look it up
			AddAxisMap(IDAxis.Key);
			puppetInput.SetAxis(axisMaps[IDAxis.Key], IDAxis.Value);
		}
		foreach (int actionID in input.actionIDs) {
			AddButtonMap(actionID);
			puppetInput.SetButton(buttonMaps[actionID]);

			// if attack, attack towards player
			if (PlayerInput.IsAttack(actionID)) {
				AddAxisMap(RewiredConsts.Action.Horizontal);
				puppetInput.SetAxis(
					axisMaps[RewiredConsts.Action.Horizontal],
					Mathf.Sign(opponent.transform.position.x - transform.position.x)
				);
			}
		}
	}

	void SetNormalizedinput(FrameInput input) {
		if (input.actionIDAxes.ContainsKey(RewiredConsts.Action.Horizontal)
		&& opponent.transform.position.x < transform.position.x) {
			input.actionIDAxes[RewiredConsts.Action.Horizontal] *= -1;
		}
		SetPuppetInput(input);
	}

	void PlayGhost() {
		puppetInput.ZeroInput();
		if (Time.unscaledTime - reactionTime < lastGhostInputTime) {
			SetPuppetInput(lastGhostInput);
			return;
		}

		lastGhostInputTime = Time.unscaledTime;

		int gameHash = snapshotSaver.GetGameStateHash();
		FrameInput ghostInput;
		if (ghost.ContainsKey(gameHash)) {
			Debug.Log("input found");
			ghostInput = ChooseWeightedInput(ghost[gameHash]);
		} else {
			Debug.Log("No game hash, picking a random input");
			ghostInput = ChooseWeightedInput(Utils.RandomDictValue(ghost));
		}
		SetNormalizedinput(ghostInput);
		lastGhostInput = ghostInput;
	}

	public FrameInput ChooseWeightedInput(List<WeightedFrameInput> inputs) {
		float v = Random.value;
		foreach (WeightedFrameInput weightedInput in inputs) {
			v -= weightedInput.normalizedWeight;
			if (v < 0) {
				return weightedInput.frameInput;
			}
		}
		return inputs[inputs.Count-1].frameInput;
	}

	void AddAxisMap(int actionID) {
		if (!axisMaps.ContainsKey(actionID)) {
			ActionElementMap map = player.controllers.maps.GetFirstAxisMapWithAction(
				ControllerType.Custom,
				actionID,
				skipDisabledMaps: false
			);
			axisMaps[actionID] = map.elementIdentifierId;
		}
	}

	void AddButtonMap(int actionID) {
		if (!buttonMaps.ContainsKey(actionID)) {
			ActionElementMap map = player.controllers.maps.GetFirstButtonMapWithAction(
				ControllerType.Custom,
				actionID,
				skipDisabledMaps: false
			);
			buttonMaps[actionID] = map.elementIdentifierId;
		}
	}
}
