using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using System.Linq;
using System;

public class AIPlayer : MonoBehaviour {
	
	ComputerController comControl;
	PlayerInput playerInput;

	public Replay currentReplay { get; private set; }
	FrameInput currentInput;

	Dictionary<int, List<WeightedFrameInput>> ghost;
	FrameInput lastGhostInput;
	GameObject opponent;
	float lastGhostInputTime = 0;
	GameSnapshotSaver snapshotSaver = new GameSnapshotSaver();
	bool humanBeforeGhost = false;
	public bool logInputData = false;
	
	float startTime;
	int lastFrame;

	public float reactionTime = 1f/4f;

	void Start() {
		playerInput = GetComponent<PlayerInput>();
		comControl = playerInput.comControl;
	}

	public void PlayReplay(Replay replay) {
		playerInput.DisableHumanControl();
		startTime = Time.time;
		currentReplay = replay;
		lastFrame = 0;
	}

	public void StartGhost(Ghost ghost) {
		PlayerInput opponent = PlayerInput.GetPlayerOneInput();
		StartGhost(ghost.Load(), opponent.gameObject);
	}

	public void StartGhost(Ghostfile ghostFile, GameObject opponent) {
		// it needs to be initialized if it's added this frame at runtime
		if (playerInput == null) Start();
		if (playerInput.isHuman) humanBeforeGhost = true;
		playerInput.DisableHumanControl();
		snapshotSaver.Initialize(this.gameObject, opponent);
		ghost = ghostFile.ghost;
		this.opponent = opponent;
	}

	public void StopReplay() {
		currentReplay = null;
		comControl.Zero();
		if (humanBeforeGhost) playerInput.EnableHumanControl();
		lastFrame = 0;
	}

	public void StopGhost() {
		ghost = null;
		comControl.Zero();
		if (humanBeforeGhost) playerInput.EnableHumanControl();
	}

	void Update() {
		if (currentReplay != null) {
			int currentFrame = (int) ((Time.time-startTime)/(InputRecorder.pollInterval));
			// this needs to happen every frame according to Rewired docs
			if (currentFrame<currentReplay.length) {
				comControl.Zero();
				FrameInput inputs = currentReplay.frameInputs[currentFrame];
				SetInput(inputs);
			}
			lastFrame = currentFrame;
			if (currentFrame >= currentReplay.length-1) {
				Terminal.Log("Replay on "+gameObject.name+" finished, stopping and zeroing inputs");
				StopReplay();
			}
		} else if (ghost != null) {
			PlayGhost();
		}
	}

	void SetInput(FrameInput input) {
		if (input.actionIDAxes == null) {
			// then there's no input for this frame, just stay still
			return;
		}
		foreach (KeyValuePair<int, int> IDAxis in input.actionIDAxes) {
			comControl.SetActionAxis(IDAxis.Key, IDAxis.Value);
		}
		foreach (int actionID in input.actionIDs) {
			comControl.SetActionButton(actionID);

			// if attack, attack towards player
			if (PlayerInput.IsAttack(actionID)) {
				comControl.SetActionAxis(
					RewiredConsts.Action.Horizontal,
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
		SetInput(input);
	}

	void PlayGhost() {
		if (Time.time - reactionTime < lastGhostInputTime) {
			comControl.Zero();
			SetInput(lastGhostInput);
			return;
		}

		lastGhostInputTime = Time.time;

		int gameHash = snapshotSaver.GetGameStateHash();
		FrameInput ghostInput;
		if (ghost.ContainsKey(gameHash)) {
			if (logInputData) Debug.Log("input found");
			ghostInput = ChooseWeightedInput(ghost[gameHash]);
		} else {
			if (logInputData) Debug.Log("No game hash, picking a random input");
			ghostInput = ChooseWeightedInput(Utils.RandomDictValue(ghost));
		}
		SetNormalizedinput(ghostInput);
		lastGhostInput = ghostInput;
	}

	public FrameInput ChooseWeightedInput(List<WeightedFrameInput> inputs) {
		float v = UnityEngine.Random.value; 
		foreach (WeightedFrameInput weightedInput in inputs) {
			v -= weightedInput.normalizedWeight;
			if (v < 0) {
				return weightedInput.frameInput;
			}
		}
		return inputs[inputs.Count-1].frameInput;
	}
}
